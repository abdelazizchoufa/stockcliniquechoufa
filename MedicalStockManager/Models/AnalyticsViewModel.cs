namespace MedicalStockManager.Models;

public class MonthlyConsumptionViewModel
{
    public string Month { get; set; } = string.Empty;
    public int TotalOut { get; set; }
    public int TotalIn { get; set; }
}

public class TopConsumerViewModel
{
    public string ItemName { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int TotalConsumed { get; set; }
}

public class NeverMovedItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class AnalyticsViewModel
{
    public IReadOnlyList<MonthlyConsumptionViewModel> Last6Months { get; set; } = Array.Empty<MonthlyConsumptionViewModel>();
    public IReadOnlyList<TopConsumerViewModel> TopConsumers { get; set; } = Array.Empty<TopConsumerViewModel>();
    public IReadOnlyList<NeverMovedItemViewModel> NeverMovedItems { get; set; } = Array.Empty<NeverMovedItemViewModel>();
    public decimal TotalStockValue { get; set; }
    public int TotalMovementsThisMonth { get; set; }
    public decimal AverageDailyConsumption { get; set; }
}
