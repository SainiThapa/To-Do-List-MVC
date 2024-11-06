using Microsoft.AspNetCore.Identity;

namespace TODOLIST.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();

    }
}
