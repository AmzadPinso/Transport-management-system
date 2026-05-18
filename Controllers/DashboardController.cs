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

        public DashboardController(IDriverRepository driverRepo)
        {
            _driverRepo = driverRepo;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard Overview";

            var drivers = await _driverRepo.GetAllAsync();
            var now = DateTime.Now;

            ViewBag.TotalDrivers = drivers.Count();
            ViewBag.AvailableDrivers = drivers.Count(d => d.AvailabilityStatus == DriverAvailabilityStatus.Available);
            ViewBag.DriversOnTrip = drivers.Count(d => d.AvailabilityStatus == DriverAvailabilityStatus.OnTrip);
            ViewBag.ExpiredLicenses = drivers.Count(d => d.LicenseExpiryDate <= now);
            ViewBag.ExpiringLicenses = drivers.Count(d => d.LicenseExpiryDate > now && d.LicenseExpiryDate <= now.AddDays(30));

            return View();
        }
    }
}
