using Microsoft.EntityFrameworkCore;
using Orion.Domain.Entities;
using Orion.Domain.Interfaces;
using Orion.Infrastructure.Persistence;

namespace Orion.Infrastructure.Repositories;

/// <summary>
/// EF Core repository implementation for <see cref="User"/> persistence.
/// </summary>
public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        var email = User.NormalizeEmail(normalizedEmail);
        return dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public Task<User?> GetByGitHubIdAsync(
        string gitHubId,
        CancellationToken cancellationToken = default)
    {
        var normalizedGitHubId = gitHubId.Trim();
        return dbContext.Users.FirstOrDefaultAsync(
            u => u.GitHubId == normalizedGitHubId,
            cancellationToken);
    }

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, pageNumber);
        var safeSize = Math.Clamp(pageSize, 1, 100);

        var query = dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .ThenBy(u => u.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> ExistsByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        var email = User.NormalizeEmail(normalizedEmail);
        // IgnoreQueryFilters ensures soft-deleted accounts still block duplicate unique emails in DB.
        return dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public Task<bool> ExistsByUsernameAsync(
        string normalizedUsername,
        CancellationToken cancellationToken = default)
    {
        var username = User.NormalizeUsername(normalizedUsername);
        return dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Username == username, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
