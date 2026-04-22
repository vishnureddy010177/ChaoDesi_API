using System.Security.Cryptography;
using System.Text;
using ChaoDesi.Application.Interfaces;

namespace ChaoDesi.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var hash = HashPassword(password);
        return hash == hashedPassword;
    }
}