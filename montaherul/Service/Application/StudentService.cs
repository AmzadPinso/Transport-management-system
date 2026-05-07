using montaherul.Models;
using montaherul.Service.Interface;
using montaherul.UnitOfWork.Interface;

namespace montaherul.Service.Application
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _uow;

        public StudentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<StudentModel>> GetAllAsync()
        {
            return await _uow.Student.GetAllAsync();
        }

        public async Task<StudentModel?> GetByIdAsync(int id)
        {
            return await _uow.Student.GetByIdAsync(id);
        }

        // ✅ PAGINATION + SEARCH (from stored procedure / repo)
        public async Task<(IEnumerable<StudentModel>, int)> GetStudents(
      string? search,
      int pageNumber,
      int pageSize,
      string sortColumn,
      string sortDirection)
        {
            return await _uow.Student.GetStudents(
                search, pageNumber, pageSize, sortColumn, sortDirection);
        }
        // ✅ CREATE
        public async Task<StudentModel> CreateAsync(StudentModel student)
        {
            await _uow.Student.AddAsync(student);
            await _uow.SaveChangesAsync();
            return student;
        }

        // ✅ UPDATE
        public async Task<bool> UpdateAsync(int id, StudentModel student)
        {
            var existing = await _uow.Student.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Name = student.Name;
            existing.Age = student.Age;
            existing.Email = student.Email;
            existing.Address = student.Address;
            existing.CourseId = student.CourseId;

            _uow.Student.Update(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        // ✅ DELETE
        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _uow.Student.GetByIdAsync(id);
            if (existing == null) return false;

            _uow.Student.Delete(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        // ✅ COURSES (needed for dropdown)
        public async Task<IEnumerable<CourseModel>> GetCourses()
        {
            return await _uow.Course.GetAllAsync();
        }
    }
}