using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Transport_Management_System.Models;
using Transport_Management_System.Models.ViewModels;
using Transport_Management_System.Repository.Interface;
using Transport_Management_System.Services;

namespace Transport_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MaintenanceController : Controller
    {
        private readonly IMaintenanceRepository _maintenanceRepo;
        private readonly IVehicleRepository _vehicleRepo;
        private readonly IMaintenanceService _maintenanceService;

        public MaintenanceController(
            IMaintenanceRepository maintenanceRepo,
            IVehicleRepository vehicleRepo,
            IMaintenanceService maintenanceService)
        {
            _maintenanceRepo = maintenanceRepo;
            _vehicleRepo = vehicleRepo;
            _maintenanceService = maintenanceService;
        }

        // GET: /Maintenance
        public async Task<IActionResult> Index(
            string? search,
            int? vehicleId,
            MaintenanceStatus? status,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var alerts = await _maintenanceService.GetMaintenanceAlertsAsync();
            var (records, totalCount) = await _maintenanceRepo.GetPagedAsync(search, vehicleId, status, pageNumber, pageSize);
            
            var vehicles = await _vehicleRepo.GetAllAsync();

            ViewBag.Search = search;
            ViewBag.SelectedVehicleId = vehicleId;
            ViewBag.SelectedStatus = status;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.VehicleList = new SelectList(vehicles, "VehicleId", "VehicleName", vehicleId);
            ViewBag.Alerts = alerts;

            ViewData["Title"] = "Maintenance Monitoring & Alerts";
            return View(records);
        }

        // GET: /Maintenance/Create
        public async Task<IActionResult> Create()
        {
            var vehicles = await _vehicleRepo.GetAllAsync();
            var vm = new MaintenanceFormViewModel
            {
                VehicleList = vehicles.Select(v => new SelectListItem
                {
                    Value = v.VehicleId.ToString(),
                    Text = $"{v.VehicleName} ({v.VehicleNumber})"
                })
            };

            ViewData["Title"] = "Add Maintenance Record";
            return View(vm);
        }

        // POST: /Maintenance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var record = new MaintenanceRecord
                {
                    VehicleId = model.VehicleId,
                    MaintenanceType = model.MaintenanceType,
                    ServiceDate = model.ServiceDate,
                    NextServiceDate = model.NextServiceDate,
                    ServiceProvider = model.ServiceProvider,
                    Cost = model.Cost,
                    Notes = model.Notes,
                    Status = model.Status,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _maintenanceRepo.AddAsync(record);

                // Update vehicle's LastServiceDate if this is completed
                if (record.Status == MaintenanceStatus.Completed)
                {
                    var vehicle = await _vehicleRepo.GetByIdAsync(record.VehicleId);
                    if (vehicle != null && record.ServiceDate > vehicle.LastServiceDate)
                    {
                        vehicle.LastServiceDate = record.ServiceDate;
                        vehicle.UpdatedAt = DateTime.Now;
                        _vehicleRepo.Update(vehicle);
                    }
                }

                await _maintenanceRepo.SaveAsync();
                await _vehicleRepo.SaveAsync();

                TempData["SuccessMessage"] = "Maintenance record added successfully.";
                return RedirectToAction(nameof(Index));
            }

            var vehicles = await _vehicleRepo.GetAllAsync();
            model.VehicleList = vehicles.Select(v => new SelectListItem
            {
                Value = v.VehicleId.ToString(),
                Text = $"{v.VehicleName} ({v.VehicleNumber})"
            });

            ViewData["Title"] = "Add Maintenance Record";
            return View(model);
        }

        // GET: /Maintenance/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var record = await _maintenanceRepo.GetByIdAsync(id.Value);
            if (record == null) return NotFound();

            var vehicles = await _vehicleRepo.GetAllAsync();
            var vm = new MaintenanceFormViewModel
            {
                MaintenanceRecordId = record.MaintenanceRecordId,
                VehicleId = record.VehicleId,
                MaintenanceType = record.MaintenanceType,
                ServiceDate = record.ServiceDate,
                NextServiceDate = record.NextServiceDate,
                ServiceProvider = record.ServiceProvider,
                Cost = record.Cost,
                Notes = record.Notes,
                Status = record.Status,
                VehicleList = vehicles.Select(v => new SelectListItem
                {
                    Value = v.VehicleId.ToString(),
                    Text = $"{v.VehicleName} ({v.VehicleNumber})"
                })
            };

            ViewData["Title"] = "Edit Maintenance Record";
            return View(vm);
        }

        // POST: /Maintenance/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaintenanceFormViewModel model)
        {
            if (id != model.MaintenanceRecordId) return NotFound();

            if (ModelState.IsValid)
            {
                var record = await _maintenanceRepo.GetByIdAsync(id);
                if (record == null) return NotFound();

                record.VehicleId = model.VehicleId;
                record.MaintenanceType = model.MaintenanceType;
                record.ServiceDate = model.ServiceDate;
                record.NextServiceDate = model.NextServiceDate;
                record.ServiceProvider = model.ServiceProvider;
                record.Cost = model.Cost;
                record.Notes = model.Notes;
                record.Status = model.Status;
                record.UpdatedAt = DateTime.Now;

                _maintenanceRepo.Update(record);

                // Update vehicle's LastServiceDate if this is completed
                if (record.Status == MaintenanceStatus.Completed)
                {
                    var vehicle = await _vehicleRepo.GetByIdAsync(record.VehicleId);
                    if (vehicle != null && record.ServiceDate > vehicle.LastServiceDate)
                    {
                        vehicle.LastServiceDate = record.ServiceDate;
                        vehicle.UpdatedAt = DateTime.Now;
                        _vehicleRepo.Update(vehicle);
                    }
                }

                await _maintenanceRepo.SaveAsync();
                await _vehicleRepo.SaveAsync();

                TempData["SuccessMessage"] = "Maintenance record updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            var vehicles = await _vehicleRepo.GetAllAsync();
            model.VehicleList = vehicles.Select(v => new SelectListItem
            {
                Value = v.VehicleId.ToString(),
                Text = $"{v.VehicleName} ({v.VehicleNumber})"
            });

            ViewData["Title"] = "Edit Maintenance Record";
            return View(model);
        }

        // GET: /Maintenance/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var record = await _maintenanceRepo.GetByIdAsync(id.Value);
            if (record == null) return NotFound();

            ViewData["Title"] = "Maintenance Record Details";
            return View(record);
        }

        // POST: /Maintenance/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _maintenanceRepo.GetByIdAsync(id);
            if (record == null) return NotFound();

            _maintenanceRepo.Delete(record);
            await _maintenanceRepo.SaveAsync();

            TempData["SuccessMessage"] = "Maintenance record deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
