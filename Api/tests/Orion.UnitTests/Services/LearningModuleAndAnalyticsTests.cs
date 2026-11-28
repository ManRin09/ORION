using FluentValidation;
using NSubstitute;
using Orion.Application.DTOs.Analytics;
using Orion.Application.Services;
using Orion.Application.Validators;
using Orion.Domain.Constants;
using Orion.Domain.Entities;
using Orion.Domain.Interfaces;
using Orion.Infrastructure.Config;

namespace Orion.UnitTests.Services;

public sealed class LearningModuleAndAnalyticsTests
{
    private readonly ILearningModuleRepository _moduleRepository = Substitute.For<ILearningModuleRepository>();
    private readonly IAnalyticsRepository _analyticsRepository = Substitute.For<IAnalyticsRepository>();
    private readonly LearningModuleService _moduleService;
    private readonly AnalyticsService _analyticsService;

    public LearningModuleAndAnalyticsTests()
    {
        _moduleService = new LearningModuleService(_moduleRepository);
        _analyticsService = new AnalyticsService(
            _analyticsRepository,
            _moduleRepository,
            new TrackEventRequestValidator());
    }

    [Theory]
    [InlineData("python", true)]
    [InlineData("html-css", true)]
    [InlineData("js", true)]
    [InlineData("postgres", true)]
    [InlineData("PYTHON", true)]
    [InlineData("rust", false)]
    [InlineData("", false)]
    public void ModuleSlugs_IsValid_ValidatesCanonicalSlugs(string slug, bool expected)
    {
        Assert.Equal(expected, ModuleSlugs.IsValid(slug));
    }

    [Theory]
    [InlineData("page_view", true)]
    [InlineData("module_start", true)]
    [InlineData("module_complete", true)]
    [InlineData("invalid_event", false)]
    public void AnalyticsEventTypes_IsValid_ValidatesSupportedEvents(string eventType, bool expected)
    {
        Assert.Equal(expected, AnalyticsEventTypes.IsValid(eventType));
    }

    [Fact]
    public async Task GetActiveModulesAsync_ReturnsFourOrderedCurriculumModules()
    {
        var seededModules = LearningModuleConfiguration.GetSeedModules();
        _moduleRepository.GetActiveOrderedAsync(Arg.Any<CancellationToken>())
            .Returns(seededModules);

        var result = await _moduleService.GetActiveModulesAsync();

        Assert.Equal(4, result.Count);
        Assert.Equal(ModuleSlugs.Python, result[0].Slug);
        Assert.Equal(1, result[0].OrderIndex);
        Assert.Equal(ModuleSlugs.HtmlCss, result[1].Slug);
        Assert.Equal(2, result[1].OrderIndex);
        Assert.Equal(ModuleSlugs.Js, result[2].Slug);
        Assert.Equal(3, result[2].OrderIndex);
        Assert.Equal(ModuleSlugs.Postgres, result[3].Slug);
        Assert.Equal(4, result[3].OrderIndex);
    }

