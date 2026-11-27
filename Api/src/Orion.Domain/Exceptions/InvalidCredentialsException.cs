namespace Orion.Domain.Exceptions;

/// <summary>
/// Raised when adventurer login credentials or current password verification fail.
/// </summary>
public sealed class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("Invalid adventurer credentials.")
    {
    }

    public InvalidCredentialsException(string message)
        : base(message)
    {
    }
}
