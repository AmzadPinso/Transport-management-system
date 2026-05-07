using montaherul.Models;

namespace montaherul.Service.Interface
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseModel>> GetAllAsync();
        Task<CourseModel?> GetByIdAsync(int id);
        Task<CourseModel> CreateAsync(CourseModel course);
        Task<bool> UpdateAsync(int id, CourseModel course);
        Task<bool> DeleteAsync(int id);
        Task<List<CourseVM>> GetCourseList(int page = 1, decimal size = 5, string searchquery = "");
    }
}