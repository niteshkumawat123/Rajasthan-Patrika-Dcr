using System.ComponentModel.DataAnnotations;

namespace DCRSupplyApp.Models;

public class LoginViewModel
{
    [Required]
    public string EmployeeId { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    public string? RoleCode { get; set; }
    public string? RoleName { get; set; }
    public string? ErrorMessage { get; set; }
}
