namespace MedicalStockManager.Models;

public class DashboardViewModel
{
    public int TotalItems { get; set; }
    public int LowStockItems { get; set; }
    public int ExpiringSoonItems { get; set; }
    public IReadOnlyList<StockItem> ItemsByPriority { get; set; } = Array.Empty<StockItem>();
    public IReadOnlyList<AlertItemViewModel> LowStockAlerts { get; set; } = Array.Empty<AlertItemViewModel>();
    public IReadOnlyList<AlertItemViewModel> ExpirationAlerts { get; set; } = Array.Empty<AlertItemViewModel>();
    public IReadOnlyList<DepartmentDashboardSummaryViewModel> DepartmentSummaries { get; set; } = Array.Empty<DepartmentDashboardSummaryViewModel>();
    public IReadOnlyList<RecentMovementViewModel> RecentMovements { get; set; } = Array.Empty<RecentMovementViewModel>();
}
