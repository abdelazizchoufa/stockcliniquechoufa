namespace MedicalStockManager.Models;

public class StockItemDeleteViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public bool CanDelete { get; set; }
    public string? BlockingReason { get; set; }
}
