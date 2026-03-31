using MedicalStockManager.Models;

namespace MedicalStockManager.Services;

public interface IPurchasingService
{
    PurchaseOrdersIndexViewModel GetOverview();
    PurchaseOrderCreateViewModel GetCreateModel();
    PurchaseOrderCreateViewModel PrepareCreateModel(PurchaseOrderCreateViewModel input);
    PurchaseOrderDetailsViewModel? GetDetails(int id);
    PurchaseOrderDetailsViewModel? GetPrintDetails(int id);
    SupplierFormViewModel GetSupplierCreateModel();
    SupplierFormViewModel? GetSupplierEditModel(int id);
    SupplierDeleteViewModel? GetSupplierDeleteModel(int id);
    bool AddSupplier(SupplierFormViewModel input, out string? errorMessage);
    bool UpdateSupplier(SupplierFormViewModel input, out string? errorMessage);
    bool DeleteSupplier(int id, out string? errorMessage);
    bool CreateOrder(PurchaseOrderCreateViewModel input, out string? errorMessage);
    bool ReceiveOrder(int id, out string? errorMessage);
}
