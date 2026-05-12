using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Repository.Application
{
    public class VehicleRepository : BaseRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Vehicle>, int)> GetVehiclesPagedAsync(
            string? search,
            VehicleType? type,
            VehicleStatus? status,
            int pageNumber,
            int pageSize)
        {
            IQueryable<Vehicle> query = _context.Vehicles;

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(v => v.VehicleName.Contains(search) || v.VehicleNumber.Contains(search));
            }

            // Filters
            if (type.HasValue)
            {
                query = query.Where(v => v.VehicleType == type.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(v => v.Status == status.Value);
            }

            // Total Records
            int totalRecords = await query.CountAsync();

            // Paging
            var data = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }
    }
}
