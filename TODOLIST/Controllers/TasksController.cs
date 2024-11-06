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

// Create Tasks
        public IActionResult Create() => View();
        [HttpPost]
        public async Task<IActionResult> Create(TaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                 var taskItem = new TaskItem
                {
                    Title = model.Title,
                    Description = model.Description,
                    DueDate = model.DueDate,
                    IsActive = model.IsActive,
                    UserId = user.Id
                };
                await _taskService.CreateTaskAsync(taskItem, user.Id);
                return RedirectToAction("Index");
            }
            return View(model);
        }

// Edit Tasks   
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var task = await _taskService.GetTaskByIdAsync(id, user.Id);
            if (task == null)
            {
                return NotFound();
            }
            var taskviewmodel = new TaskViewModel
            {
                Id=task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                IsActive = task.IsActive
            };
            return View(taskviewmodel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var taskItem = new TaskItem
                {
                    Id = model.Id,
                    Title = model.Title,
                    Description = model.Description,
                    DueDate = model.DueDate,
                    IsActive = model.IsActive,
                    UserId = user.Id
                };
                var updatedTask = await _taskService.UpdateTaskAsync(taskItem, user.Id);
                if (updatedTask == null)
                {
                    return NotFound();
                }
                return RedirectToAction("Index");
            }

            return View(model);
        }
// Delete tasks
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var success = await _taskService.DeleteTaskAsync(id, user.Id);
            if (!success)
            {
                Console.WriteLine("Unsuccessful deletion");

                return NotFound();
            }
            return RedirectToAction("Index");
        }

// Get Details
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var task = await _taskService.GetTaskByIdAsync(id, user.Id); // Ensure task belongs to the user
            if (task == null)
            {
                return NotFound();
            }
             var taskViewModel = new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                IsActive = task.IsActive
            };      
            return View(taskViewModel);
        }
    }
}
