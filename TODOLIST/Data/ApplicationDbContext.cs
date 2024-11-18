using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TODOLIST.Models;
using TODOLIST.ViewModels;

namespace TODOLIST.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        { 
        }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TaskViewModel> TaskViewModel { get; set; } = default!;
        public DbSet<ApplicationUser> AspNetUsers{ get; set; }
    }
}