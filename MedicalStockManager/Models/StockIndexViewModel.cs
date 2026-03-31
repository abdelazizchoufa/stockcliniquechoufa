namespace MedicalStockManager.Models;

public class StockIndexViewModel
{
    public StockFilterViewModel Filter { get; set; } = new();
    public IReadOnlyList<StockItem> Items { get; set; } = Array.Empty<StockItem>();
}
