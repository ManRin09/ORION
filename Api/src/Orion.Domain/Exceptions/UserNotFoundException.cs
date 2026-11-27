namespace Orion.Domain.Exceptions;

/// <summary>
/// Raised when a requested user cannot be found or has been deactivated.
/// </summary>
public sealed class UserNotFoundException : DomainException
{
    public Guid UserId { get; }

    public UserNotFoundException(Guid userId)
        : base($"Adventurer with id '{userId}' was not found.")
    {
        UserId = userId;
    }
}
