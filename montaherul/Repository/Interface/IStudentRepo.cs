using montaherul.Models;

namespace montaherul.Repository.Interface
{
    public interface IStudentRepo : IBaseRepository<StudentModel>
    {
        Task<(IEnumerable<StudentModel>, int)> GetStudents(string? search, int pageNumber, int pageSize, string sortColumn, string sortDirection);
    }
}