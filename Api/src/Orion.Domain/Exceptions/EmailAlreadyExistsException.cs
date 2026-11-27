namespace Orion.Domain.Exceptions;

/// <summary>
/// Raised when attempting to register an email address that is already taken.
/// </summary>
public sealed class EmailAlreadyExistsException : DomainException
{
    public string Email { get; }

    public EmailAlreadyExistsException(string email)
        : base($"An adventurer account with email '{email}' already exists.")
    {
        Email = email;
    }
}
