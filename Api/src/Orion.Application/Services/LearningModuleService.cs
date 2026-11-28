using Orion.Application.DTOs.Modules;
using Orion.Application.Interfaces;
using Orion.Domain.Interfaces;

namespace Orion.Application.Services;

/// <summary>
/// Retrieves active learning modules ordered by curriculum sequence (1 to 4).
/// </summary>
public sealed class LearningModuleService(
    ILearningModuleRepository learningModuleRepository) : ILearningModuleService
{
    public async Task<IReadOnlyList<LearningModuleDto>> GetActiveModulesAsync(
        CancellationToken cancellationToken = default)
    {
        var modules = await learningModuleRepository.GetActiveOrderedAsync(cancellationToken);
        return modules
            .OrderBy(m => m.OrderIndex)
            .Select(LearningModuleDto.FromEntity)
            .ToList();
    }
}
