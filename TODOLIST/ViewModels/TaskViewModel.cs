using System;
using System.ComponentModel.DataAnnotations;

namespace TODOLIST.ViewModels
{
    public class TaskViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Task Title")]
        public string Title { get; set; }

        [Display(Name = "Task Description")]
        public string Description { get; set; }

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        [PastOrToday(ErrorMessage = "The due date cannot be in the past.")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Activity Status")]
        public bool IsActive { get; set; }
        public string Status => IsActive ? "Active" : "Completed";  

    }
    public class PastOrTodayAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime date)
            {
                return date >= DateTime.Today;
            }
            return true;
        }
}

}
