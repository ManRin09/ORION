using Orion.Domain.Entities;

namespace Orion.Domain.Interfaces;

/// <summary>
/// Persistence contract for querying and seeding Orion's 4 core learning modules.
/// </summary>
public interface ILearningModuleRepository
{
    Task<IReadOnlyList<LearningModule>> GetActiveOrderedAsync(CancellationToken cancellationToken = default);
}
