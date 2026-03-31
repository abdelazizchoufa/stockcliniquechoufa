using MedicalStockManager.Models;

namespace MedicalStockManager.Services;

public interface IReportService
{
    ConsumptionReportViewModel GetConsumptionReport(DateTime? startDate, DateTime? endDate);
}
