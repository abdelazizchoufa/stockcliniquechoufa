namespace MedicalStockManager.Models;

public class ExpirationItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public int DaysRemaining { get; set; }
    public string Severity { get; set; } = "warning";
}

public class ExpirationReportViewModel
{
    public IReadOnlyList<ExpirationItemViewModel> ExpiredItems { get; set; } = Array.Empty<ExpirationItemViewModel>();
    public IReadOnlyList<ExpirationItemViewModel> ExpiringIn7Days { get; set; } = Array.Empty<ExpirationItemViewModel>();
    public IReadOnlyList<ExpirationItemViewModel> ExpiringIn30Days { get; set; } = Array.Empty<ExpirationItemViewModel>();
    public IReadOnlyList<ExpirationItemViewModel> ExpiringIn90Days { get; set; } = Array.Empty<ExpirationItemViewModel>();
}
