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
        private readonly TaskService _taskService;


        public AdminController(AdminService adminService, TaskService taskService)
        {
            _adminService = adminService;
            _taskService = taskService;
        }

        public IActionResult Index()
        {
            return View();
        }
        // List of users displays
        public async Task<IActionResult> UserList()
        {
            var users = await _adminService.GetAllUsersAsync();
            return View(users);
        }

// Show User's tasks or activities
        public async Task<IActionResult> UserTasks(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User ID is missing or invalid.");
            }
            var tasks = await _taskService.GetUserTasksAsync(userId);
            if (tasks == null || !tasks.Any())
            {
                return NotFound("No tasks found for this user.");
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
