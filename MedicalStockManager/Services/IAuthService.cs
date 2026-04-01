using MedicalStockManager.Models;

namespace MedicalStockManager.Services;

public interface IAuthService
{
    Task<AppUser?> ValidateUserAsync(string username, string password);
    string HashPassword(string password);
}
