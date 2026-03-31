namespace MedicalStockManager.Models;

public class ConsumptionReportViewModel
{
    public ConsumptionReportFilterViewModel Filter { get; set; } = new();
    public int TotalQuantityOut { get; set; }
    public int TotalMovements { get; set; }
    public IReadOnlyList<DepartmentConsumptionSummaryViewModel> DepartmentSummaries { get; set; } = Array.Empty<DepartmentConsumptionSummaryViewModel>();
    public IReadOnlyList<ItemConsumptionSummaryViewModel> TopItems { get; set; } = Array.Empty<ItemConsumptionSummaryViewModel>();
}
