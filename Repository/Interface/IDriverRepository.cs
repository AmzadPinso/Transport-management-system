using Transport_Management_System.Models;

namespace Transport_Management_System.Repository.Interface
{
    public interface IDriverRepository : IBaseRepository<Driver>
    {
        Task<(IEnumerable<Driver> drivers, int totalRecords)> GetDriversPagedAsync(
            string search,
            DriverAvailabilityStatus? availabilityStatus,
            string licenseStatus,
            int pageNumber,
            int pageSize,
            string sortColumn,
            string sortDirection);
    }
}
