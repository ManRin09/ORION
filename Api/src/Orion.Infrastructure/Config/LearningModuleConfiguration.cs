using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orion.Domain.Constants;
using Orion.Domain.Entities;

namespace Orion.Infrastructure.Config;

/// <summary>
/// Fluent API configuration and initial seed for the PostgreSQL <c>learning_modules</c> table.
/// </summary>
public sealed class LearningModuleConfiguration : IEntityTypeConfiguration<LearningModule>
{
    public static readonly Guid PythonModuleId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    public static readonly Guid HtmlCssModuleId = Guid.Parse("11111111-1111-1111-1111-111111111102");
    public static readonly Guid JsModuleId = Guid.Parse("11111111-1111-1111-1111-111111111103");
    public static readonly Guid PostgresModuleId = Guid.Parse("11111111-1111-1111-1111-111111111104");

    public static IReadOnlyList<LearningModule> GetSeedModules() =>
    [
        LearningModule.Create(
            id: PythonModuleId,
            slug: ModuleSlugs.Python,
            title: "Python",
            description: "Master fundamental syntax, control structures, functions, and data manipulation in Python.",
            orderIndex: 1,
            isActive: true),
        LearningModule.Create(
            id: HtmlCssModuleId,
            slug: ModuleSlugs.HtmlCss,
            title: "HTML & CSS",
            description: "Build semantic web structures, responsive layouts, and 16-bit interfaces with HTML5 and CSS.",
            orderIndex: 2,
            isActive: true),
        LearningModule.Create(
            id: JsModuleId,
            slug: ModuleSlugs.Js,
            title: "JavaScript",
            description: "Conquer DOM manipulation, asynchronous runes, and interactive client-side logic with JavaScript.",
            orderIndex: 3,
            isActive: true),
        LearningModule.Create(
            id: PostgresModuleId,
            slug: ModuleSlugs.Postgres,
            title: "PostgreSQL",
            description: "Design relational schemas, write SQL queries, and defend data fortresses with PostgreSQL.",
            orderIndex: 4,
            isActive: true)
    ];

    public void Configure(EntityTypeBuilder<LearningModule> builder)
    {
        builder.ToTable("learning_modules");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.Slug)
            .HasColumnName("slug")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(m => m.Title)
            .HasColumnName("title")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasColumnName("description")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(m => m.OrderIndex)
            .HasColumnName("order_index")
            .IsRequired();

        builder.Property(m => m.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(m => m.Slug)
            .IsUnique()
            .HasDatabaseName("ix_learning_modules_slug");

        builder.HasIndex(m => m.OrderIndex)
            .HasDatabaseName("ix_learning_modules_order_index");

        builder.HasData(GetSeedModules());
    }
}
