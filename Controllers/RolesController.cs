using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly IBaseRepository<Role> _roleRepo;

        public RolesController(IBaseRepository<Role> roleRepo)
        {
            _roleRepo = roleRepo;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            var roles = await _roleRepo.GetAllAsync();
            return View(roles);
        }

        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RoleName,Description")] Role role)
        {
            if (ModelState.IsValid)
            {
                await _roleRepo.AddAsync(role);
                await _roleRepo.SaveAsync();
                TempData["SuccessMessage"] = "Role created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var role = await _roleRepo.GetByIdAsync(id.Value);
            if (role == null) return NotFound();
            return View(role);
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoleName,Description")] Role role)
        {
            if (id != role.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _roleRepo.Update(role);
                await _roleRepo.SaveAsync();
                TempData["SuccessMessage"] = "Role updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // GET: Roles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var role = await _roleRepo.GetByIdAsync(id.Value);
            if (role == null) return NotFound();

            return View(role);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _roleRepo.GetByIdAsync(id);
            if (role != null)
            {
                _roleRepo.Delete(role);
                await _roleRepo.SaveAsync();
                TempData["SuccessMessage"] = "Role deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
