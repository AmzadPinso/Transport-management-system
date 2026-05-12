using Transport_Management_System.Models;

namespace Transport_Management_System.Repository.Interface
{
    public interface IUserRepo : IBaseRepository<User>
    {
        Task<(IEnumerable<User>, int)> GetUsersPagedAsync(
            string? search,
            int? roleId,
            int pageNumber,
            int pageSize,
            string sortColumn,
            string sortDirection);
        
        Task<User?> GetByEmailAsync(string email);
    }
}
