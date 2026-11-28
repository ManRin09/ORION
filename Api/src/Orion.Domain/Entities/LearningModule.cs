using Orion.Domain.Constants;
using Orion.Domain.Exceptions;

namespace Orion.Domain.Entities;

/// <summary>
/// Represents one of the 4 structured learning modules in Orion's core curriculum path.
/// </summary>
public sealed class LearningModule
{
    private LearningModule()
    {
    }

    private LearningModule(
        Guid id,
        string slug,
        string title,
        string description,
        int orderIndex,
        bool isActive)
    {
        Id = id;
        Slug = slug;
        Title = title;
        Description = description;
        OrderIndex = orderIndex;
        IsActive = isActive;
    }

    public Guid Id { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int OrderIndex { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static LearningModule Create(
        Guid id,
        string slug,
        string title,
        string description,
        int orderIndex,
        bool isActive = true)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Learning module ID cannot be empty.");
        }

        if (!ModuleSlugs.IsValid(slug))
        {
            throw new DomainException($"Invalid module slug '{slug}'.");
        }

        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 100)
        {
            throw new DomainException("Module title is required and cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > 300)
        {
            throw new DomainException("Module description is required and cannot exceed 300 characters.");
        }

        if (orderIndex is < 1 or > 4)
        {
            throw new DomainException("Module orderIndex must be between 1 and 4.");
        }

        return new LearningModule(
            id: id,
            slug: ModuleSlugs.Normalize(slug),
            title: title.Trim(),
            description: description.Trim(),
            orderIndex: orderIndex,
            isActive: isActive);
    }
}
