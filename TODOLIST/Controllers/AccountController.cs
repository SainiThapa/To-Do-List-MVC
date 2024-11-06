using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TODOLIST.ViewModels;
using TODOLIST.Services;
using Microsoft.AspNetCore.Authorization;

namespace TODOLIST.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        private readonly AccountService _accountService;

        public AccountController(AccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;

        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
            try
            {
                var result = await _accountService.RegisterUserAsync(model);
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
                var result = await _accountService.LoginUserAsync(model);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        // [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutUserAsync();
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
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

        [Authorize]
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

                // Normally, you'd send this token to the user's email.
                // For simplicity, we're just displaying it here (in a real app, an email should be sent).
                return RedirectToAction("ResetPassword", new { token = token, email = model.Email });
            }
            return View(model);
        }
    }
}
