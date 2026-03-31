namespace MedicalStockManager.Models;

public class DepartmentConsumptionSummaryViewModel
{
    public Department Department { get; set; }
    public int TotalQuantityOut { get; set; }
    public int DistinctItems { get; set; }
}
