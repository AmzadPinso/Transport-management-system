using montaherul.Models;

namespace montaherul.Repository.Interface
{
    public interface ICourseRepo : IBaseRepository<CourseModel>
    {
        public Task<List<CourseVM>> GetAllCourseAsync(string searchTerm, int page = 1, int size = 5);

    }
}