using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBookingsController : Controller
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly ITripRepository    _tripRepo;

        public AdminBookingsController(
            IBookingRepository bookingRepo,
            ITripRepository    tripRepo)
        {
            _bookingRepo = bookingRepo;
            _tripRepo    = tripRepo;
        }

        // GET /AdminBookings
        public async Task<IActionResult> Index(
            string? search,
            BookingStatus? status,
            PaymentStatus? paymentStatus,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber = 1,
            int pageSize   = 15)
        {
            var (bookings, total) = await _bookingRepo.GetBookingsPagedAsync(
                search, status, paymentStatus, fromDate, toDate, pageNumber, pageSize);

            ViewBag.Search            = search;
            ViewBag.SelectedStatus    = status;
            ViewBag.SelectedPayment   = paymentStatus;
            ViewBag.FromDate          = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate            = toDate?.ToString("yyyy-MM-dd");
            ViewBag.PageNumber        = pageNumber;
            ViewBag.PageSize          = pageSize;
            ViewBag.TotalRecords      = total;
            ViewBag.TotalPages        = (int)Math.Ceiling(total / (double)pageSize);

            // Summary stats
            ViewBag.TotalBookings     = await _bookingRepo.GetTotalBookingsCountAsync();
            ViewBag.ConfirmedBookings = await _bookingRepo.GetBookingsCountByStatusAsync(BookingStatus.Confirmed);
            ViewBag.PendingBookings   = await _bookingRepo.GetBookingsCountByStatusAsync(BookingStatus.Pending);
            ViewBag.CancelledBookings = await _bookingRepo.GetBookingsCountByStatusAsync(BookingStatus.Cancelled);
            ViewBag.TotalRevenue      = await _bookingRepo.GetTotalRevenueAsync();

            ViewData["Title"] = "Booking Management";
            return View(bookings);
        }

        // GET /AdminBookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _bookingRepo.GetByIdWithDetailsAsync(id.Value);
            if (booking == null) return NotFound();

            ViewData["Title"] = $"Booking - {booking.BookingReference}";
            return View(booking);
        }

        // POST /AdminBookings/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int bookingId, BookingStatus status)
        {
            var booking = await _bookingRepo.GetByIdWithDetailsAsync(bookingId);
            if (booking == null) return NotFound();

            var oldStatus     = booking.Status;
            booking.Status    = status;
            booking.UpdatedAt = DateTime.Now;

            if (status == BookingStatus.Cancelled && oldStatus != BookingStatus.Cancelled)
            {
                booking.CancelledAt = DateTime.Now;
                // Restore trip capacity
                var trip = await _tripRepo.GetByIdAsync(booking.TripId);
                if (trip != null)
                {
                    trip.AvailableCapacity++;
                    trip.UpdatedAt = DateTime.Now;
                    _tripRepo.Update(trip);
                }
            }

            _bookingRepo.Update(booking);
            await _bookingRepo.SaveAsync();

            TempData["SuccessMessage"] = $"Booking status updated to {status}.";
            return RedirectToAction(nameof(Details), new { id = bookingId });
        }

        // POST /AdminBookings/UpdatePaymentStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(int bookingId, PaymentStatus paymentStatus)
        {
            var booking = await _bookingRepo.GetByIdWithDetailsAsync(bookingId);
            if (booking == null) return NotFound();

            booking.PaymentStatus = paymentStatus;
            booking.UpdatedAt     = DateTime.Now;

            _bookingRepo.Update(booking);
            await _bookingRepo.SaveAsync();

            TempData["SuccessMessage"] = $"Payment status updated to {paymentStatus}.";
            return RedirectToAction(nameof(Details), new { id = bookingId });
        }
    }
}
