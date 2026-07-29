using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShipmentsController : Controller
    {
        private readonly IShipmentRepository _shipmentRepo;
        private readonly IVehicleRepository _vehicleRepo;

        public ShipmentsController(IShipmentRepository shipmentRepo, IVehicleRepository vehicleRepo)
        {
            _shipmentRepo = shipmentRepo;
            _vehicleRepo = vehicleRepo;
        }

        // GET: Shipments
        public async Task<IActionResult> Index(string? search, ShipmentType? type, ShipmentStatus? status, int pageNumber = 1, int pageSize = 10)
        {
            var (shipments, totalRecords) = await _shipmentRepo.GetShipmentsPagedAsync(search, type, status, pageNumber, pageSize);

            ViewBag.Search        = search;
            ViewBag.SelectedType  = type;
            ViewBag.SelectedStatus = status;
            ViewBag.PageNumber    = pageNumber;
            ViewBag.PageSize      = pageSize;
            ViewBag.TotalRecords  = totalRecords;
            ViewBag.TotalPages    = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return View(shipments);
        }

        // GET: Shipments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var shipment = await _shipmentRepo.GetByIdAsync(id.Value);
            if (shipment == null) return NotFound();

            // Manually load vehicle if not eager-loaded
            if (shipment.VehicleId.HasValue && shipment.Vehicle == null)
            {
                shipment.Vehicle = await _vehicleRepo.GetByIdAsync(shipment.VehicleId.Value);
            }

            return View(shipment);
        }

        // GET: Shipments/Create
        public async Task<IActionResult> Create()
        {
            await PopulateVehiclesDropdownAsync();
            return View();
        }

        // POST: Shipments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shipment shipment)
        {
            // ShipmentRef is auto-generated — remove it from ModelState
            // so its empty value never causes a false ModelState failure.
            ModelState.Remove(nameof(Shipment.ShipmentRef));
            ModelState.Remove(nameof(Shipment.CreatedAt));
            ModelState.Remove(nameof(Shipment.UpdatedAt));

            if (ModelState.IsValid)
            {
                try
                {
                    shipment.ShipmentRef = GenerateRef();
                    shipment.CreatedAt   = DateTime.Now;
                    shipment.UpdatedAt   = DateTime.Now;
                    await _shipmentRepo.AddAsync(shipment);
                    await _shipmentRepo.SaveAsync();
                    TempData["SuccessMessage"] = $"Shipment {shipment.ShipmentRef} created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Unable to save shipment. Error: {ex.InnerException?.Message ?? ex.Message}");
                }
            }

            // Surface all validation errors in development for easier debugging
#if DEBUG
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => $"{x.Key}: {string.Join(", ", x.Value!.Errors.Select(e => e.ErrorMessage))}");
            TempData["DevErrors"] = string.Join(" | ", errors);
#endif

            await PopulateVehiclesDropdownAsync(shipment.VehicleId);
            return View(shipment);
        }

        // GET: Shipments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var shipment = await _shipmentRepo.GetByIdAsync(id.Value);
            if (shipment == null) return NotFound();
            await PopulateVehiclesDropdownAsync(shipment.VehicleId);
            return View(shipment);
        }

        // POST: Shipments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Shipment shipment)
        {
            if (id != shipment.ShipmentId) return NotFound();

            // Remove auto-managed fields from ModelState
            ModelState.Remove(nameof(Shipment.ShipmentRef));
            ModelState.Remove(nameof(Shipment.CreatedAt));
            ModelState.Remove(nameof(Shipment.UpdatedAt));

            if (ModelState.IsValid)
            {
                try
                {
                    shipment.UpdatedAt = DateTime.Now;
                    _shipmentRepo.Update(shipment);
                    await _shipmentRepo.SaveAsync();
                    TempData["SuccessMessage"] = "Shipment updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Unable to save changes. Error: {ex.InnerException?.Message ?? ex.Message}");
                }
            }
            await PopulateVehiclesDropdownAsync(shipment.VehicleId);
            return View(shipment);
        }

        // GET: Shipments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var shipment = await _shipmentRepo.GetByIdAsync(id.Value);
            if (shipment == null) return NotFound();
            return View(shipment);
        }

        // POST: Shipments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shipment = await _shipmentRepo.GetByIdAsync(id);
            if (shipment != null)
            {
                _shipmentRepo.Delete(shipment);
                await _shipmentRepo.SaveAsync();
                TempData["SuccessMessage"] = "Shipment deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ─────────────────────────────────────────────────────────────
        private async Task PopulateVehiclesDropdownAsync(int? selectedId = null)
        {
            var vehicles = await _vehicleRepo.GetAllAsync();
            ViewBag.Vehicles = new SelectList(
                vehicles.Where(v => v.Status == VehicleStatus.Active),
                "VehicleId", "VehicleName", selectedId);
        }

        private static string GenerateRef()
        {
            var ts   = DateTime.Now;
            var rand = new Random().Next(100, 999);
            return $"SHP-{ts:yyMMdd}-{rand}";
        }
    }
}
