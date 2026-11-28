using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orion.Domain.Entities;

namespace Orion.Infrastructure.Config;

/// <summary>
/// Fluent API configuration for mapping <see cref="AnalyticsEvent"/> to the PostgreSQL <c>analytics_events</c> table.
/// </summary>
public sealed class AnalyticsEventConfiguration : IEntityTypeConfiguration<AnalyticsEvent>
{
    public void Configure(EntityTypeBuilder<AnalyticsEvent> builder)
    {
        builder.ToTable("analytics_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.VisitorId)
            .HasColumnName("visitor_id")
            .IsRequired();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ModuleSlug)
            .HasColumnName("module_slug")
            .HasMaxLength(30);

        builder.Property(e => e.Path)
            .HasColumnName("path")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(512);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("ix_analytics_events_created_at");

        builder.HasIndex(e => new { e.EventType, e.ModuleSlug })
            .HasDatabaseName("ix_analytics_events_event_type_module_slug");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
