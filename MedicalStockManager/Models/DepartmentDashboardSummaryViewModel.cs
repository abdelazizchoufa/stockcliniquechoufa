namespace MedicalStockManager.Models;

public class DepartmentDashboardSummaryViewModel
{
    public string ServiceName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public int LowStockCount { get; set; }
    public int TotalQuantity { get; set; }
}
