using Transport_Management_System.Models;

namespace Transport_Management_System.Repository.Interface
{
    public interface IStationRepository : IBaseRepository<Station>
    {
        Task<(IEnumerable<Station> stations, int totalRecords)> GetStationsPagedAsync(
            string? search,
            bool? isActive,
            int pageNumber,
            int pageSize,
            string sortColumn,
            string sortDirection);
    }
}
