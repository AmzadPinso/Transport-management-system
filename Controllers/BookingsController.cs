using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Transport_Management_System.Models;
using Transport_Management_System.Models.ViewModels;
using Transport_Management_System.Repository.Interface;
using Transport_Management_System.Services;

namespace Transport_Management_System.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly ITripRepository    _tripRepo;
        private readonly IStationRepository _stationRepo;
        private readonly IUserRepo          _userRepo;
        private readonly ISeatService       _seatService;

        public BookingsController(
            IBookingRepository bookingRepo,
            ITripRepository    tripRepo,
            IStationRepository stationRepo,
            IUserRepo          userRepo,
            ISeatService       seatService)
        {
            _bookingRepo  = bookingRepo;
            _tripRepo     = tripRepo;
            _stationRepo  = stationRepo;
            _userRepo     = userRepo;
            _seatService  = seatService;
        }

        // ─── Helpers ──────────────────────────────────────────────
        private int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId");
            return int.TryParse(claim, out var id) ? id : 0;
        }

        // ─── Trip Search ──────────────────────────────────────────
        // GET /Bookings
        public async Task<IActionResult> Index(
            int? originStationId,
            int? destinationStationId,
            DateTime? departureDate)
        {
            var stations = await _stationRepo.GetAllAsync();
            var vm = new TripSearchViewModel
            {
                Stations             = stations,
                OriginStationId      = originStationId,
                DestinationStationId = destinationStationId,
                DepartureDate        = departureDate
            };

            // Always load available trips; filters narrow down results
            vm.SearchResults = await _tripRepo.GetAvailableTripsAsync(
                originStationId, destinationStationId, departureDate);

            ViewData["Title"] = "Search Trips";
            return View(vm);
        }

        // ─── Seat Selection ───────────────────────────────────────
        // GET /Bookings/SelectSeat/5
        public async Task<IActionResult> SelectSeat(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _tripRepo.GetByIdAsync(id.Value);
            if (trip == null) return NotFound();

            if (trip.Status != TripStatus.Scheduled && trip.Status != TripStatus.ReadyForDispatch)
            {
                TempData["ErrorMessage"] = "This trip is no longer accepting bookings.";
                return RedirectToAction(nameof(Index));
            }

            var bookedSeats = await _bookingRepo.GetBookedSeatsForTripAsync(trip.TripId);
            var seatList    = bookedSeats.ToList();
            var layout      = _seatService.GenerateSeatLayout(trip.Vehicle?.Capacity ?? 40, seatList);

            var vm = new BookingSeatViewModel
            {
                Trip          = trip,
                SeatLayout    = layout,
                TotalBooked   = seatList.Count,
                TotalAvailable = (trip.Vehicle?.Capacity ?? 40) - seatList.Count
            };

            ViewData["Title"] = "Select Seat";
            return View(vm);
        }

        // ─── Booking Confirmation ─────────────────────────────────
        // GET /Bookings/Confirm?tripId=5&seatNumber=B2
        public async Task<IActionResult> Confirm(int tripId, string seatNumber)
        {
            if (string.IsNullOrWhiteSpace(seatNumber))
                return RedirectToAction(nameof(SelectSeat), new { id = tripId });

            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var trip = await _tripRepo.GetByIdAsync(tripId);
            if (trip == null) return NotFound();

            // Check seat is still free
            if (await _bookingRepo.HasSeatAlreadyBookedAsync(tripId, seatNumber))
            {
                TempData["ErrorMessage"] = $"Seat {seatNumber} was just taken. Please choose another.";
                return RedirectToAction(nameof(SelectSeat), new { id = tripId });
            }

            // Check user hasn't already booked this trip
            if (await _bookingRepo.HasUserAlreadyBookedTripAsync(userId, tripId))
            {
                TempData["ErrorMessage"] = "You already have an active booking on this trip.";
                return RedirectToAction(nameof(History));
            }

            var user = await _userRepo.GetByIdAsync(userId);

            var vm = new BookingConfirmViewModel
            {
                TripId         = tripId,
                SeatNumber     = seatNumber,
                Trip           = trip,
                PassengerName  = user?.UserName ?? string.Empty,
                PassengerEmail = user?.Email    ?? string.Empty
            };

            ViewData["Title"] = "Confirm Booking";
            return View(vm);
        }

        // POST /Bookings/Confirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(BookingConfirmViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            // Final seat conflict check
            if (await _bookingRepo.HasSeatAlreadyBookedAsync(model.TripId, model.SeatNumber))
            {
                TempData["ErrorMessage"] = $"Seat {model.SeatNumber} was just taken. Please choose another.";
                return RedirectToAction(nameof(SelectSeat), new { id = model.TripId });
            }

            if (await _bookingRepo.HasUserAlreadyBookedTripAsync(userId, model.TripId))
            {
                TempData["ErrorMessage"] = "You already have an active booking on this trip.";
                return RedirectToAction(nameof(History));
            }

            var trip = await _tripRepo.GetByIdAsync(model.TripId);
            if (trip == null) return NotFound();

            // Generate unique reference (retry on collision)
            string reference;
            do { reference = _seatService.GenerateBookingReference(); }
            while (await _bookingRepo.GetBookingByReferenceAsync(reference) != null);

            var booking = new Booking
            {
                BookingReference = reference,
                UserId           = userId,
                TripId           = model.TripId,
                SeatNumber       = model.SeatNumber,
                BookingDate      = DateTime.Now,
                Status           = BookingStatus.Confirmed,
                PaymentStatus    = PaymentStatus.Pending,
                TotalAmount      = trip.TicketPrice,
                Remarks          = model.Remarks,
                CreatedAt        = DateTime.Now,
                UpdatedAt        = DateTime.Now
            };

            await _bookingRepo.AddAsync(booking);

            // Reduce available capacity on the trip
            trip.AvailableCapacity = Math.Max(0, trip.AvailableCapacity - 1);
            trip.UpdatedAt = DateTime.Now;
            _tripRepo.Update(trip);

            await _bookingRepo.SaveAsync();

            TempData["SuccessMessage"] = $"Booking confirmed! Reference: {reference}";
            return RedirectToAction(nameof(Ticket), new { id = booking.BookingId });
        }

        // ─── Ticket ───────────────────────────────────────────────
        // GET /Bookings/Ticket/5
        public async Task<IActionResult> Ticket(int? id)
        {
            if (id == null) return NotFound();
            var userId  = GetCurrentUserId();
            var booking = await _bookingRepo.GetByIdWithDetailsAsync(id.Value);

            if (booking == null) return NotFound();

            // Users can only view their own tickets; admins can view all
            if (!User.IsInRole("Admin") && booking.UserId != userId)
                return Forbid();

            ViewData["Title"] = $"Ticket - {booking.BookingReference}";
            return View(booking);
        }

        // ─── Booking History ──────────────────────────────────────
        // GET /Bookings/History
        public async Task<IActionResult> History(
            string? search,
            BookingStatus? status,
            int pageNumber = 1,
            int pageSize   = 10)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var (bookings, total) = await _bookingRepo.GetUserBookingsPagedAsync(
                userId, search, status, pageNumber, pageSize);

            ViewBag.Search         = search;
            ViewBag.SelectedStatus = status;
            ViewBag.PageNumber     = pageNumber;
            ViewBag.PageSize       = pageSize;
            ViewBag.TotalRecords   = total;
            ViewBag.TotalPages     = (int)Math.Ceiling(total / (double)pageSize);

            ViewData["Title"] = "My Bookings";
            return View(bookings);
        }

        // ─── Cancel Booking ───────────────────────────────────────
        // POST /Bookings/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int bookingId, string? reason)
        {
            var userId  = GetCurrentUserId();
            var booking = await _bookingRepo.GetByIdWithDetailsAsync(bookingId);

            if (booking == null) return NotFound();
            if (!User.IsInRole("Admin") && booking.UserId != userId) return Forbid();

            if (booking.Status == BookingStatus.Cancelled)
            {
                TempData["ErrorMessage"] = "Booking is already cancelled.";
                return RedirectToAction(nameof(History));
            }

            booking.Status             = BookingStatus.Cancelled;
            booking.CancellationReason = reason;
            booking.CancelledAt        = DateTime.Now;
            booking.UpdatedAt          = DateTime.Now;

            // Return the seat capacity to the trip
            var trip = await _tripRepo.GetByIdAsync(booking.TripId);
            if (trip != null)
            {
                trip.AvailableCapacity++;
                trip.UpdatedAt = DateTime.Now;
                _tripRepo.Update(trip);
            }

            _bookingRepo.Update(booking);
            await _bookingRepo.SaveAsync();

            TempData["SuccessMessage"] = "Booking cancelled successfully.";
            return RedirectToAction(nameof(History));
        }
    }
}
