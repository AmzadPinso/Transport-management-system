using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using montaherul.Models;
using montaherul.Repository.Application;
using montaherul.Repository.Interface;
using montaherul.Service.Interface;

namespace montaherul.Controllers
{
   // [ApiController]
    [Route("api/[controller]")]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IStudentRepo _studentRepo;
public StudentController(IStudentService studentService, IStudentRepo studentRepo)
        {
            _studentService = studentService;
            _studentRepo = studentRepo;
        }


        // ✅ API (Pagination + Search)

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedStudents(
        string? search,
        int pageNumber = 1,
        int pageSize = 10,
        string sortColumn = "Id",
        string sortDirection = "ASC")
        {
            var (data, total) = await _studentRepo.GetStudents(
                search, pageNumber, pageSize, sortColumn, sortDirection);

            return Ok(new
            {
                last_page = (int)Math.Ceiling((double)total / pageSize),
                data = data ?? new List<StudentModel>()
            });
        }

        // ================= MVC =================

        [HttpGet("/Student")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/Student/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null) return NotFound();

            return View(student);
        }

        [HttpGet("/Student/Create")]
        public async Task<IActionResult> Create()
        {
            var courses = await _studentService.GetCourses();
            ViewBag.CourseList = new SelectList(courses, "Id", "CourseName");

            return View("CreateEdit");
        }

        [HttpGet("/Student/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null) return NotFound();

            var courses = await _studentService.GetCourses();
            ViewBag.CourseList = new SelectList(courses, "Id", "CourseName", student.CourseId);

            return View("CreateEdit", student);
        }

        [HttpPost("/Student/Save")]
        public async Task<IActionResult> Save(StudentModel student)
        {
            if (student.Id == 0)
                await _studentService.CreateAsync(student);
            else
                await _studentService.UpdateAsync(student.Id, student);

            return RedirectToAction("Index");
        }

        [HttpGet("/Student/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null) return NotFound();

            return View(student);
        }

        [HttpPost("/Student/Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _studentService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}