using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TODOLIST.Data;
using TODOLIST.Models;
using TODOLIST.ViewModels;


namespace TODOLIST.Services
{
    public class TaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<TaskItem> CreateTaskAsync(TaskItem taskitem, string userId)
        {
            taskitem.UserId = userId;
            _context.TaskItems.Add(taskitem);
            await _context.SaveChangesAsync();
            return taskitem;
        }

        public async Task<List<TaskItem>> GetUserTasksAsync(string userId)
        {
            return await _context.TaskItems
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

               public async Task<TaskItem> GetTaskByIdAsync(int id, string userId)
        {
            return await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }

        public async Task<TaskItem> UpdateTaskAsync(TaskItem updatedTask, string userId)
        {
            var existingTask = await GetTaskByIdAsync(updatedTask.Id, userId);
            if (existingTask == null)
            {
                return null; // or throw an exception if preferred
            }

            existingTask.Title = updatedTask.Title;
            existingTask.Description = updatedTask.Description;
            existingTask.DueDate = updatedTask.DueDate;
            existingTask.IsActive = updatedTask.IsActive;

            await _context.SaveChangesAsync();
            return existingTask;
        }
        public async Task<TaskViewModel?> GetTaskByIdAsync(int id)
        {
            return await _context.TaskItems
                .Where(t => t.Id == id)
                .Select(t => new TaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    IsActive = t.IsActive
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteTaskAsync(int id, string userId)
        {
            var task = await GetTaskByIdAsync(id, userId);
            if (task == null)
            {
                return false; // Task not found or doesn't belong to the user
            }

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
            return true; // Successfully deleted
        }

    }
}
