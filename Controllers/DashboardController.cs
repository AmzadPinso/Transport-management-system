using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDriverRepository _driverRepo;
        private readonly IRouteRepository _routeRepo;
        private readonly IStationRepository _stationRepo;
        private readonly ITripRepository _tripRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly IMaintenanceRepository _maintenanceRepo;
        private readonly IDriverIssueRepository _driverIssueRepo;
        private readonly IExpenseRepository _expenseRepo;

        public DashboardController(
            IDriverRepository driverRepo,
            IRouteRepository routeRepo,
            IStationRepository stationRepo,
            ITripRepository tripRepo,
            IBookingRepository bookingRepo,
            IMaintenanceRepository maintenanceRepo,
            IDriverIssueRepository driverIssueRepo,
            IExpenseRepository expenseRepo)
        {
            _driverRepo = driverRepo;
            _routeRepo = routeRepo;
            _stationRepo = stationRepo;
            _tripRepo = tripRepo;
            _bookingRepo = bookingRepo;
            _maintenanceRepo = maintenanceRepo;
            _driverIssueRepo = driverIssueRepo;
            _expenseRepo = expenseRepo;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard Overview";

            var drivers = await _driverRepo.GetAllAsync();
            var routes = await _routeRepo.GetAllAsync();
            var stations = await _stationRepo.GetAllAsync();
            var trips = await _tripRepo.GetAllAsync();
            var now = DateTime.Now;

            ViewBag.TotalDrivers = drivers.Count();
            ViewBag.AvailableDrivers = drivers.Count(d => d.AvailabilityStatus == DriverAvailabilityStatus.Available);
            ViewBag.DriversOnTrip = drivers.Count(d => d.AvailabilityStatus == DriverAvailabilityStatus.OnTrip);
            ViewBag.ExpiredLicenses = drivers.Count(d => d.LicenseExpiryDate <= now);
            ViewBag.ExpiringLicenses = drivers.Count(d => d.LicenseExpiryDate > now && d.LicenseExpiryDate <= now.AddDays(30));

            ViewBag.TotalRoutes = routes.Count();
            ViewBag.ActiveRoutes = routes.Count(r => r.Status == RouteStatus.Active);
            ViewBag.TotalStations = stations.Count();
            ViewBag.ActiveStations = stations.Count(s => s.IsActive);

            // Trip Statistics
            ViewBag.TotalTrips = trips.Count();
            ViewBag.ScheduledTrips = trips.Count(t => t.Status == TripStatus.Scheduled);
            ViewBag.OngoingTrips = trips.Count(t => t.Status == TripStatus.Ongoing);
            ViewBag.CompletedTrips = trips.Count(t => t.Status == TripStatus.Completed);
            ViewBag.DelayedTrips = trips.Count(t => t.Status == TripStatus.Delayed);
            ViewBag.CancelledTrips = trips.Count(t => t.Status == TripStatus.Cancelled);
            ViewBag.ReadyForDispatchTrips = trips.Count(t => t.Status == TripStatus.ReadyForDispatch);

            // Booking & Revenue Statistics
            ViewBag.TotalBookings = await _bookingRepo.GetTotalBookingsCountAsync();
            ViewBag.ConfirmedBookings = await _bookingRepo.GetBookingsCountByStatusAsync(BookingStatus.Confirmed);
            ViewBag.PendingBookings = await _bookingRepo.GetBookingsCountByStatusAsync(BookingStatus.Pending);
            ViewBag.CancelledBookings = await _bookingRepo.GetBookingsCountByStatusAsync(BookingStatus.Cancelled);
            ViewBag.TotalRevenue = await _bookingRepo.GetTotalRevenueAsync();

            // Week 6 Sprint — Smart Features & Maintenance stats
            ViewBag.OverdueMaintenanceCount = await _maintenanceRepo.GetOverdueCountAsync();
            ViewBag.UpcomingMaintenanceCount = await _maintenanceRepo.GetUpcomingCountAsync(7);
            ViewBag.InProgressMaintenanceCount = await _maintenanceRepo.GetInProgressCountAsync();
            ViewBag.OpenDriverIssuesCount = await _driverIssueRepo.GetOpenIssuesCountAsync();
            
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            ViewBag.TotalExpenses = await _expenseRepo.GetTotalExpensesAsync();
            ViewBag.ThisMonthExpenses = await _expenseRepo.GetTotalExpensesAsync(startOfMonth, today);

            return View();
        }
    }
}

