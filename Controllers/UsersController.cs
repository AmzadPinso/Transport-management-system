using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly IBaseRepository<Role> _roleRepo;
        private readonly PasswordHasher<User> _passwordHasher;

        public UsersController(IUserRepo userRepo, IBaseRepository<Role> roleRepo)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _passwordHasher = new PasswordHasher<User>();
        }

        // GET: Users
        public async Task<IActionResult> Index(
            string? search,
            int? roleId,
            int pageNumber = 1,
            int pageSize = 10,
            string sortColumn = "UserId",
            string sortDirection = "ASC")
        {
            var (users, totalRecords) = await _userRepo.GetUsersPagedAsync(search, roleId, pageNumber, pageSize, sortColumn, sortDirection);

            ViewBag.RoleId = new SelectList(await _roleRepo.GetAllAsync(), "Id", "RoleName", roleId);
            ViewBag.SelectedRoleId = roleId;

            ViewBag.Search = search;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userRepo.GetByIdAsync(id.Value);
            if (user == null) return NotFound();

            return View(user);
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            ViewData["RoleId"] = new SelectList(await _roleRepo.GetAllAsync(), "Id", "RoleName");
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,UserName,Email,Address,RoleId")] User user, string Password)
        {
            if (string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("PasswordHash", "Password is required");
            }

            if (ModelState.IsValid)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, Password);
                await _userRepo.AddAsync(user);
                await _userRepo.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(await _roleRepo.GetAllAsync(), "Id", "RoleName", user.RoleId);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userRepo.GetByIdAsync(id.Value);
            if (user == null) return NotFound();
            ViewData["RoleId"] = new SelectList(await _roleRepo.GetAllAsync(), "Id", "RoleName", user.RoleId);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,UserName,Email,Address,RoleId")] User user, string? NewPassword)
        {
            if (id != user.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Fetch the existing entity from the database (starts tracking)
                    var existingUser = await _userRepo.GetByIdAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // 2. Update properties on the tracked entity
                    existingUser.UserName = user.UserName;
                    existingUser.Email = user.Email;
                    existingUser.Address = user.Address;
                    existingUser.RoleId = user.RoleId;
                    existingUser.UpdatedAt = DateTime.Now;

                    existingUser.UpdatedAt = DateTime.Now;

                    // 4. Save changes (EF will detect modifications on 'existingUser')
                    await _userRepo.SaveAsync();

                    TempData["SuccessMessage"] = "User profile updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "The record was modified by another user. Please try again.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the profile: " + ex.Message);
                }
            }

            // Re-populate dropdown on failure
            ViewData["RoleId"] = new SelectList(await _roleRepo.GetAllAsync(), "Id", "RoleName", user.RoleId);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userRepo.GetByIdAsync(id.Value);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user != null)
            {
                _userRepo.Delete(user);
                await _userRepo.SaveAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
