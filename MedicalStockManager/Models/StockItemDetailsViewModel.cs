namespace MedicalStockManager.Models;

public class StockItemDetailsViewModel
{
    public StockItem Item { get; set; } = new();
    public IReadOnlyList<StockMovement> Movements { get; set; } = Array.Empty<StockMovement>();
    public StockMovementInputModel NewMovement { get; set; } = new();
    public bool CanEdit { get; set; }
}
