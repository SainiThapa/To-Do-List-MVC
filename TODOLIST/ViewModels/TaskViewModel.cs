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
        public DateTime DueDate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
        public string Status => IsActive ? "Active" : "Completed";
    }
}
