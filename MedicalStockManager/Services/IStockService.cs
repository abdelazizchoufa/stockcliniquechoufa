using MedicalStockManager.Models;

namespace MedicalStockManager.Services;

public interface IStockService
{
    StockIndexViewModel GetStockIndex(StockFilterViewModel filter);
    StockItem? GetItem(int id);
    StockItemDetailsViewModel? GetItemDetails(int id);
    StockItemFormViewModel GetCreateModel();
    StockItemFormViewModel? GetEditModel(int id);
    bool AddItem(StockItemFormViewModel item, out string? errorMessage);
    bool UpdateItem(StockItemFormViewModel item, out string? errorMessage);
    StockItemDeleteViewModel? GetDeleteModel(int id);
    bool DeleteItem(int id, out string? errorMessage);
    bool AddMovement(StockMovementInputModel input, out string? errorMessage);
    DashboardViewModel GetDashboard();
}
