using System.ComponentModel.DataAnnotations;

namespace TODOLIST.ViewModels
{
  public class PasswordResetViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; }

    [Required]
    public string Token { get; set; }
    }
}
