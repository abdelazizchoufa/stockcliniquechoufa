namespace MedicalStockManager.Models;

public class SupplierDetailsViewModel
{
    public Supplier Supplier { get; set; } = new();
    public IReadOnlyList<PurchaseOrderListItemViewModel> Orders { get; set; } = Array.Empty<PurchaseOrderListItemViewModel>();
    public decimal TotalSpend { get; set; }
    public decimal AverageOrderAmount { get; set; }
    public int? AverageDeliveryDays { get; set; }
    public PurchaseOrderListItemViewModel? LastOrder { get; set; }
}
