using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly PasswordHasher<User> _passwordHasher;

        public ProfileController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string userName, string email, string address)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return NotFound();

            user.UserName = userName;
            user.Email = email;
            user.Address = address;
            user.UpdatedAt = DateTime.Now;

            await _userRepo.SaveAsync();
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation do not match.";
                return RedirectToAction(nameof(Index));
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return NotFound();

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (verification != PasswordVerificationResult.Success)
            {
                TempData["ErrorMessage"] = "Current password is incorrect.";
                return RedirectToAction(nameof(Index));
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            user.UpdatedAt = DateTime.Now;
            await _userRepo.SaveAsync();

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
