using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;
using Transport_Management_System.Services;

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
        private readonly IReportsService _reportsService;
        private readonly AppDbContext _context;

        public DashboardController(
            IDriverRepository driverRepo,
            IRouteRepository routeRepo,
            IStationRepository stationRepo,
            ITripRepository tripRepo,
            IBookingRepository bookingRepo,
            IMaintenanceRepository maintenanceRepo,
            IDriverIssueRepository driverIssueRepo,
            IExpenseRepository expenseRepo,
            IReportsService reportsService,
            AppDbContext context)
        {
            _driverRepo = driverRepo;
            _routeRepo = routeRepo;
            _stationRepo = stationRepo;
            _tripRepo = tripRepo;
            _bookingRepo = bookingRepo;
            _maintenanceRepo = maintenanceRepo;
            _driverIssueRepo = driverIssueRepo;
            _expenseRepo = expenseRepo;
            _reportsService = reportsService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard Overview";

            var drivers = await _driverRepo.GetAllAsync();
            var routes = await _routeRepo.GetAllAsync();
            var stations = await _stationRepo.GetAllAsync();
            var trips = await _tripRepo.GetAllAsync();
            var vehicles = await _context.Vehicles.ToListAsync();
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

            // Vehicle Status Counts
            ViewBag.ActiveVehiclesCount = vehicles.Count(v => v.Status == VehicleStatus.Active);
            ViewBag.MaintenanceVehiclesCount = vehicles.Count(v => v.Status == VehicleStatus.InMaintenance);
            ViewBag.OutOfServiceVehiclesCount = vehicles.Count(v => v.Status == VehicleStatus.OutOfService);
            ViewBag.TotalVehiclesCount = vehicles.Count;

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

            // Fetch the advanced executive model containing recent bookings, trips, maintenance, and reports
            var execModel = await _reportsService.GetExecutiveDashboardAsync();

            // Fetch last 7 days booking counts for the chart
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i))
                .Reverse()
                .ToList();

            var chartLabels = last7Days.Select(d => d.ToString("ddd")).ToArray();
            var chartBookings = new int[7];
            var chartRevenues = new decimal[7];

            for (int i = 0; i < 7; i++)
            {
                var date = last7Days[i];
                var dateBookings = await _context.Bookings
                    .Where(b => b.BookingDate.Date == date.Date)
                    .ToListAsync();

                chartBookings[i] = dateBookings.Count;
                chartRevenues[i] = dateBookings.Where(b => b.PaymentStatus == PaymentStatus.Paid).Sum(b => b.TotalAmount);
            }

            ViewBag.ChartLabels = chartLabels;
            ViewBag.ChartBookings = chartBookings;
            ViewBag.ChartRevenues = chartRevenues;

            return View(execModel);
        }
    }
}
