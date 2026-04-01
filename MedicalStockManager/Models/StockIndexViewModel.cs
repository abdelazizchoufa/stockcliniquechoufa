namespace MedicalStockManager.Models;

public class StockIndexViewModel
{
    public StockFilterViewModel Filter { get; set; } = new();
    public IReadOnlyList<StockItem> Items { get; set; } = Array.Empty<StockItem>();
    
    public int TotalCount { get; set; }
    public int TotalPages => Filter.PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)Filter.PageSize) : 0;
}
