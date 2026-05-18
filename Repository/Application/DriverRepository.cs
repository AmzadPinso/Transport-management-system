using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Repository.Application
{
    public class DriverRepository : BaseRepository<Driver>, IDriverRepository
    {
        public DriverRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Driver> drivers, int totalRecords)> GetDriversPagedAsync(
            string search,
            DriverAvailabilityStatus? availabilityStatus,
            string licenseStatus,
            int pageNumber,
            int pageSize,
            string sortColumn,
            string sortDirection)
        {
            var query = _context.Drivers.Include(d => d.AssignedVehicle).AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(d => 
                    d.FullName.ToLower().Contains(search) || 
                    d.LicenseNumber.ToLower().Contains(search) || 
                    d.PhoneNumber.Contains(search));
            }

            // Availability Status Filter
            if (availabilityStatus.HasValue)
            {
                query = query.Where(d => d.AvailabilityStatus == availabilityStatus.Value);
            }

            // License Status Filter
            if (!string.IsNullOrWhiteSpace(licenseStatus))
            {
                var now = DateTime.Now;
                var thirtyDaysFromNow = now.AddDays(30);

                switch (licenseStatus.ToLower())
                {
                    case "valid":
                        query = query.Where(d => d.LicenseExpiryDate > thirtyDaysFromNow);
                        break;
                    case "expiring":
                        query = query.Where(d => d.LicenseExpiryDate > now && d.LicenseExpiryDate <= thirtyDaysFromNow);
                        break;
                    case "expired":
                        query = query.Where(d => d.LicenseExpiryDate <= now);
                        break;
                }
            }

            // Get total count before pagination
            var totalRecords = await query.CountAsync();

            // Sorting
            query = sortDirection.ToLower() == "asc"
                ? sortColumn.ToLower() switch
                {
                    "fullname" => query.OrderBy(d => d.FullName),
                    "license" => query.OrderBy(d => d.LicenseNumber),
                    "expiry" => query.OrderBy(d => d.LicenseExpiryDate),
                    "status" => query.OrderBy(d => d.AvailabilityStatus),
                    _ => query.OrderBy(d => d.DriverId) // Default
                }
                : sortColumn.ToLower() switch
                {
                    "fullname" => query.OrderByDescending(d => d.FullName),
                    "license" => query.OrderByDescending(d => d.LicenseNumber),
                    "expiry" => query.OrderByDescending(d => d.LicenseExpiryDate),
                    "status" => query.OrderByDescending(d => d.AvailabilityStatus),
                    _ => query.OrderByDescending(d => d.DriverId) // Default
                };

            // Pagination
            var drivers = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (drivers, totalRecords);
        }
    }
}
