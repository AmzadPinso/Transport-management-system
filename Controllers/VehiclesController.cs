using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VehiclesController : Controller
    {
        private readonly IVehicleRepository _vehicleRepo;

        public VehiclesController(IVehicleRepository vehicleRepo)
        {
            _vehicleRepo = vehicleRepo;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index(string? search, VehicleType? type, VehicleStatus? status, int pageNumber = 1, int pageSize = 10)
        {
            var (vehicles, totalRecords) = await _vehicleRepo.GetVehiclesPagedAsync(search, type, status, pageNumber, pageSize);

            ViewBag.Search = search;
            ViewBag.SelectedType = type;
            ViewBag.SelectedStatus = status;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return View(vehicles);
        }

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _vehicleRepo.GetByIdAsync(id.Value);
            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // GET: Vehicles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VehicleName,VehicleNumber,VehicleType,Capacity,Status,LastServiceDate")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                vehicle.CreatedAt = DateTime.Now;
                vehicle.UpdatedAt = DateTime.Now;
                await _vehicleRepo.AddAsync(vehicle);
                await _vehicleRepo.SaveAsync();
                TempData["SuccessMessage"] = "Vehicle created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        // GET: Vehicles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _vehicleRepo.GetByIdAsync(id.Value);
            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VehicleId,VehicleName,VehicleNumber,VehicleType,Capacity,Status,LastServiceDate,CreatedAt")] Vehicle vehicle)
        {
            if (id != vehicle.VehicleId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    vehicle.UpdatedAt = DateTime.Now;
                    _vehicleRepo.Update(vehicle);
                    await _vehicleRepo.SaveAsync();
                    TempData["SuccessMessage"] = "Vehicle updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. Error: " + ex.Message);
                }
            }
            return View(vehicle);
        }

        // GET: Vehicles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _vehicleRepo.GetByIdAsync(id.Value);
            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _vehicleRepo.GetByIdAsync(id);
            if (vehicle != null)
            {
                _vehicleRepo.Delete(vehicle);
                await _vehicleRepo.SaveAsync();
                TempData["SuccessMessage"] = "Vehicle deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
