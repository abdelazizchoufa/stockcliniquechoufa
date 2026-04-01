using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class ServiceCreateInputViewModel
{
    [Required(ErrorMessage = "Le nom du service est obligatoire.")]
    [MaxLength(100)]
    [Display(Name = "Service")]
    public string Name { get; set; } = string.Empty;
}

public class ServiceListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

public class ServiceDirectoryViewModel
{
    public ServiceCreateInputViewModel NewService { get; set; } = new();
    public IReadOnlyList<ServiceListItemViewModel> Services { get; set; } = Array.Empty<ServiceListItemViewModel>();
}
