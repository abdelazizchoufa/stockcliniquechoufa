namespace MedicalStockManager.Models;

public class DepartmentConsumptionSummaryViewModel
{
    public string ServiceName { get; set; } = string.Empty;
    public int TotalQuantityOut { get; set; }
    public int DistinctItems { get; set; }
}
