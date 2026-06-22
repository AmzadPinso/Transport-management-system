using Transport_Management_System.Models;

namespace Transport_Management_System.Repository.Interface
{
    public interface IBookingRepository : IBaseRepository<Booking>
    {
        Task<(IEnumerable<Booking>, int)> GetBookingsPagedAsync(
            string? search,
            BookingStatus? status,
            PaymentStatus? paymentStatus,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            int pageSize);

        Task<(IEnumerable<Booking>, int)> GetUserBookingsPagedAsync(
            int userId,
            string? search,
            BookingStatus? status,
            int pageNumber,
            int pageSize);

        Task<Booking?> GetBookingByReferenceAsync(string bookingReference);
        Task<Booking?> GetByIdWithDetailsAsync(int bookingId);
        Task<IEnumerable<string>> GetBookedSeatsForTripAsync(int tripId);
        Task<bool> HasSeatAlreadyBookedAsync(int tripId, string seatNumber);
        Task<bool> HasUserAlreadyBookedTripAsync(int userId, int tripId);
        Task<int> GetTotalBookingsCountAsync();
        Task<int> GetBookingsCountByStatusAsync(BookingStatus status);
        Task<decimal> GetTotalRevenueAsync();
    }
}
