using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Identity;

namespace OpenCBT.Infrastructure.Identity;

public class Argon2PasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class
{
    public string HashPassword(TUser user, string password)
    {
        return Argon2.Hash(password);
    }

    public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword))
        {
            return PasswordVerificationResult.Failed;
        }

        if (Argon2.Verify(hashedPassword, providedPassword))
        {
            return PasswordVerificationResult.Success;
        }

        // Graceful upgrade for legacy PBKDF2 hashes
        var defaultHasher = new PasswordHasher<TUser>();
        var pbkdf2Result = defaultHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        
        if (pbkdf2Result == PasswordVerificationResult.Success)
        {
            // The password is correct but uses PBKDF2. Tell Identity to rehash it!
            return PasswordVerificationResult.SuccessRehashNeeded;
        }

        return PasswordVerificationResult.Failed;
    }
}
