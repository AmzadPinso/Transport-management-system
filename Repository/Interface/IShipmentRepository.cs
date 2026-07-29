using Transport_Management_System.Models;

namespace Transport_Management_System.Repository.Interface
{
    public interface IShipmentRepository : IBaseRepository<Shipment>
    {
        Task<(IEnumerable<Shipment>, int)> GetShipmentsPagedAsync(
            string? search,
            ShipmentType? type,
            ShipmentStatus? status,
            int pageNumber,
            int pageSize);
    }
}
