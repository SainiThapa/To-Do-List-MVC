using System.ComponentModel.DataAnnotations;

namespace TODOLIST.ViewModels.APIViewModels
{
    public class GetTaskItemViewModel
    {
        public int Id { get; set; } // Primary Key
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Required]
        public bool IsActive { get; set; } // true for active, false for completed/inactive

        public string UserId { get; set; }
    }
    public class UpdateTaskItemViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Required]
        public bool IsActive { get; set; } // true for active, false for completed/inactive

    }
    public class CreateTaskItemViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Required]
        public bool IsActive { get; set; }
        // public string UserId { get; set; }
    }
}
