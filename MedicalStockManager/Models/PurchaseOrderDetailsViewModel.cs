namespace MedicalStockManager.Models;

public class PurchaseOrderDetailsViewModel
{
    public PurchaseOrder Order { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public int TotalQuantity { get; set; }
}
