namespace MedicalStockManager.Models;

public class SupplierSpendViewModel
{
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int OrderCount { get; set; }
}
