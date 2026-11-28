using Orion.Application.DTOs.Modules;

namespace Orion.Application.Interfaces;

/// <summary>
/// Use cases for retrieving Orion's 4-module learning path.
/// </summary>
public interface ILearningModuleService
{
    Task<IReadOnlyList<LearningModuleDto>> GetActiveModulesAsync(
        CancellationToken cancellationToken = default);
}
