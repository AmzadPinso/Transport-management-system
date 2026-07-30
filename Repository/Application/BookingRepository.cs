using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Repository.Application
{
    public class BookingRepository : BaseRepository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                        .ThenInclude(r => r!.OriginStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                        .ThenInclude(r => r!.DestinationStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Vehicle)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Driver)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task<Booking?> GetByIdWithDetailsAsync(int bookingId)
            => await GetByIdAsync(bookingId);

        public async Task<Booking?> GetBookingByReferenceAsync(string bookingReference)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                        .ThenInclude(r => r!.OriginStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                        .ThenInclude(r => r!.DestinationStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Vehicle)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Driver)
                .FirstOrDefaultAsync(b => b.BookingReference == bookingReference);
        }

        public async Task<(IEnumerable<Booking>, int)> GetBookingsPagedAsync(
            string? search,
            BookingStatus? status,
            PaymentStatus? paymentStatus,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                        .ThenInclude(r => r!.OriginStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                        .ThenInclude(r => r!.DestinationStation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(b =>
                    b.BookingReference.ToLower().Contains(s) ||
                    b.User!.UserName.ToLower().Contains(s) ||
                    b.User!.Email.ToLower().Contains(s) ||
                    b.Trip!.TripName.ToLower().Contains(s));
            }

            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);

            if (paymentStatus.HasValue)
                query = query.Where(b => b.PaymentStatus == paymentStatus.Value);

            if (fromDate.HasValue)
                query = query.Where(b => b.BookingDate.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(b => b.BookingDate.Date <= toDate.Value.Date);

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(b => b.BookingDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }

        public async Task<(IEnumerable<Booking>, int)> GetUserBookingsPagedAsync(
            int userId,
            string? search,
            BookingStatus? status,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                        .ThenInclude(r => r!.OriginStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                        .ThenInclude(r => r!.DestinationStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Vehicle)
                .Where(b => b.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(b =>
                    b.BookingReference.ToLower().Contains(s) ||
                    b.Trip!.TripName.ToLower().Contains(s));
            }

            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(b => b.BookingDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }

        public async Task<IEnumerable<string>> GetBookedSeatsForTripAsync(int tripId)
        {
            return await _context.Bookings
                .Where(b => b.TripId == tripId &&
                            b.Status != BookingStatus.Cancelled)
                .Select(b => b.SeatNumber)
                .ToListAsync();
        }

        public async Task<bool> HasSeatAlreadyBookedAsync(int tripId, string seatNumber)
        {
            return await _context.Bookings
                .AnyAsync(b => b.TripId == tripId &&
                               b.SeatNumber == seatNumber &&
                               b.Status != BookingStatus.Cancelled);
        }

        public async Task<bool> HasUserAlreadyBookedTripAsync(int userId, int tripId)
        {
            return await _context.Bookings
                .AnyAsync(b => b.UserId == userId &&
                               b.TripId == tripId &&
                               b.Status != BookingStatus.Cancelled);
        }

        public async Task<IEnumerable<Booking>> GetGroupBookingsAsync(string groupBookingRef)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                        .ThenInclude(r => r!.OriginStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                        .ThenInclude(r => r!.DestinationStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Vehicle)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Driver)
                .Where(b => b.GroupBookingRef == groupBookingRef)
                .OrderBy(b => b.SeatNumber)
                .ToListAsync();
        }

        public async Task<int> GetTotalBookingsCountAsync()
            => await _context.Bookings.CountAsync();

        public async Task<int> GetBookingsCountByStatusAsync(BookingStatus status)
            => await _context.Bookings.CountAsync(b => b.Status == status);

        public async Task<decimal> GetTotalRevenueAsync()
            => await _context.Bookings
                .Where(b => b.Status != BookingStatus.Cancelled)
                .SumAsync(b => b.TotalAmount);
    }
}
