namespace MedicalStockManager.Models;

public class PurchaseFinanceSummaryViewModel
{
    public decimal TotalOrderedAmount { get; set; }
    public decimal TotalReceivedAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public int OrdersThisMonth { get; set; }
    public decimal AverageOrderAmount { get; set; }
}
