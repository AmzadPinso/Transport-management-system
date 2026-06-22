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

        public DashboardController(
            IDriverRepository driverRepo,
            IRouteRepository routeRepo,
            IStationRepository stationRepo)
        {
            _driverRepo = driverRepo;
            _routeRepo = routeRepo;
            _stationRepo = stationRepo;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard Overview";

            var drivers = await _driverRepo.GetAllAsync();
            var routes = await _routeRepo.GetAllAsync();
            var stations = await _stationRepo.GetAllAsync();
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

            return View();
        }
    }
}
