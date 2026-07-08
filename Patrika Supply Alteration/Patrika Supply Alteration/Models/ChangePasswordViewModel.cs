using System.ComponentModel.DataAnnotations;

namespace DCRSupplyApp.Models;

public class ChangePasswordViewModel
{
    public string? EmployeeCode { get; set; }

    [Required(ErrorMessage = "New password is required.")]
    [MinLength(4, ErrorMessage = "Password must be at least 4 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required.")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
}
