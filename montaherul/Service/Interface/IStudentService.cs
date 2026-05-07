using Microsoft.AspNetCore.Mvc;
using montaherul.Models;

namespace montaherul.Service.Interface
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentModel>> GetAllAsync();
        Task<StudentModel?> GetByIdAsync(int id);

        Task<(IEnumerable<StudentModel>, int)> GetStudents(
         string? search,
         int pageNumber,
         int pageSize,
         string sortColumn,
         string sortDirection);

        Task<StudentModel> CreateAsync(StudentModel student);
        Task<bool> UpdateAsync(int id, StudentModel student);
        Task<bool> DeleteAsync(int id);

        Task<IEnumerable<CourseModel>> GetCourses();
       
    }
}