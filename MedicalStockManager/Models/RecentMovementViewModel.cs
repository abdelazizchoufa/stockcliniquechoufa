namespace MedicalStockManager.Models;

public class RecentMovementViewModel
{
    public int StockItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public Department Department { get; set; }
    public MovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
}
