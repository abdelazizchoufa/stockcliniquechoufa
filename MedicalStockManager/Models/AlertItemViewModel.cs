namespace MedicalStockManager.Models;

public class AlertItemViewModel
{
    public int StockItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "warning";
}
