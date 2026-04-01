using MedicalStockManager.Data;
using MedicalStockManager.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalStockManager.Services;

public class AuthService(ApplicationDbContext dbContext) : IAuthService
{
    public async Task<AppUser?> ValidateUserAsync(string username, string password)
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();

        // Récupérer l'utilisateur par nom uniquement, puis vérifier le mot de passe côté C#
        // (BCrypt inclut le sel dans le hash — impossible de comparer en SQL)
        var user = await dbContext.AppUsers.FirstOrDefaultAsync(u =>
            u.IsActive &&
            u.Username.ToLower() == normalizedUsername);

        if (user is null) return null;

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
    }

    public string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
}
