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
        public async Task<IActionResult> SelectSeat(int? id, int passengerCount = 1)
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
            var recommendation = _seatService.RecommendSeats(layout, passengerCount);

            var vm = new BookingSeatViewModel
            {
                Trip          = trip,
                SeatLayout    = layout,
                TotalBooked   = seatList.Count,
                TotalAvailable = (trip.Vehicle?.Capacity ?? 40) - seatList.Count
            };

            ViewBag.Recommendation = recommendation;
            ViewBag.PassengerCount = passengerCount;
            ViewData["Title"] = "Select Seat";
            return View(vm);
        }

        // ─── Booking Confirmation ─────────────────────────────────
        // GET /Bookings/Confirm?tripId=5&seatNumbers=A1,A2,A3
        public async Task<IActionResult> Confirm(int tripId, string seatNumbers)
        {
            if (string.IsNullOrWhiteSpace(seatNumbers))
                return RedirectToAction(nameof(SelectSeat), new { id = tripId });

            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var trip = await _tripRepo.GetByIdAsync(tripId);
            if (trip == null) return NotFound();

            // Parse and deduplicate seats
            var seats = seatNumbers
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (seats.Count == 0)
                return RedirectToAction(nameof(SelectSeat), new { id = tripId });

            // Check each seat is still free
            var takenSeats = new List<string>();
            foreach (var seat in seats)
            {
                if (await _bookingRepo.HasSeatAlreadyBookedAsync(tripId, seat))
                    takenSeats.Add(seat);
            }

            if (takenSeats.Count == seats.Count)
            {
                TempData["ErrorMessage"] = $"All selected seats ({string.Join(", ", takenSeats)}) are already taken. Please choose again.";
                return RedirectToAction(nameof(SelectSeat), new { id = tripId });
            }

            if (takenSeats.Any())
                TempData["WarningMessage"] = $"Seat(s) {string.Join(", ", takenSeats)} were already taken and removed from your selection.";

            var availableSeats = seats.Except(takenSeats, StringComparer.OrdinalIgnoreCase).ToList();
            var user = await _userRepo.GetByIdAsync(userId);

            var vm = new BookingConfirmViewModel
            {
                TripId         = tripId,
                SeatNumbers    = string.Join(",", availableSeats),
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

            var seats = model.SeatList;
            if (seats.Count == 0)
                return RedirectToAction(nameof(SelectSeat), new { id = model.TripId });

            var trip = await _tripRepo.GetByIdAsync(model.TripId);
            if (trip == null) return NotFound();

            // Final per-seat conflict check
            var takenSeats = new List<string>();
            foreach (var seat in seats)
            {
                if (await _bookingRepo.HasSeatAlreadyBookedAsync(model.TripId, seat))
                    takenSeats.Add(seat);
            }

            var availableSeats = seats.Except(takenSeats, StringComparer.OrdinalIgnoreCase).ToList();

            if (availableSeats.Count == 0)
            {
                TempData["ErrorMessage"] = $"All selected seats ({string.Join(", ", takenSeats)}) were taken between confirmation and submit. Please choose again.";
                return RedirectToAction(nameof(SelectSeat), new { id = model.TripId });
            }

            if (takenSeats.Any())
                TempData["WarningMessage"] = $"Seat(s) {string.Join(", ", takenSeats)} were taken by another user and skipped.";

            // Generate a shared group reference for multi-seat bookings
            bool isGroup = availableSeats.Count > 1;
            string? groupRef = isGroup ? $"GRP-{DateTime.Now:yyMMddHHmm}-{new Random().Next(100,999)}" : null;

            int firstBookingId = 0;

            foreach (var seat in availableSeats)
            {
                // Generate unique reference per seat (retry on collision)
                string reference;
                do { reference = _seatService.GenerateBookingReference(); }
                while (await _bookingRepo.GetBookingByReferenceAsync(reference) != null);

                var booking = new Booking
                {
                    BookingReference = reference,
                    GroupBookingRef  = groupRef,
                    UserId           = userId,
                    TripId           = model.TripId,
                    SeatNumber       = seat,
                    BookingDate      = DateTime.Now,
                    Status           = BookingStatus.Confirmed,
                    PaymentStatus    = PaymentStatus.Pending,
                    TotalAmount      = trip.TicketPrice,
                    Remarks          = model.Remarks,
                    CreatedAt        = DateTime.Now,
                    UpdatedAt        = DateTime.Now
                };

                await _bookingRepo.AddAsync(booking);

                if (firstBookingId == 0)
                {
                    // Flush to get the BookingId of the first record
                    await _bookingRepo.SaveAsync();
                    firstBookingId = booking.BookingId;
                }

                // Reduce capacity for each seat
                trip.AvailableCapacity = Math.Max(0, trip.AvailableCapacity - 1);
            }

            trip.UpdatedAt = DateTime.Now;
            _tripRepo.Update(trip);
            await _bookingRepo.SaveAsync();

            TempData["SuccessMessage"] = availableSeats.Count == 1
                ? $"Booking confirmed! Seat {availableSeats[0]} booked successfully."
                : $"{availableSeats.Count} seats booked! Group ref: {groupRef}";

            return RedirectToAction(nameof(Ticket), new { id = firstBookingId });
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

            // Load all seats for group bookings
            if (!string.IsNullOrEmpty(booking.GroupBookingRef))
            {
                var groupBookings = await _bookingRepo.GetGroupBookingsAsync(booking.GroupBookingRef);
                ViewBag.GroupBookings = groupBookings;
            }

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