    [Fact]
    public async Task TrackEventAsync_WithValidModuleStartEvent_PersistsNormalizedAnalyticsEvent()
    {
        var visitorId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        AnalyticsEvent? persisted = null;

        _analyticsRepository
            .When(r => r.AddAsync(Arg.Any<AnalyticsEvent>(), Arg.Any<CancellationToken>()))
            .Do(call => persisted = call.Arg<AnalyticsEvent>());

        var request = new TrackEventRequest(
            VisitorId: visitorId,
            EventType: "MODULE_START",
            ModuleSlug: "PYTHON",
            Path: "/dashboard/modules/python");

        await _analyticsService.TrackEventAsync(request, userId, "Mozilla/5.0 OrionTest");

        Assert.NotNull(persisted);
        Assert.Equal(visitorId, persisted.VisitorId);
        Assert.Equal(userId, persisted.UserId);
        Assert.Equal(AnalyticsEventTypes.ModuleStart, persisted.EventType);
        Assert.Equal(ModuleSlugs.Python, persisted.ModuleSlug);
        Assert.Equal("/dashboard/modules/python", persisted.Path);
        Assert.Equal("Mozilla/5.0 OrionTest", persisted.UserAgent);

        await _analyticsRepository.Received(1).AddAsync(Arg.Any<AnalyticsEvent>(), Arg.Any<CancellationToken>());
        await _analyticsRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TrackEventAsync_WhenModuleStartLacksModuleSlug_ThrowsValidationException()
    {
        var request = new TrackEventRequest(
            VisitorId: Guid.NewGuid(),
            EventType: AnalyticsEventTypes.ModuleStart,
            ModuleSlug: null,
            Path: "/dashboard");

        await Assert.ThrowsAsync<ValidationException>(() => _analyticsService.TrackEventAsync(request));
        await _analyticsRepository.DidNotReceive().AddAsync(Arg.Any<AnalyticsEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAdminSummaryAsync_AggregatesVisitorsUsersAndPerModuleCompletionRates()
    {
        var seededModules = LearningModuleConfiguration.GetSeedModules();
        _moduleRepository.GetActiveOrderedAsync(Arg.Any<CancellationToken>()).Returns(seededModules);

        _analyticsRepository.CountUniqueVisitorsSinceAsync(
                Arg.Is<DateTime>(d => d < DateTime.UtcNow.AddDays(-2)),
                Arg.Any<CancellationToken>())
            .Returns(120);

        _analyticsRepository.CountUniqueVisitorsSinceAsync(
                Arg.Is<DateTime>(d => d >= DateTime.UtcNow.AddDays(-2)),
                Arg.Any<CancellationToken>())
            .Returns(35);

        _analyticsRepository.CountTotalRegisteredUsersAsync(Arg.Any<CancellationToken>())
            .Returns(48);

        _analyticsRepository.GetModuleEngagementAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ModuleEngagementAggregate>
            {
                new(ModuleSlugs.Python, UniqueStarts: 40, UniqueCompletions: 30),
                new(ModuleSlugs.HtmlCss, UniqueStarts: 20, UniqueCompletions: 10)
            });

        var sampleUser = User.CreateLocal(
            firstName: "Juan",
            middleName: "Maldonado",
            email: "juanjosemn160@gmail.com",
            username: "juanjo2409",
            passwordHash: "hashed",
            acceptedTermsAtUtc: DateTime.UtcNow);

        _analyticsRepository.GetAllActiveUsersOrderedAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User> { sampleUser });

        _analyticsRepository.GetUserModuleActivityAsync(Arg.Any<CancellationToken>())
            .Returns(new List<UserModuleActivityAggregate>
            {
                new(
                    UserId: sampleUser.Id,
                    StartedModuleSlugs: [ModuleSlugs.Python, ModuleSlugs.HtmlCss],
                    CompletedModuleSlugs: [ModuleSlugs.Python],
                    LastActivityAtUtc: DateTime.UtcNow)
            });

        var summary = await _analyticsService.GetAdminSummaryAsync();

        Assert.Equal(120, summary.UniqueVisitorsLast30Days);
        Assert.Equal(35, summary.ActiveVisitorsLast24Hours);
        Assert.Equal(48, summary.TotalRegisteredUsers);
        Assert.Equal(4, summary.Modules.Count);

        var pythonStats = summary.Modules.Single(m => m.Slug == ModuleSlugs.Python);
        Assert.Equal(40, pythonStats.UniqueStarts);
        Assert.Equal(30, pythonStats.UniqueCompletions);
        Assert.Equal(75.0, pythonStats.CompletionRatePercent);

        var postgresStats = summary.Modules.Single(m => m.Slug == ModuleSlugs.Postgres);
        Assert.Equal(0, postgresStats.UniqueStarts);
        Assert.Equal(0, postgresStats.UniqueCompletions);
        Assert.Equal(0.0, postgresStats.CompletionRatePercent);

        Assert.Single(summary.UsersProgress);
        var userProgress = summary.UsersProgress[0];
        Assert.Equal("juanjo2409", userProgress.Username);
        Assert.Equal(2, userProgress.StartedModulesCount);
        Assert.Equal(1, userProgress.CompletedModulesCount);
        Assert.Equal(4, userProgress.TotalModulesCount);
        Assert.Equal(25.0, userProgress.OverallProgressPercent);
    }
}
