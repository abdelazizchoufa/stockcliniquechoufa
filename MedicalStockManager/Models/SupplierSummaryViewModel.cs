namespace MedicalStockManager.Models;

public class SupplierSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int OrderCount { get; set; }
    public DateTime? LastOrderDate { get; set; }
}
