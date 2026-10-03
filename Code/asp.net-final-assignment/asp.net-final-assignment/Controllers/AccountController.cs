using asp.net_final_assignment.Models;
using asp.net_final_assignment.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace asp.net_final_assignment.Controllers
{
    public class AccountController : Controller
    {
        // Managers
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserManager<AppUser> userManager;

        // Dependency Injection
        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }
         
        // Login
        [HttpGet]
        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var res = await signInManager.PasswordSignInAsync(model.Username!, model.Password!, false, false);
                if (res.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Username or Password is incorrect.");
            }
            return View();
        }

        // Register
        [HttpGet]
        [Route("register")]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new()
                {
                    Name = model.Email,
                    UserName = model.Email,
                    Email = model.Email,
                };

                var res = await userManager.CreateAsync(user, model.Password!);

                if (res.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var err in res.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            return View();
        }

        // Logout
        [HttpGet]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {                         
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
