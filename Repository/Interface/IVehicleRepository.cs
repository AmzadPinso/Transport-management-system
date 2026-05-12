using Transport_Management_System.Models;

namespace Transport_Management_System.Repository.Interface
{
    public interface IVehicleRepository : IBaseRepository<Vehicle>
    {
        Task<(IEnumerable<Vehicle>, int)> GetVehiclesPagedAsync(
            string? search,
            VehicleType? type,
            VehicleStatus? status,
            int pageNumber,
            int pageSize);
    }
}
