using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using montaherul;
using montaherul.Models;
using montaherul.Service.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace montaherul.Controllers
{
    public class CourseController : Controller
    {
        private readonly MyDBContext _context ;
        private readonly ICourseService _course ;

        public CourseController(MyDBContext context,ICourseService course)
        {
            _context = context;
            _course = course;
        }

        // GET: Course
        public async Task<IActionResult> Index()
        {
            var courses = _context.Courses
                .Include(c => c.Teacher);

            return View(await courses.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> GetCourseList(int page = 1, decimal size = 5, string searchquery = "")
        {
            try
            {
                var recordsTotal = 0;
               
                var companyList = await _course.GetCourseList(page, size, searchquery);
                if (companyList.Count != 0)
                {
                    recordsTotal = companyList.Count();
                    if (companyList.FirstOrDefault().TOTALCOUNT != 0)
                    {
                        recordsTotal = companyList.FirstOrDefault().TOTALCOUNT;
                    }
                    else
                    {
                        recordsTotal = 0;
                    }
                }
                var pagecount = Math.Ceiling(recordsTotal / size);

                return Ok(new
                {
                    success = true,
                    data = companyList,
                    last_page = pagecount,
                    recordsTotal,
                    pagecount
                });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }




        // GET: Course/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        // GET: Course/Create
        public IActionResult Create()
        {
            ViewBag.TeacherList = new SelectList(_context.TeacherModel, "Id", "Name");
            return View("CreateEdit");
        }

        // GET: Course/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses.FindAsync(id);

            if (course == null)
                return NotFound();

            ViewBag.TeacherList = new SelectList(_context.TeacherModel, "Id", "Name", course.TeacherId);
            return View("CreateEdit", course);
        }

        // POST: Course/Save (Create + Edit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(CourseModel course)
        {
            //if (!ModelState.IsValid)
            //{
              

            //    ViewBag.TeacherList = new SelectList(_context.TeacherModel, "Id", "Name", course.TeacherId);
            //    return View("CreateEdit", course);

            //}



            try
            {
                if (course.Id == 0)
                {
                    _context.Courses.Add(course); // Create
                }
                else
                {
                    _context.Courses.Update(course); // Edit
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Courses.Any(e => e.Id == course.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Course/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course != null)
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}