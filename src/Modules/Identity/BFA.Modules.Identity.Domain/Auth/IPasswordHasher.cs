namespace BFA.Modules.Identity.Domain.Auth;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
