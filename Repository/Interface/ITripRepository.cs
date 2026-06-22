using Transport_Management_System.Models;

namespace Transport_Management_System.Repository.Interface
{
    public interface ITripRepository : IBaseRepository<Trip>
    {
        Task<(IEnumerable<Trip>, int)> GetTripsPagedAsync(
            string? search,
            TripStatus? status,
            DateTime? departureDate,
            int? driverId,
            int? vehicleId,
            int pageNumber,
            int pageSize);

        Task<bool> HasVehicleConflictAsync(int vehicleId, DateTime departure, DateTime arrival, int? excludeTripId = null);
        Task<bool> HasDriverConflictAsync(int driverId, DateTime departure, DateTime arrival, int? excludeTripId = null);
    }
}
