using MedicalStockManager.Models;

namespace MedicalStockManager.Services;

public class InventorySessionListViewModel
{
    public IReadOnlyList<InventorySession> Sessions { get; set; } = Array.Empty<InventorySession>();
}

public class InventoryCreateViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<InventoryLineInput> Lines { get; set; } = new();
}

public class InventoryLineInput
{
    public int StockItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int TheoreticalQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int CountedQuantity { get; set; }
    public string? Notes { get; set; }
}

public interface IInventoryService
{
    InventorySessionListViewModel GetSessions();
    InventoryCreateViewModel GetCreateModel();
    bool CreateSession(InventoryCreateViewModel input, string username, bool applyAdjustments, out string? errorMessage);
    InventorySession? GetSessionDetails(int id);
}
