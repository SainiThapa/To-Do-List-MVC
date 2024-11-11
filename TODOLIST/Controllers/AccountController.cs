using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TODOLIST.ViewModels;
using TODOLIST.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using TODOLIST.Models;
using NuGet.Protocol;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TODOLIST.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly AccountService _accountService;

        public AccountController(AccountService accountService, ILogger<AccountController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _accountService = accountService;
            _logger = logger;
            _userManager=userManager;
            _signInManager = signInManager;

        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
            try
            {
                var result = await _accountService.RegisterUserAsync(model,User);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        Console.WriteLine($"Error Code: {error.Code}, Description: {error.Description}");

                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                _logger.LogError(ex, "An error occurred during registration.");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
            }
        }
            return View(model);        
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // var result = await _accountService.LoginUserAsync(model);
                // if (result.Succeeded)
                // {
                //     return RedirectToAction("Index", "Home");
                // }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        claimsPrincipal);

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        // [HttpPost]
        [Authorize(Policy ="RequireCookie")]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutUserAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [Authorize(Policy ="RequireCookie")] // Make sure user is logged in
        public async Task<IActionResult> ResetPassword()
        {

            var user = await _userManager.GetUserAsync(User);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            if (token == null || user == null)
            {
                throw new ApplicationException("A token must be supplied for password reset.");
            }
            
            var model = new PasswordResetViewModel { Email=user.Email, Token = token };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy ="RequireCookie")]
        public async Task<IActionResult> ResetPassword(PasswordResetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(model);
            }
            var result = await _accountService.ResetPasswordAsync(model);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        [Authorize(Policy ="RequireCookie")]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}