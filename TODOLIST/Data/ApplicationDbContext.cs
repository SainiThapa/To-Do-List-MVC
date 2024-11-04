using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TODOLIST.Models;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<TaskItem> TaskItems { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
}
