using System.ComponentModel.DataAnnotations;

namespace TODOLIST.ViewModels
{
    public class ProfileViewModel
    {
        public string Id { get; set; }

        [Display(Name = "First Name")]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Total Tasks")]
        public int TaskCount { get; set; }
    }
}
