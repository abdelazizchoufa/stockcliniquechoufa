namespace MedicalStockManager.Models;

public class SupplierDeleteViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public int OrderCount { get; set; }
    public bool CanDelete { get; set; }
    public string? BlockingReason { get; set; }
}
