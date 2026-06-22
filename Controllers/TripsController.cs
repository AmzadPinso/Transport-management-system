using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Transport_Management_System.Models;
using Transport_Management_System.Models.ViewModels;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TripsController : Controller
    {
        private readonly ITripRepository _tripRepo;
        private readonly IVehicleRepository _vehicleRepo;
        private readonly IDriverRepository _driverRepo;
        private readonly IRouteRepository _routeRepo;

        public TripsController(
            ITripRepository tripRepo,
            IVehicleRepository vehicleRepo,
            IDriverRepository driverRepo,
            IRouteRepository routeRepo)
        {
            _tripRepo = tripRepo;
            _vehicleRepo = vehicleRepo;
            _driverRepo = driverRepo;
            _routeRepo = routeRepo;
        }

        // GET: Trips
        public async Task<IActionResult> Index(
            string? search,
            TripStatus? status,
            DateTime? departureDate,
            int? driverId,
            int? vehicleId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var (trips, totalRecords) = await _tripRepo.GetTripsPagedAsync(
                search, status, departureDate, driverId, vehicleId, pageNumber, pageSize);

            // Populate viewbags for filtering dropdowns
            var drivers = await _driverRepo.GetAllAsync();
            var vehicles = await _vehicleRepo.GetAllAsync();

            ViewBag.DriversList = new SelectList(drivers, "DriverId", "FullName", driverId);
            ViewBag.VehiclesList = new SelectList(vehicles, "VehicleId", "VehicleNumber", vehicleId);
            ViewBag.Search = search;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedDepartureDate = departureDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedDriverId = driverId;
            ViewBag.SelectedVehicleId = vehicleId;

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return View(trips);
        }

        // GET: Trips/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _tripRepo.GetByIdAsync(id.Value);
            if (trip == null) return NotFound();

            return View(trip);
        }

        // GET: Trips/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new TripFormViewModel());
        }

        // POST: Trips/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TripFormViewModel model)
        {
            var departureDateTime = model.DepartureDate.Date.Add(model.DepartureTime);

            // Enforce future dates/times for scheduling
            if (departureDateTime <= DateTime.Now)
            {
                ModelState.AddModelError("DepartureDate", "Departure date and time must be in the future.");
            }

            if (model.EstimatedArrivalTime <= departureDateTime)
            {
                ModelState.AddModelError("EstimatedArrivalTime", "Estimated Arrival Time must be after the Departure Time.");
            }

            // Conflict Detection
            if (ModelState.IsValid)
            {
                bool hasVehicleConflict = await _tripRepo.HasVehicleConflictAsync(model.VehicleId, departureDateTime, model.EstimatedArrivalTime);
                if (hasVehicleConflict)
                {
                    ModelState.AddModelError("VehicleId", "Selected vehicle is already assigned during this schedule.");
                }

                bool hasDriverConflict = await _tripRepo.HasDriverConflictAsync(model.DriverId, departureDateTime, model.EstimatedArrivalTime);
                if (hasDriverConflict)
                {
                    ModelState.AddModelError("DriverId", "Selected driver is already assigned during this schedule.");
                }
            }

            if (ModelState.IsValid)
            {
                var vehicle = await _vehicleRepo.GetByIdAsync(model.VehicleId);
                var trip = new Trip
                {
                    TripName = model.TripName,
                    RouteId = model.RouteId,
                    VehicleId = model.VehicleId,
                    DriverId = model.DriverId,
                    DepartureDate = model.DepartureDate,
                    DepartureTime = model.DepartureTime,
                    EstimatedArrivalTime = model.EstimatedArrivalTime,
                    AvailableCapacity = vehicle?.Capacity ?? 0,
                    Status = TripStatus.Scheduled,
                    Remarks = model.Remarks,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _tripRepo.AddAsync(trip);
                await _tripRepo.SaveAsync();

                TempData["SuccessMessage"] = "Trip scheduled successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync();
            return View(model);
        }

        // GET: Trips/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _tripRepo.GetByIdAsync(id.Value);
            if (trip == null) return NotFound();

            var model = new TripFormViewModel
            {
                TripId = trip.TripId,
                TripName = trip.TripName,
                RouteId = trip.RouteId,
                VehicleId = trip.VehicleId,
                DriverId = trip.DriverId,
                DepartureDate = trip.DepartureDate,
                DepartureTime = trip.DepartureTime,
                EstimatedArrivalTime = trip.EstimatedArrivalTime,
                Remarks = trip.Remarks,
                Status = trip.Status
            };

            await PopulateDropdownsAsync(trip.VehicleId, trip.DriverId);
            return View(model);
        }

        // POST: Trips/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TripFormViewModel model)
        {
            if (id != model.TripId) return NotFound();

            var departureDateTime = model.DepartureDate.Date.Add(model.DepartureTime);

            if (model.EstimatedArrivalTime <= departureDateTime)
            {
                ModelState.AddModelError("EstimatedArrivalTime", "Estimated Arrival Time must be after the Departure Time.");
            }

            // Conflict Detection excluding current trip
            if (ModelState.IsValid)
            {
                bool hasVehicleConflict = await _tripRepo.HasVehicleConflictAsync(model.VehicleId, departureDateTime, model.EstimatedArrivalTime, id);
                if (hasVehicleConflict)
                {
                    ModelState.AddModelError("VehicleId", "Selected vehicle is already assigned during this schedule.");
                }

                bool hasDriverConflict = await _tripRepo.HasDriverConflictAsync(model.DriverId, departureDateTime, model.EstimatedArrivalTime, id);
                if (hasDriverConflict)
                {
                    ModelState.AddModelError("DriverId", "Selected driver is already assigned during this schedule.");
                }
            }

            if (ModelState.IsValid)
            {
                var trip = await _tripRepo.GetByIdAsync(id);
                if (trip == null) return NotFound();

                var vehicleChanged = trip.VehicleId != model.VehicleId;

                trip.TripName = model.TripName;
                trip.RouteId = model.RouteId;
                trip.VehicleId = model.VehicleId;
                trip.DriverId = model.DriverId;
                trip.DepartureDate = model.DepartureDate;
                trip.DepartureTime = model.DepartureTime;
                trip.EstimatedArrivalTime = model.EstimatedArrivalTime;
                trip.Remarks = model.Remarks;
                trip.Status = model.Status;
                trip.UpdatedAt = DateTime.Now;

                if (vehicleChanged)
                {
                    var newVehicle = await _vehicleRepo.GetByIdAsync(model.VehicleId);
                    trip.AvailableCapacity = newVehicle?.Capacity ?? 0;
                }

                _tripRepo.Update(trip);
                await _tripRepo.SaveAsync();

                // If trip status was updated, manage driver availability as well
                await SyncDriverStatusAsync(trip.DriverId, trip.Status);

                TempData["SuccessMessage"] = "Trip schedule updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(model.VehicleId, model.DriverId);
            return View(model);
        }

        // POST: Trips/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, TripStatus status)
        {
            var trip = await _tripRepo.GetByIdAsync(id);
            if (trip == null) return NotFound();

            var oldStatus = trip.Status;
            trip.Status = status;
            trip.UpdatedAt = DateTime.Now;

            _tripRepo.Update(trip);
            await _tripRepo.SaveAsync();

            // Perform Driver Availability Integration
            await SyncDriverStatusAsync(trip.DriverId, status);

            TempData["SuccessMessage"] = $"Trip status updated to {status} successfully!";
            return RedirectToAction(nameof(Details), new { id = trip.TripId });
        }

        // GET: Trips/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _tripRepo.GetByIdAsync(id.Value);
            if (trip == null) return NotFound();

            return View(trip);
        }

        // POST: Trips/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trip = await _tripRepo.GetByIdAsync(id);
            if (trip != null)
            {
                _tripRepo.Delete(trip);
                await _tripRepo.SaveAsync();

                // Make sure the driver is set back to available if the trip was ongoing/scheduled
                await SyncDriverStatusAsync(trip.DriverId, TripStatus.Cancelled);

                TempData["SuccessMessage"] = "Trip deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync(int? currentVehicleId = null, int? currentDriverId = null)
        {
            var routes = await _routeRepo.GetAllAsync();
            var vehicles = await _vehicleRepo.GetAllAsync();
            var drivers = await _driverRepo.GetAllAsync();

            // Filter active routes
            var activeRoutes = routes.Where(r => r.Status == RouteStatus.Active).ToList();
            ViewBag.Routes = new SelectList(activeRoutes, "RouteId", "RouteName");

            // Filter active vehicles + include current assigned vehicle (even if in maintenance/out of service)
            var activeVehicles = vehicles
                .Where(v => v.Status == VehicleStatus.Active || v.VehicleId == currentVehicleId)
                .Select(v => new { v.VehicleId, DisplayText = $"{v.VehicleName} ({v.VehicleNumber}) - Cap: {v.Capacity}" })
                .ToList();
            ViewBag.Vehicles = new SelectList(activeVehicles, "VehicleId", "DisplayText");

            // Filter available drivers + include current assigned driver
            var availableDrivers = drivers
                .Where(d => d.AvailabilityStatus == DriverAvailabilityStatus.Available || d.DriverId == currentDriverId)
                .Select(d => new { d.DriverId, DisplayText = $"{d.FullName} ({d.PhoneNumber}) - Status: {d.AvailabilityStatus}" })
                .ToList();
            ViewBag.Drivers = new SelectList(availableDrivers, "DriverId", "DisplayText");
        }

        private async Task SyncDriverStatusAsync(int driverId, TripStatus tripStatus)
        {
            var driver = await _driverRepo.GetByIdAsync(driverId);
            if (driver != null)
            {
                if (tripStatus == TripStatus.Ongoing)
                {
                    driver.AvailabilityStatus = DriverAvailabilityStatus.OnTrip;
                }
                else if (tripStatus == TripStatus.Completed || tripStatus == TripStatus.Cancelled)
                {
                    driver.AvailabilityStatus = DriverAvailabilityStatus.Available;
                }

                driver.UpdatedAt = DateTime.Now;
                _driverRepo.Update(driver);
                await _driverRepo.SaveAsync();
            }
        }
    }
}
