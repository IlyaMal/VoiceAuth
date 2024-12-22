using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace VoiceAuth.Services;

public class EncryptService
{
    public static string HashPassword(string password, string salt)
    {
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(password,
            System.Text.Encoding.ASCII.GetBytes(salt),
            KeyDerivationPrf.HMACSHA512,
            5000,
            64));
    }
}