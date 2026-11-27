using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orion.Domain.Entities;
using Orion.Domain.Enums;

namespace Orion.Infrastructure.Config;

/// <summary>
/// Fluent API configuration for mapping <see cref="User"/> to the PostgreSQL <c>users</c> table.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", table =>
        {
            table.HasCheckConstraint(
                "ck_users_gamification_non_negative",
                "total_xp >= 0 AND current_level >= 1 AND current_streak >= 0 AND longest_streak >= 0 AND gems >= 0");
        });

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.MiddleName)
            .HasColumnName("middle_name")
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(500);

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255);

        builder.Property(u => u.AuthProvider)
            .HasColumnName("auth_provider")
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(AuthProvider.Local)
            .IsRequired();

        builder.Property(u => u.GitHubId)
            .HasColumnName("github_id")
            .HasMaxLength(100);

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(UserRole.User)
            .IsRequired();

        builder.Property(u => u.PlanType)
            .HasColumnName("plan_type")
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(PlanType.Free)
            .IsRequired();

        builder.Property(u => u.AcceptedTermsAt)
            .HasColumnName("accepted_terms_at")
            .IsRequired();

        builder.Property(u => u.TotalXp)
            .HasColumnName("total_xp")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.CurrentLevel)
            .HasColumnName("current_level")
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(u => u.CurrentStreak)
            .HasColumnName("current_streak")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.LongestStreak)
            .HasColumnName("longest_streak")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.Gems)
            .HasColumnName("gems")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("ix_users_username");

        builder.HasIndex(u => u.GitHubId)
            .IsUnique()
            .HasFilter("github_id IS NOT NULL")
            .HasDatabaseName("ix_users_github_id");

        // Enforce soft-delete across default queries
        builder.HasQueryFilter(u => u.IsActive);
    }
}
