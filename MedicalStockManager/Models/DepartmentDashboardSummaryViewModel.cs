namespace MedicalStockManager.Models;

public class DepartmentDashboardSummaryViewModel
{
    public Department Department { get; set; }
    public int ItemCount { get; set; }
    public int LowStockCount { get; set; }
    public int TotalQuantity { get; set; }
}
