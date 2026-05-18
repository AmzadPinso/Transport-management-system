using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DriversController : Controller
    {
        private readonly IDriverRepository _driverRepo;
        private readonly IVehicleRepository _vehicleRepo;

        public DriversController(IDriverRepository driverRepo, IVehicleRepository vehicleRepo)
        {
            _driverRepo = driverRepo;
            _vehicleRepo = vehicleRepo;
        }

        // GET: Drivers
        public async Task<IActionResult> Index(
            string search,
            DriverAvailabilityStatus? availabilityStatus,
            string licenseStatus,
            int pageNumber = 1,
            int pageSize = 10,
            string sortColumn = "DriverId",
            string sortDirection = "asc")
        {
            ViewBag.Search = search;
            ViewBag.AvailabilityStatus = availabilityStatus;
            ViewBag.LicenseStatus = licenseStatus;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;

            var (drivers, totalRecords) = await _driverRepo.GetDriversPagedAsync(
                search, availabilityStatus, licenseStatus, pageNumber, pageSize, sortColumn, sortDirection);

            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return View(drivers);
        }

        // GET: Drivers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var driver = await _driverRepo.GetByIdAsync(id.Value);
            if (driver == null) return NotFound();

            if (driver.AssignedVehicleId.HasValue)
            {
                driver.AssignedVehicle = await _vehicleRepo.GetByIdAsync(driver.AssignedVehicleId.Value);
            }

            return View(driver);
        }

        // GET: Drivers/Create
        public async Task<IActionResult> Create()
        {
            await PopulateVehiclesDropDownList();
            return View();
        }

        // POST: Drivers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Driver driver)
        {
            if (ModelState.IsValid)
            {
                driver.CreatedAt = DateTime.Now;
                driver.UpdatedAt = DateTime.Now;

                await _driverRepo.AddAsync(driver);
                await _driverRepo.SaveAsync();
                TempData["SuccessMessage"] = "Driver registered successfully!";
                return RedirectToAction(nameof(Index));
            }
            await PopulateVehiclesDropDownList(driver.AssignedVehicleId);
            return View(driver);
        }

        // GET: Drivers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var driver = await _driverRepo.GetByIdAsync(id.Value);
            if (driver == null) return NotFound();

            await PopulateVehiclesDropDownList(driver.AssignedVehicleId);
            return View(driver);
        }

        // POST: Drivers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Driver driver)
        {
            if (id != driver.DriverId) return NotFound();

            if (ModelState.IsValid)
            {
                var existingDriver = await _driverRepo.GetByIdAsync(id);
                if (existingDriver == null) return NotFound();

                existingDriver.FullName = driver.FullName;
                existingDriver.PhoneNumber = driver.PhoneNumber;
                existingDriver.Email = driver.Email;
                existingDriver.Address = driver.Address;
                existingDriver.LicenseNumber = driver.LicenseNumber;
                existingDriver.LicenseExpiryDate = driver.LicenseExpiryDate;
                existingDriver.ExperienceYears = driver.ExperienceYears;
                existingDriver.AvailabilityStatus = driver.AvailabilityStatus;
                existingDriver.AssignedVehicleId = driver.AssignedVehicleId;
                existingDriver.JoiningDate = driver.JoiningDate;
                existingDriver.UpdatedAt = DateTime.Now;

                _driverRepo.Update(existingDriver);
                await _driverRepo.SaveAsync();

                TempData["SuccessMessage"] = "Driver profile updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await PopulateVehiclesDropDownList(driver.AssignedVehicleId);
            return View(driver);
        }

        // GET: Drivers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var driver = await _driverRepo.GetByIdAsync(id.Value);
            if (driver == null) return NotFound();

            if (driver.AssignedVehicleId.HasValue)
            {
                driver.AssignedVehicle = await _vehicleRepo.GetByIdAsync(driver.AssignedVehicleId.Value);
            }

            return View(driver);
        }

        // POST: Drivers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var driver = await _driverRepo.GetByIdAsync(id);
            if (driver != null)
            {
                _driverRepo.Delete(driver);
                await _driverRepo.SaveAsync();
                TempData["SuccessMessage"] = "Driver removed successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateVehiclesDropDownList(object? selectedVehicle = null)
        {
            var vehicles = await _vehicleRepo.GetAllAsync();
            // Optional: Filter only active or unassigned vehicles
            ViewBag.AssignedVehicleId = new SelectList(vehicles, "VehicleId", "VehicleNumber", selectedVehicle);
        }
    }
}
