using Orion.Domain.Entities;

namespace Orion.Application.DTOs.Modules;

/// <summary>
/// Public representation of an active Orion learning module in the 4-module curriculum path.
/// </summary>
public sealed record LearningModuleDto(
    string Slug,
    string Title,
    string Description,
    int OrderIndex)
{
    public static LearningModuleDto FromEntity(LearningModule module) =>
        new(
            Slug: module.Slug,
            Title: module.Title,
            Description: module.Description,
            OrderIndex: module.OrderIndex);
}
