namespace Orion.Domain.Exceptions;

/// <summary>
/// Raised when attempting to assign a username that is already claimed by another adventurer.
/// </summary>
public sealed class UsernameAlreadyExistsException : DomainException
{
    public string Username { get; }

    public UsernameAlreadyExistsException(string username)
        : base($"The adventurer username '{username}' is already taken.")
    {
        Username = username;
    }
}
