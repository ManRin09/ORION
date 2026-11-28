using Microsoft.EntityFrameworkCore;
using Orion.Domain.Entities;
using Orion.Domain.Interfaces;
using Orion.Infrastructure.Persistence;

namespace Orion.Infrastructure.Repositories;

/// <summary>
/// EF Core repository implementation for querying <see cref="LearningModule"/> records.
/// </summary>
public sealed class LearningModuleRepository(AppDbContext dbContext) : ILearningModuleRepository
{
    public async Task<IReadOnlyList<LearningModule>> GetActiveOrderedAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.LearningModules
            .AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.OrderIndex)
            .ToListAsync(cancellationToken);
}
