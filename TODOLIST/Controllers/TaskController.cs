using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TODOLIST.Models;
using TODOLIST.Services;
using TODOLIST.ViewModels;

namespace TODOLIST.Controllers
{
    [Authorize] // Ensure that the user is authenticated before accessing the actions
    public class TasksController : Controller
    {
        private readonly TaskService _taskService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(TaskService taskService, UserManager<ApplicationUser> userManager)
        {
            _taskService = taskService;
            _userManager = userManager;
        }

        // GET: /Tasks/
        [HttpGet]
        public async Task<IActionResult> Index()
        {
                var user = await _userManager.GetUserAsync(User);
                var tasks = await _taskService.GetUserTasksAsync(user.Id);

                var taskViewModels = tasks.Select(task => new TaskViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsActive = task.IsActive
        }).ToList();
        return View(taskViewModels);
        
        }

        // POST: /Tasks/Create
        [HttpPost]
        public async Task<IActionResult> Create(TaskItem taskitem)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                taskitem.UserId = user.Id;  // Ensure the task is associated with the user
                await _taskService.CreateTaskAsync(taskitem, user.Id);
                return RedirectToAction("Index");
            }
            return View(taskitem);
        }

        // GET: /Tasks/Edit/{id}
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

        // POST: /Tasks/Edit/{id}
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

        // POST: /Tasks/Delete/{id}
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

        // GET: /Tasks/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var task = await _taskService.GetTaskByIdAsync(id, user.Id); // Ensure task belongs to the user
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }
    }
}
