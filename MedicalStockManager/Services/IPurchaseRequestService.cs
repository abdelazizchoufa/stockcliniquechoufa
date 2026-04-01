using MedicalStockManager.Models;

namespace MedicalStockManager.Services;

public interface IPurchaseRequestService
{
    PurchaseRequestIndexViewModel GetIndex(string? username, bool isApprover);
    PurchaseRequestCreateViewModel GetCreateModel();
    bool CreateRequest(PurchaseRequestCreateViewModel input, string username, out string? errorMessage);
    PurchaseRequestDetailsViewModel? GetDetails(int id, bool canApprove);
    bool ApproveRequest(ApproveRequestViewModel input, string username, out string? errorMessage);
    bool RejectRequest(RejectRequestViewModel input, string username, out string? errorMessage);
}
