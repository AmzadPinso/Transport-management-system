using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Models.ViewModels;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly IBaseRepository<Role> _roleRepo;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(IUserRepo userRepo, IBaseRepository<Role> roleRepo)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _passwordHasher = new PasswordHasher<User>();
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _userRepo.GetByEmailAsync(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }

                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    Address = model.Address,
                    RoleId = 2, // Hardcoded "User" role ID from AppDbContext seed data
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Hash password
                user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

                await _userRepo.AddAsync(user);
                await _userRepo.SaveAsync();

                TempData["SuccessMessage"] = "Registration successful. Please login.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Account/Login
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userRepo.GetByEmailAsync(model.Email);

                if (user != null)
                {
                    // Verify password
                    var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

                    if (result == PasswordVerificationResult.Success)
                    {
                        // Update Last Login
                        user.LastLogIN = DateTime.Now;
                        _userRepo.Update(user);
                        await _userRepo.SaveAsync();

                        // Create Claims for Cookie Auth
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        // Redirect based on role
                        if (user.Role?.RoleName == "Admin")
                        {
                            return RedirectToAction("Index", "Users");
                        }
                        
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // GET: Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
