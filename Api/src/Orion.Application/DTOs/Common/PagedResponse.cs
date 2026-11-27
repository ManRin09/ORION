namespace Orion.Application.DTOs.Common;

/// <summary>
/// Generic paginated result envelope for collection endpoints.
/// </summary>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
