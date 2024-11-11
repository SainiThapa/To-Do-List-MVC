using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TODOLIST.Services;
using TODOLIST.ViewModels;
using System.Linq;
using System.Globalization;
using CsvHelper;
using System.IO;

namespace TODOLIST.Controllers
{
    [Authorize(Roles = "Admin", Policy ="RequireCookie")]
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;
        private readonly TaskService _taskService;
        private readonly ILogger<AdminController> _logger;
        private readonly AccountService _accountService;

        public AdminController(AdminService adminService, TaskService taskService,AccountService accountService,ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _taskService = taskService;
            _accountService = accountService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index","Tasks");
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

        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                return RedirectToAction("ForgotPassword");
            }
            
            var model = new PasswordResetViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(PasswordResetViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Token))
                {
                    ModelState.AddModelError(string.Empty, "Invalid token.");
                    return View(model);
                }

                var result = await _accountService.ResetPasswordAsync(model);
                if (result.Succeeded)
                {
                    ViewData["Message"] = "Password reset successful!";
                    return RedirectToAction("Index","Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        public IActionResult ForgotPassword() => View();


        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var token = await _accountService.GeneratePasswordResetTokenAsync(model.Email);
                if (string.IsNullOrEmpty(token))
                {
                    // If the token is null, the email might not exist in the database
                    ModelState.AddModelError(string.Empty, "Invalid email address.");
                    return View(model);
                }
                return RedirectToAction("ResetPassword", new { token = token, email = model.Email });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadUserTasksSummary()
        {
            var usersTasks = await _adminService.GetUserTasksSummaryAsync();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csvWriter.WriteRecords(usersTasks);
            writer.Flush();
            stream.Position = 0;

            return File(stream, "text/csv", "UserTasksSummary.csv");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadAllTasksWithOwners()
        {
            var tasksWithOwners = await _adminService.GetAllTasksWithOwnerAsync();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csvWriter.WriteRecords(tasksWithOwners);
            writer.Flush();
            stream.Position = 0;

            return File(stream, "text/csv", "AllTasksWithOwners.csv");
        }

         // GET: Admin/AddUser
        public IActionResult AddUser()
        {
            return View();
        }

        // POST: Admin/AddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var result = await _accountService.RegisterUserAsync(model, User);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin created a new user account with email: {Email}", model.Email);
                    return RedirectToAction("UserList", "Admin");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while an admin was creating a new user.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            }

            return View(model);
        }

         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUsers(List<string> userIds)
        {
            if (userIds != null && userIds.Count > 0)
            {
                var result = await _accountService.DeleteUsersAsync(userIds);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Users deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error deleting users: " + result.Errors.FirstOrDefault()?.Description;
                }
            }
            return RedirectToAction("UserList");  // Redirect to the list of users after attempting deletion
        }
    }
}
