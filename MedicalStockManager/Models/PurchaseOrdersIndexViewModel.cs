namespace MedicalStockManager.Models;

public class PurchaseOrdersIndexViewModel
{
    public IReadOnlyList<PurchaseOrderListItemViewModel> Orders { get; set; } = Array.Empty<PurchaseOrderListItemViewModel>();
    public IReadOnlyList<SupplierSummaryViewModel> Suppliers { get; set; } = Array.Empty<SupplierSummaryViewModel>();
    public PurchaseFinanceSummaryViewModel FinanceSummary { get; set; } = new();
    public IReadOnlyList<SupplierSpendViewModel> TopSuppliers { get; set; } = Array.Empty<SupplierSpendViewModel>();
}
