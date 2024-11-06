using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TODOLIST.Data;
using TODOLIST.Models;

namespace TODOLIST.Services
{
    public class AdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<ApplicationUser>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("User")) // Only add users with the "User" role
                {
                    userList.Add(user);
                }
            }
            return userList;
        }


        public async Task DeleteTasksAsync(List<int> taskIds)
        {
            var tasksToDelete = await _context.TaskItems
                .Where(task => taskIds.Contains(task.Id))
                .ToListAsync();

            _context.TaskItems.RemoveRange(tasksToDelete);
            await _context.SaveChangesAsync();
        }
    }
}
