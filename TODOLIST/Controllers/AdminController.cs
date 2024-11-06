using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TODOLIST.Services;
using TODOLIST.ViewModels;

namespace TODOLIST.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        private readonly TaskService _taskService;


        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        // List of users displays
        public async Task<IActionResult> UserList()
        {
            var users = await _adminService.GetAllUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> UserTasks(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID is null or empty.");

                return NotFound(); // Handle the case where userId is null
            }
            var tasks = await _taskService.GetUserTasksAsync(userId);
            if (tasks == null)
            {
                _logger.LogError($"No tasks found for user with ID {userId}");
                return NotFound(); // Handle the case where tasks are not found
            }

            ViewBag.UserId = userId;
            return View(tasks);
        }
        // Delete selected user tasks
        [HttpPost]
        public async Task<IActionResult> DeleteSelectedTasks(List<int> taskIds,string userId)
        {
            if (taskIds != null && taskIds.Count > 0)
            {
                foreach (var taskId in taskIds)
                {
                    await _taskService.DeleteTaskAsync(taskId, userId);
                }
                return RedirectToAction("UserTasks", new { userId = userId });
            }
            return RedirectToAction("UserTasks", new { userId = userId });
        }
    }
}
