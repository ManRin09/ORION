using FluentValidation;
using Orion.Application.DTOs.Analytics;
using Orion.Domain.Constants;

namespace Orion.Application.Validators;

/// <summary>
/// Validates incoming telemetry tracking requests against allowed event types and module slugs.
/// </summary>
public sealed class TrackEventRequestValidator : AbstractValidator<TrackEventRequest>
{
    public TrackEventRequestValidator()
    {
        RuleFor(x => x.VisitorId)
            .NotEmpty()
            .WithMessage("VisitorId is required.");

        RuleFor(x => x.EventType)
            .NotEmpty()
            .WithMessage("EventType is required.")
            .MaximumLength(50)
            .WithMessage("EventType cannot exceed 50 characters.")
            .Must(AnalyticsEventTypes.IsValid)
            .WithMessage($"EventType must be one of: {string.Join(", ", AnalyticsEventTypes.All)}.");

        RuleFor(x => x.ModuleSlug)
            .NotEmpty()
            .WithMessage("ModuleSlug is required for module_start and module_complete events.")
            .When(x => AnalyticsEventTypes.RequiresModuleSlug(x.EventType));

        RuleFor(x => x.ModuleSlug)
            .MaximumLength(30)
            .WithMessage("ModuleSlug cannot exceed 30 characters.")
            .Must(ModuleSlugs.IsValid)
            .WithMessage($"ModuleSlug must be one of: {string.Join(", ", ModuleSlugs.All)}.")
            .When(x => !string.IsNullOrWhiteSpace(x.ModuleSlug));

        RuleFor(x => x.Path)
            .NotEmpty()
            .WithMessage("Path is required.")
            .MaximumLength(255)
            .WithMessage("Path cannot exceed 255 characters.");
    }
}
