using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TODOLIST.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TODOLIST.Services;
using TODOLIST.Data;
using TODOLIST.Models;
using TODOLIST.ViewModels.APIViewModels;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace TODOLIST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountApiController(UserManager<ApplicationUser> userManager,ApplicationDbContext context, 
        IConfiguration configuration, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }



        // POST: api/AccountApi/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if(existingUser!=null)
            {
                return BadRequest(new {message="A user with this email already exists"});
            }

            var user = new ApplicationUser { 
                UserName = model.Email, 
                Email = model.Email, 
                FirstName = model.FirstName, LastName = model.LastName };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await EnsureRoleExistsAsync("User");
                await _userManager.AddToRoleAsync(user,"User");
                return Ok(new { message = "Registration successful" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }
        
                public async Task EnsureRoleExistsAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // api/AccountApi/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLogin)
        {
            var validation = await IsValidUser(userLogin);
            Console.WriteLine(validation);
            if (validation)
            {
                Console.WriteLine("User login string : --------------"+userLogin.ToString());
                var token = GenerateJwtToken(userLogin.Email);
                return Ok(new { Token = token });
            }
            return Unauthorized();
        }

        // api/AccountApi/login
        [HttpPost("admin/login")]
        public async Task<IActionResult> AdminLogin([FromBody] UserLoginDto userLogin)
        {
            var validation = await IsValidUser(userLogin);
            if (validation)
            {
                var user = await _userManager.FindByEmailAsync(userLogin.Email);
                if (user == null || !(await _userManager.IsInRoleAsync(user, "Admin")))
                {
                    return Unauthorized("Access denied. Admins only.");
                }

                // Generate the token
                var token = GenerateJwtToken(userLogin.Email);

                return Ok(new
                {
                    Token = token });
            }

            return Unauthorized("Invalid login credentials.");
        }


        private async Task<bool> IsValidUser(UserLoginDto userLogin)
        {
                Console.WriteLine($"-----------{userLogin}-----------");
                var result = await _signInManager.PasswordSignInAsync(
                userLogin.Email,
                userLogin.Password,
                false,
                lockoutOnFailure: false);

            return result.Succeeded?true:false;
        }
        private string GenerateJwtToken(string Email)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("Email", Email),
                // new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Authorize]
        [HttpGet("AspNetUsers")]
        //Get : api/AccountApi/AspNetUsers
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetAllAspNetUser()
        {
            return await _context.AspNetUsers.ToListAsync();
        }

        [Authorize]
        // GET: api/AccountApi/AspNetUsers/{id}
        [HttpGet("AspNetUsers/{userId}")]
        public async Task<IActionResult> GetAspNetUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var userViewModel = new UserViewModel { Email = user.Email, FirstName = user.FirstName, LastName = user.LastName };
            return Ok(userViewModel);
        }

        [Authorize]
        [HttpGet("AspNetUsers/{userId}/details")]
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            var user = await _userManager.Users
                .Include(u => u.TaskItems) // Assuming TaskItems is the navigation property for tasks
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            var userDetails = new
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Tasks = user.TaskItems.Select(t => new
                {   
                    t.Id,
                    t.Title,
                    t.Description,
                    t.DueDate,
                    IsActive = t.IsActive ? "Active" : "Completed"
                }).ToList()
            };

            return Ok(userDetails);
        }

        //POST: api/AccountApi/AspNetUsers/{userId}/deleteTasks 
        [Authorize]
        [HttpPost("AspNetUsers/{userId}/deleteTasks")]
        public async Task<IActionResult> DeleteTasks(string userId, [FromBody] List<int> taskIds)
        {
                    Console.WriteLine($"User ID: {userId}");
                    Console.WriteLine($"Task IDs: {string.Join(", ", taskIds)}");
            var tasksToDelete = _context.TaskItems.Where(t => taskIds.Contains(t.Id) && t.UserId == userId).ToList();
            if (tasksToDelete.Count == 0)
                return NotFound("No tasks found to delete.");
            _context.TaskItems.RemoveRange(tasksToDelete);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{tasksToDelete.Count} tasks deleted successfully." });
        }


        [Authorize]
        // PUT: api/AccountApi/user/{userId}/updatePassword
        [HttpPut("AspNetUser/{email}/updatePassword")]
        public async Task<IActionResult> UpdateUserPassword(string email, [FromBody] UpdateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("%%%%%%%%%%%%%^&*((()))");
                return BadRequest(ModelState);
            }
            Console.WriteLine(model.Password.ToString());
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound();
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);

            if (result.Succeeded)
            {
                return Ok(new { message = "Password updated successfully" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return BadRequest(ModelState);
        }


        [Authorize]
        // DELETE: api/AccountApi/user/{email}
        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                return Ok(new { message = "User deleted successfully" });

            return BadRequest(result.Errors);
        }

        // api/AccountApi/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userEmail = User.FindFirst("Email").Value;
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
                return NotFound("User not found");

            var taskCount = await _context.TaskItems.CountAsync(t => t.UserId == user.Id);

            var profile = new UserProfileViewModel
            {
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                TaskCount = taskCount
            };

            return Ok(profile);
        }

        //AccountApi/Reports/UserSummary 
        [Authorize]
        [HttpGet("Reports/UserSummary")]
         public IActionResult GetUserSummary()
        {
            var users = _context.Users
                .Select(u => new { u.Email, u.FirstName, u.LastName, TaskCount = u.TaskItems.Count })
                .ToList();

            var csv = GenerateCsv(users);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "UserSummary.csv");
        }

        [Authorize]
        [HttpGet("Reports/TaskReport")]
        public IActionResult GetTaskReport()
        {
            var tasks = _context.TaskItems
                .Select(t => new {t.Id, t.Title, t.Description,t.DueDate,t.IsActive, t.User.Email })
                .ToList();

            var csv = GenerateCsv(tasks);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "TaskReport.csv");
        }

        private string GenerateCsv<T>(IEnumerable<T> data)
        {
            var properties = typeof(T).GetProperties();
            var csv = new StringBuilder();

            csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));
            foreach (var item in data)
            {
                csv.AppendLine(string.Join(",", properties.Select(p => p.GetValue(item)?.ToString())));
            }

            return csv.ToString();
        }
    }

}