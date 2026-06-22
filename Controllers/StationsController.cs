using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transport_Management_System.Models;
using Transport_Management_System.Models.ViewModels;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StationsController : Controller
    {
        private readonly IStationRepository _stationRepo;

        public StationsController(IStationRepository stationRepo)
        {
            _stationRepo = stationRepo;
        }

        // GET: Stations
        public async Task<IActionResult> Index(
            string? search,
            bool? isActive,
            int pageNumber = 1,
            int pageSize = 10,
            string sortColumn = "StationId",
            string sortDirection = "desc")
        {
            var (stations, totalRecords) = await _stationRepo.GetStationsPagedAsync(
                search, isActive, pageNumber, pageSize, sortColumn, sortDirection);

            ViewBag.Search = search;
            ViewBag.IsActive = isActive;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.TotalRecords = totalRecords;

            return View(stations);
        }

        // GET: Stations/Create
        public IActionResult Create()
        {
            return View(new StationFormViewModel());
        }

        // POST: Stations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StationFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var station = new Station
                {
                    StationName = model.StationName,
                    City = model.City,
                    District = model.District,
                    Address = model.Address,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _stationRepo.AddAsync(station);
                await _stationRepo.SaveAsync();

                TempData["SuccessMessage"] = "Station created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Stations/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var station = await _stationRepo.GetByIdAsync(id);
            if (station == null)
            {
                return NotFound();
            }

            var model = new StationFormViewModel
            {
                StationId = station.StationId,
                StationName = station.StationName,
                City = station.City,
                District = station.District,
                Address = station.Address,
                Description = station.Description,
                IsActive = station.IsActive
            };

            return View(model);
        }

        // POST: Stations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StationFormViewModel model)
        {
            if (id != model.StationId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var station = await _stationRepo.GetByIdAsync(id);
                if (station == null)
                {
                    return NotFound();
                }

                station.StationName = model.StationName;
                station.City = model.City;
                station.District = model.District;
                station.Address = model.Address;
                station.Description = model.Description;
                station.IsActive = model.IsActive;
                station.UpdatedAt = DateTime.Now;

                _stationRepo.Update(station);
                await _stationRepo.SaveAsync();

                TempData["SuccessMessage"] = "Station updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: Stations/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var station = await _stationRepo.GetByIdAsync(id);
            if (station == null)
            {
                return NotFound();
            }

            station.IsActive = !station.IsActive;
            station.UpdatedAt = DateTime.Now;

            _stationRepo.Update(station);
            await _stationRepo.SaveAsync();

            TempData["SuccessMessage"] = $"Station {(station.IsActive ? "activated" : "deactivated")} successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Stations/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var station = await _stationRepo.GetByIdAsync(id);
            if (station == null)
            {
                return NotFound();
            }

            try
            {
                _stationRepo.Delete(station);
                await _stationRepo.SaveAsync();
                TempData["SuccessMessage"] = "Station deleted successfully!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Cannot delete station. It may be used by existing routes.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
