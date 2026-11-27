using Orion.Infrastructure.Security;

namespace Orion.UnitTests.Security;

public sealed class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher = new();

    [Fact]
    public void HashAndVerify_WithMatchingPassword_ReturnsTrueAndGeneratesSaltedHash()
    {
        const string secretSpell = "Orion!Spell2026";

        var hash1 = _hasher.Hash(secretSpell);
        var hash2 = _hasher.Hash(secretSpell);

        Assert.NotEqual(secretSpell, hash1);
        Assert.NotEqual(hash1, hash2);
        Assert.True(_hasher.Verify(secretSpell, hash1));
        Assert.False(_hasher.Verify("Wrong!Spell2026", hash1));
    }
}
