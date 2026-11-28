using Microsoft.EntityFrameworkCore;
using Orion.Domain.Entities;

namespace Orion.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for Orion backed by Neon PostgreSQL.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<LearningModule> LearningModules => Set<LearningModule>();
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
