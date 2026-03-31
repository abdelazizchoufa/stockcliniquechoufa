using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Nom d'utilisateur")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
