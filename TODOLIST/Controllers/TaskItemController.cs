using TODOLIST.Data;
using TODOLIST.Models;
using TODOLIST.ViewModels.APIViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace TODOLIST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]


    public class TaskItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskItemController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // GET: api/TaskItem/List
        [HttpGet("List")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTaskItems()
        {
            var userEmail = User.FindFirst("Email").Value;
            Console.WriteLine("======================================="+userEmail);
            // var user = await _userManager.FindByEmailAsync(userEmail);
            return await _context.TaskItems.Where(t => t.User.Email == userEmail)
                .ToListAsync();
        }

        // GET: api/TaskItem/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GetTaskItemViewModel>> GetTaskItem(int id)
        {
            var taskItem = await _context.TaskItems
                .Select(t => new GetTaskItemViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    IsActive = t.IsActive,
                    UserId = t.UserId
                })
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskItem == null)
            {
                return NotFound();
            }

            return taskItem;
        }

        // POST: api/TaskItem/create
        [HttpPost("create")]
        public async Task<ActionResult<GetTaskItemViewModel>> CreateTaskItem(CreateTaskItemViewModel model)
        {
            var userEmail = User.FindFirst("Email").Value;
            if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized("User email not found in the token.");
                }

            var user =await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return BadRequest("User does not exist.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var taskItem = new TaskItem
            {
                Title = model.Title,
                Description = model.Description,
                DueDate = model.DueDate,
                IsActive = model.IsActive,
                UserId = user.Id
            };

            _context.TaskItems.Add(taskItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaskItem), new { id = taskItem.Id }, taskItem);
        }

        // PUT: api/TaskItem/edit/{id}
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> UpdateTaskItem(int id, UpdateTaskItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }

            taskItem.Title = model.Title;
            taskItem.Description = model.Description;
            taskItem.DueDate = model.DueDate;
            taskItem.IsActive = model.IsActive;

            _context.Entry(taskItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/TaskItem/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteTaskItem(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }

            _context.TaskItems.Remove(taskItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
