namespace MedicalStockManager.Models;

public class MovementFilterViewModel
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public MovementType? MovementType { get; set; }
}

public class StockItemDetailsViewModel
{
    public StockItem Item { get; set; } = new();
    public IReadOnlyList<StockMovement> Movements { get; set; } = Array.Empty<StockMovement>();
    public StockMovementInputModel NewMovement { get; set; } = new();
    public bool CanEdit { get; set; }
    public MovementFilterViewModel Filter { get; set; } = new();
}
