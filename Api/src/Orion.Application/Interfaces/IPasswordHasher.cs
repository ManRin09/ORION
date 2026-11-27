namespace Orion.Application.Interfaces;

/// <summary>
/// Abstraction for hashing and verifying adventurer passwords securely.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
