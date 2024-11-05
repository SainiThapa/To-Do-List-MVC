using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TODOLIST.Models; 
using TODOLIST.Services;

namespace TODOLIST.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly TaskService _taskService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(TaskService taskService, UserManager<ApplicationUser> userManager)
        {
            _taskService = taskService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var tasks = await _taskService.GetUserTasksAsync(user.Id);
            return View(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskItem taskitem)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                await _taskService.CreateTaskAsync(taskitem, user.Id);
                return RedirectToAction("Index");
            }

            return View(taskitem);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var task = await _taskService.GetTaskByIdAsync(id, user.Id);
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TaskItem taskItem)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var updatedTask = await _taskService.UpdateTaskAsync(taskItem, user.Id);
                if (updatedTask == null)
                {
                    return NotFound();
                }
                return RedirectToAction("Index");
            }

            return View(taskItem);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var success = await _taskService.DeleteTaskAsync(id, user.Id);
            if (!success)
            {
                return NotFound();
            }
            return RedirectToAction("Index");
        }
    }
}
