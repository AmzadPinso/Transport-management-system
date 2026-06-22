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

        public DashboardController(
            IDriverRepository driverRepo,
            IRouteRepository routeRepo,
            IStationRepository stationRepo,
            ITripRepository tripRepo)
        {
            _driverRepo = driverRepo;
            _routeRepo = routeRepo;
            _stationRepo = stationRepo;
            _tripRepo = tripRepo;
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

            return View();
        }
    }
}
