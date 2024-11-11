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

        // [HttpGet("AspNetUsers")]
        // //Get : api/AccountApi/AspNetUsers
        // public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetAllAspNetUser()
        // {
        //     return await _context.AspNetUsers.ToListAsync();
        // }


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
        // PUT: api/AccountApi/user/{email}/updatePassword
        [HttpPut("AspNetUser/{userId}/updatePassword")]
        public async Task<IActionResult> UpdateUserPassword(string userId, [FromBody] UpdateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(userId);
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
    }

}