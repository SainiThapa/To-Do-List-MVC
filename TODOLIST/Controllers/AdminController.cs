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

        private readonly AccountService _accountService;

        public AdminController(AdminService adminService, TaskService taskService,AccountService accountService)
        {
            _adminService = adminService;
            _taskService = taskService;
            _accountService = accountService;
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

    }
}
