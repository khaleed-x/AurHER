using AurHER.DTOs.Admin;
using AurHER.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.RateLimiting;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;

namespace AurHER.Controllers
{
    [Authorize]//All actions in this class need authentication before they can be authorized
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var dashboard = await _adminService.GetDashboardStatsAsync();
            return View(dashboard);
        }

       
        [AllowAnonymous]//bypass the authentication to show the login form
        [HttpGet]
        public IActionResult Login()
        {
          
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard");

            return View();
        }

        
        [AllowAnonymous] //bypass the authentication to show the login form and send credentials for authentication
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _adminService.LoginAsync(model); 

            if (!result.Success)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Login failed");
                return View(model);   
            }

            // Create list of facts for the correct login info that'll be used for cookies
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, result.UserName!),
                new Claim(ClaimTypes.Role, result.Role!)
            };
            //
            var claimsIdentity = new ClaimsIdentity( // wraps  list of claims into a single object
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // IsPersistent = true — cookie survives browser close. If false, cookie disappears when browser closes ExpiresUtc — when the cookie expires and the admin gets logged out automatically
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            // Sign in — sets the authenticated  cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), // principal represents the actual person making the request, so this person owns the claim identity
                authProperties);

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            //delete cookies from browser
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
    }
}