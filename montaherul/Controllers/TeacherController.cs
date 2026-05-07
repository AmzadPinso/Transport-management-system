using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using montaherul;
using montaherul.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace montaherul.Controllers
{
    public class TeacherController : Controller
    {
        private readonly MyDBContext _context;
        private readonly IWebHostEnvironment _environment;

        public TeacherController(MyDBContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ===================== INDEX =====================
        public async Task<IActionResult> Index()
        {
            return View(await _context.TeacherModel.ToListAsync());
        }

        // ===================== DETAILS =====================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.TeacherModel
                .FirstOrDefaultAsync(m => m.Id == id);

            if (teacher == null) return NotFound();

            return View(teacher);
        }

        // ===================== CREATE =====================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TeacherModel teacherModel,
            IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    string folder = Path.Combine(_environment.WebRootPath, "images/teachers");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() +
                                      Path.GetExtension(imageFile.FileName);

                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    teacherModel.ProfileImage = fileName;
                }

                _context.Add(teacherModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(teacherModel);
        }

        // ===================== EDIT =====================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.TeacherModel.FindAsync(id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            TeacherModel teacherModel,
            IFormFile imageFile)
        {
            if (id != teacherModel.Id)
                return NotFound();

            var existingTeacher = await _context.TeacherModel
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null)
                    {
                        // Delete old image
                        if (!string.IsNullOrEmpty(existingTeacher.ProfileImage))
                        {
                            string oldPath = Path.Combine(
                                _environment.WebRootPath,
                                "images/teachers",
                                existingTeacher.ProfileImage);

                            if (System.IO.File.Exists(oldPath))
                                System.IO.File.Delete(oldPath);
                        }

                        // Save new image
                        string folder = Path.Combine(_environment.WebRootPath, "images/teachers");

                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        string fileName = Guid.NewGuid().ToString() +
                                          Path.GetExtension(imageFile.FileName);

                        string filePath = Path.Combine(folder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        teacherModel.ProfileImage = fileName;
                    }
                    else
                    {
                        teacherModel.ProfileImage = existingTeacher.ProfileImage;
                    }

                    _context.Update(teacherModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherModelExists(teacherModel.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(teacherModel);
        }

        // ===================== DELETE =====================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.TeacherModel
                .FirstOrDefaultAsync(m => m.Id == id);

            if (teacher == null) return NotFound();

            return View(teacher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.TeacherModel.FindAsync(id);

            if (teacher != null)
            {
                // Delete image from folder
                if (!string.IsNullOrEmpty(teacher.ProfileImage))
                {
                    string path = Path.Combine(
                        _environment.WebRootPath,
                        "images/teachers",
                        teacher.ProfileImage);

                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                _context.TeacherModel.Remove(teacher);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeacherModelExists(int id)
        {
            return _context.TeacherModel.Any(e => e.Id == id);
        }
    }
}