using Transport_Management_System.Models;

namespace Transport_Management_System.Repository.Interface
{
    public interface IDriverIssueRepository : IBaseRepository<DriverIssue>
    {
        Task<(IEnumerable<DriverIssue> Issues, int TotalCount)> GetPagedAsync(
            string? search,
            int? vehicleId,
            int? driverId,
            IssueStatus? status,
            IssuePriority? priority,
            int pageNumber,
            int pageSize);

        Task<IEnumerable<DriverIssue>> GetByDriverIdAsync(int driverId);
        Task<IEnumerable<DriverIssue>> GetByVehicleIdAsync(int vehicleId);
        Task<int> GetOpenIssuesCountAsync();
    }
}
