using MedicalStockManager.Models;

namespace MedicalStockManager.Services;

public interface IAuthService
{
    AppUser? ValidateUser(string username, string password);
    string HashPassword(string password);
}
