using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Repository.Application
{
    public class DriverIssueRepository : BaseRepository<DriverIssue>, IDriverIssueRepository
    {
        public DriverIssueRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<DriverIssue?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(d => d.Driver)
                .Include(d => d.Vehicle)
                .Include(d => d.ReportedByUser)
                .FirstOrDefaultAsync(d => d.DriverIssueId == id);
        }

        public async Task<(IEnumerable<DriverIssue> Issues, int TotalCount)> GetPagedAsync(
            string? search,
            int? vehicleId,
            int? driverId,
            IssueStatus? status,
            IssuePriority? priority,
            int pageNumber,
            int pageSize)
        {
            var query = _dbSet
                .Include(d => d.Driver)
                .Include(d => d.Vehicle)
                .Include(d => d.ReportedByUser)
                .AsQueryable();

            if (vehicleId.HasValue)
            {
                query = query.Where(d => d.VehicleId == vehicleId.Value);
            }

            if (driverId.HasValue)
            {
                query = query.Where(d => d.DriverId == driverId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(d => d.Priority == priority.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    (d.Driver != null && d.Driver.FullName.Contains(search)) ||
                    (d.Vehicle != null && (d.Vehicle.VehicleName.Contains(search) || d.Vehicle.VehicleNumber.Contains(search))) ||
                    d.IssueDescription.Contains(search) ||
                    (d.ResolutionNotes != null && d.ResolutionNotes.Contains(search))
                );
            }

            var totalCount = await query.CountAsync();
            var issues = await query
                .OrderByDescending(d => d.ReportDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (issues, totalCount);
        }

        public async Task<IEnumerable<DriverIssue>> GetByDriverIdAsync(int driverId)
        {
            return await _dbSet
                .Include(d => d.Driver)
                .Include(d => d.Vehicle)
                .Where(d => d.DriverId == driverId)
                .OrderByDescending(d => d.ReportDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<DriverIssue>> GetByVehicleIdAsync(int vehicleId)
        {
            return await _dbSet
                .Include(d => d.Driver)
                .Include(d => d.Vehicle)
                .Where(d => d.VehicleId == vehicleId)
                .OrderByDescending(d => d.ReportDate)
                .ToListAsync();
        }

        public async Task<int> GetOpenIssuesCountAsync()
        {
            return await _dbSet
                .CountAsync(d => d.Status == IssueStatus.Open || d.Status == IssueStatus.InProgress || d.Status == IssueStatus.UnderReview);
        }
    }
}
