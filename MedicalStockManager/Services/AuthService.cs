using System.Security.Cryptography;
using System.Text;
using MedicalStockManager.Data;
using MedicalStockManager.Models;

namespace MedicalStockManager.Services;

public class AuthService(ApplicationDbContext dbContext) : IAuthService
{
    public AppUser? ValidateUser(string username, string password)
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();
        var passwordHash = HashPassword(password);

        return dbContext.AppUsers.FirstOrDefault(user =>
            user.IsActive &&
            user.Username.ToLower() == normalizedUsername &&
            user.PasswordHash == passwordHash);
    }

    public string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
