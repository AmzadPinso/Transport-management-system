using Transport_Management_System.Models;
using Route = Transport_Management_System.Models.Route;

namespace Transport_Management_System.Repository.Interface
{
    public interface IRouteRepository : IBaseRepository<Route>
    {
        Task<(IEnumerable<Route> routes, int totalRecords)> GetRoutesPagedAsync(
            string? search,
            int? originStationId,
            int? destinationStationId,
            RouteStatus? status,
            string? city,
            double? minDistance,
            double? maxDistance,
            int pageNumber,
            int pageSize,
            string sortColumn,
            string sortDirection);

        Task<Route?> GetRouteWithDetailsAsync(int id);
    }
}
