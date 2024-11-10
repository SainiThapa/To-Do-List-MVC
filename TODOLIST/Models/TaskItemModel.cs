using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TODOLIST.Models
{
    public class TaskItem
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

            [ForeignKey("UserId")]
            public ApplicationUser User { get; set; }
    }
}
