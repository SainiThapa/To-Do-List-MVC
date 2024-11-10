using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TODOLIST.Data;
using TODOLIST.Models;
using TODOLIST.ViewModels;

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

        public async Task<List<UserTasksSummaryViewModel>> GetUserTasksSummaryAsync()
        {
            var usersWithTasks = new List<UserTasksSummaryViewModel>();

            var users = _userManager.Users.Include(u => u.TaskItems).ToList();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("User")) // Filter to include only users with "User" role
                {
                    usersWithTasks.Add(new UserTasksSummaryViewModel
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = user.UserName,
                        Tasks = user.TaskItems.Select(t => new TaskViewModel 
                        {
                            Id = t.Id,
                            Title = t.Title,
                            Description = t.Description,
                            DueDate = t.DueDate,
                            IsActive = t.IsActive
                        }).ToList()
                    });
                }
            }
            return usersWithTasks;
        }

        public async Task<List<TaskWithOwnerViewModel>> GetAllTasksWithOwnerAsync()
        {
            return await _context.TaskItems
                .Include(t => t.User)  // Include the user to access owner details
                .Select(t => new TaskWithOwnerViewModel
                {
                    TaskId = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    IsActive = t.IsActive,
                    Owner_FullName = t.User.FirstName+" "+t.User.LastName,  // Assuming UserName is used for identification
                    OwnerEmail = t.User.Email
                }).ToListAsync();
        }
    }
}
