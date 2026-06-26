using Transport_Management_System.Models;

namespace Transport_Management_System.Repository.Interface
{
    public interface IMaintenanceRepository : IBaseRepository<MaintenanceRecord>
    {
        Task<IEnumerable<MaintenanceRecord>> GetByVehicleIdAsync(int vehicleId);

        Task<IEnumerable<MaintenanceRecord>> GetOverdueAsync();

        Task<IEnumerable<MaintenanceRecord>> GetUpcomingAsync(int daysAhead = 7);

        Task<(IEnumerable<MaintenanceRecord> Records, int TotalCount)> GetPagedAsync(
            string? search,
            int? vehicleId,
            MaintenanceStatus? status,
            int pageNumber,
            int pageSize);

        Task<int> GetOverdueCountAsync();
        Task<int> GetUpcomingCountAsync(int daysAhead = 7);
        Task<int> GetInProgressCountAsync();
    }
}
