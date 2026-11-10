using Microsoft.AspNetCore.Identity;

namespace RaceDay.API.Services;

public class PasswordService
{
    private readonly PasswordHasher<string> _hasher = new();

    public string HashPassword(string password)
    {
        return _hasher.HashPassword("RaceDayUser", password);
    }

    public bool VerifyPassword(string hashedPassword, string password)
    {
        var result = _hasher.VerifyHashedPassword(
            "RaceDayUser",
            hashedPassword,
            password);

        return result != PasswordVerificationResult.Failed;
    }
}