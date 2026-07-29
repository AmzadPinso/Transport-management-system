using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Repository.Application
{
    public class ShipmentRepository : BaseRepository<Shipment>, IShipmentRepository
    {
        public ShipmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Shipment>, int)> GetShipmentsPagedAsync(
            string? search,
            ShipmentType? type,
            ShipmentStatus? status,
            int pageNumber,
            int pageSize)
        {
            IQueryable<Shipment> query = _context.Shipments.Include(s => s.Vehicle);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.ShipmentRef.Contains(search) ||
                    s.SenderName.Contains(search) ||
                    s.ReceiverName.Contains(search) ||
                    s.Origin.Contains(search) ||
                    s.Destination.Contains(search));
            }

            if (type.HasValue)
                query = query.Where(s => s.ShipmentType == type.Value);

            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);

            int totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }
    }
}
