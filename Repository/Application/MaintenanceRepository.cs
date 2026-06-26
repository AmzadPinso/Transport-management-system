using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Repository.Application
{
    public class MaintenanceRepository : BaseRepository<MaintenanceRecord>, IMaintenanceRepository
    {
        public MaintenanceRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<MaintenanceRecord?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(m => m.Vehicle)
                .FirstOrDefaultAsync(m => m.MaintenanceRecordId == id);
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetByVehicleIdAsync(int vehicleId)
        {
            return await _dbSet
                .Include(m => m.Vehicle)
                .Where(m => m.VehicleId == vehicleId)
                .OrderByDescending(m => m.ServiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetOverdueAsync()
        {
            var today = DateTime.Today;
            return await _dbSet
                .Include(m => m.Vehicle)
                .Where(m => m.Status != MaintenanceStatus.Completed && m.Status != MaintenanceStatus.Cancelled && m.NextServiceDate < today)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetUpcomingAsync(int daysAhead = 7)
        {
            var today = DateTime.Today;
            var targetDate = today.AddDays(daysAhead);
            return await _dbSet
                .Include(m => m.Vehicle)
                .Where(m => m.Status != MaintenanceStatus.Completed && m.Status != MaintenanceStatus.Cancelled && m.NextServiceDate >= today && m.NextServiceDate <= targetDate)
                .ToListAsync();
        }

        public async Task<(IEnumerable<MaintenanceRecord> Records, int TotalCount)> GetPagedAsync(
            string? search,
            int? vehicleId,
            MaintenanceStatus? status,
            int pageNumber,
            int pageSize)
        {
            var query = _dbSet.Include(m => m.Vehicle).AsQueryable();

            if (vehicleId.HasValue)
            {
                query = query.Where(m => m.VehicleId == vehicleId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(m => m.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => 
                    (m.Vehicle != null && (m.Vehicle.VehicleName.Contains(search) || m.Vehicle.VehicleNumber.Contains(search))) ||
                    (m.ServiceProvider != null && m.ServiceProvider.Contains(search)) ||
                    (m.Notes != null && m.Notes.Contains(search))
                );
            }

            var totalCount = await query.CountAsync();
            var records = await query
                .OrderByDescending(m => m.ServiceDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (records, totalCount);
        }

        public async Task<int> GetOverdueCountAsync()
        {
            var today = DateTime.Today;
            return await _dbSet
                .CountAsync(m => m.Status != MaintenanceStatus.Completed && m.Status != MaintenanceStatus.Cancelled && m.NextServiceDate < today);
        }

        public async Task<int> GetUpcomingCountAsync(int daysAhead = 7)
        {
            var today = DateTime.Today;
            var targetDate = today.AddDays(daysAhead);
            return await _dbSet
                .CountAsync(m => m.Status != MaintenanceStatus.Completed && m.Status != MaintenanceStatus.Cancelled && m.NextServiceDate >= today && m.NextServiceDate <= targetDate);
        }

        public async Task<int> GetInProgressCountAsync()
        {
            return await _dbSet
                .CountAsync(m => m.Status == MaintenanceStatus.InProgress);
        }
    }
}
