using MedicalStockManager.Models;

namespace MedicalStockManager.Services;

public class MaterialRequestFormViewModel
{
    public int Id { get; set; }
    public int RequestingServiceId { get; set; }
    public string? Notes { get; set; }
    public List<MaterialRequestLineFormViewModel> Lines { get; set; } = new();
}

public class MaterialRequestLineFormViewModel
{
    public int StockItemId { get; set; }
    public int RequestedQuantity { get; set; }
}

public interface IMaterialRequestService
{
    int CreateRequest(MaterialRequestFormViewModel input, string username);
    bool SubmitRequest(int requestId, out string? errorMessage);
    bool ApproveRequest(int requestId, string approverUsername, out string? errorMessage);
    bool RejectRequest(int requestId, string rejecterUsername, string reason, out string? errorMessage);
    
    MaterialRequest? GetRequestDetails(int id);
    IReadOnlyList<MaterialRequest> GetMyRequests(string username);
    IReadOnlyList<MaterialRequest> GetPendingRequests();
    IReadOnlyList<MaterialRequest> GetCompletedRequests();
}
