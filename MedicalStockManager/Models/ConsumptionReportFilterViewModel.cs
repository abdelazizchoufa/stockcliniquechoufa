using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class ConsumptionReportFilterViewModel
{
    [DataType(DataType.Date)]
    [Display(Name = "Date debut")]
    public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);

    [DataType(DataType.Date)]
    [Display(Name = "Date fin")]
    public DateTime EndDate { get; set; } = DateTime.Today;
}
