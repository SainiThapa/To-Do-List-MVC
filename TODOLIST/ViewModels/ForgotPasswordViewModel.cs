using System.ComponentModel.DataAnnotations;
namespace TODOLIST.ViewModels{

    public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}

}