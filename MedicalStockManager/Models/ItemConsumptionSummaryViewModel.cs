namespace MedicalStockManager.Models;

public class ItemConsumptionSummaryViewModel
{
    public string ItemName { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int TotalQuantityOut { get; set; }
    public int MovementCount { get; set; }
}
