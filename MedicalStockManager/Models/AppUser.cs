using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(40)]
    public string Role { get; set; } = AppRole.Lecture;

    public bool IsActive { get; set; } = true;
}
