using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Models.ViewModels;
using Transport_Management_System.Repository.Interface;
using Route = Transport_Management_System.Models.Route;

namespace Transport_Management_System.Controllers
{
    [Authorize]
    public class RoutesController : Controller
    {
        private readonly IRouteRepository _routeRepo;
        private readonly IStationRepository _stationRepo;
        private readonly AppDbContext _context;

        public RoutesController(IRouteRepository routeRepo, IStationRepository stationRepo, AppDbContext context)
        {
            _routeRepo = routeRepo;
            _stationRepo = stationRepo;
            _context = context;
        }

        // GET: Routes
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(
            string? search,
            int? originStationId,
            int? destinationStationId,
            RouteStatus? status,
            string? city,
            double? minDistance,
            double? maxDistance,
            int pageNumber = 1,
            int pageSize = 10,
            string sortColumn = "RouteId",
            string sortDirection = "desc")
        {
            var (routes, totalRecords) = await _routeRepo.GetRoutesPagedAsync(
                search, originStationId, destinationStationId, status, city, minDistance, maxDistance, pageNumber, pageSize, sortColumn, sortDirection);

            var allStations = (await _stationRepo.GetAllAsync()).Where(s => s.IsActive).OrderBy(s => s.StationName).ToList();

            ViewBag.Search = search;
            ViewBag.OriginStationId = originStationId;
            ViewBag.DestinationStationId = destinationStationId;
            ViewBag.Status = status;
            ViewBag.City = city;
            ViewBag.MinDistance = minDistance;
            ViewBag.MaxDistance = maxDistance;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.TotalRecords = totalRecords;

            ViewBag.Stations = new SelectList(allStations, "StationId", "StationName");

            return View(routes);
        }

        // GET: Routes/Calculator
        public async Task<IActionResult> Calculator()
        {
            var stations = (await _stationRepo.GetAllAsync()).Where(s => s.IsActive).OrderBy(s => s.StationName).ToList();
            ViewBag.Stations = new SelectList(stations, "StationId", "StationName");
            return View();
        }

        // GET: Routes/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var route = await _routeRepo.GetRouteWithDetailsAsync(id);
            if (route == null)
            {
                return NotFound();
            }

            return View(route);
        }

        // GET: Routes/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var stations = (await _stationRepo.GetAllAsync()).Where(s => s.IsActive).OrderBy(s => s.StationName).ToList();
            ViewBag.Stations = new SelectList(stations, "StationId", "StationName");
            return View(new RouteFormViewModel());
        }

        // POST: Routes/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RouteFormViewModel model)
        {
            if (model.OriginStationId == model.DestinationStationId)
            {
                ModelState.AddModelError("DestinationStationId", "Origin and Destination stations cannot be the same.");
            }

            if (ModelState.IsValid)
            {
                var route = new Route
                {
                    RouteName = model.RouteName,
                    OriginStationId = model.OriginStationId,
                    DestinationStationId = model.DestinationStationId,
                    DistanceKm = model.DistanceKm,
                    EstimatedDurationMinutes = model.EstimatedDurationMinutes,
                    Status = model.Status,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _routeRepo.AddAsync(route);
                await _routeRepo.SaveAsync();

                TempData["SuccessMessage"] = "Route created successfully! You can now manage intermediate stops and pickup/drop-off points.";
                return RedirectToAction(nameof(Index));
            }

            var stations = (await _stationRepo.GetAllAsync()).Where(s => s.IsActive).OrderBy(s => s.StationName).ToList();
            ViewBag.Stations = new SelectList(stations, "StationId", "StationName", model.OriginStationId);
            return View(model);
        }

        // GET: Routes/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var route = await _routeRepo.GetByIdAsync(id);
            if (route == null)
            {
                return NotFound();
            }

            var model = new RouteFormViewModel
            {
                RouteId = route.RouteId,
                RouteName = route.RouteName,
                OriginStationId = route.OriginStationId,
                DestinationStationId = route.DestinationStationId,
                DistanceKm = route.DistanceKm,
                EstimatedDurationMinutes = route.EstimatedDurationMinutes,
                Status = route.Status
            };

            var stations = (await _stationRepo.GetAllAsync()).Where(s => s.IsActive).OrderBy(s => s.StationName).ToList();
            ViewBag.Stations = new SelectList(stations, "StationId", "StationName");
            return View(model);
        }

        // POST: Routes/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RouteFormViewModel model)
        {
            if (id != model.RouteId)
            {
                return BadRequest();
            }

            if (model.OriginStationId == model.DestinationStationId)
            {
                ModelState.AddModelError("DestinationStationId", "Origin and Destination stations cannot be the same.");
            }

            if (ModelState.IsValid)
            {
                var route = await _routeRepo.GetByIdAsync(id);
                if (route == null)
                {
                    return NotFound();
                }

                route.RouteName = model.RouteName;
                route.OriginStationId = model.OriginStationId;
                route.DestinationStationId = model.DestinationStationId;
                route.DistanceKm = model.DistanceKm;
                route.EstimatedDurationMinutes = model.EstimatedDurationMinutes;
                route.Status = model.Status;
                route.UpdatedAt = DateTime.Now;

                _routeRepo.Update(route);
                await _routeRepo.SaveAsync();

                TempData["SuccessMessage"] = "Route updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            var stations = (await _stationRepo.GetAllAsync()).Where(s => s.IsActive).OrderBy(s => s.StationName).ToList();
            ViewBag.Stations = new SelectList(stations, "StationId", "StationName", model.OriginStationId);
            return View(model);
        }

        // POST: Routes/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var route = await _routeRepo.GetByIdAsync(id);
            if (route == null)
            {
                return NotFound();
            }

            _routeRepo.Delete(route);
            await _routeRepo.SaveAsync();

            TempData["SuccessMessage"] = "Route deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Routes/ManageStops/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageStops(int id)
        {
            var route = await _routeRepo.GetRouteWithDetailsAsync(id);
            if (route == null)
            {
                return NotFound();
            }

            var stations = (await _stationRepo.GetAllAsync())
                .Where(s => s.IsActive && s.StationId != route.OriginStationId && s.StationId != route.DestinationStationId)
                .OrderBy(s => s.StationName)
                .ToList();

            ViewBag.Stations = new SelectList(stations, "StationId", "StationName");
            return View(route);
        }

        // POST: Routes/AddStop
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStop(int routeId, int stationId, int sequenceOrder)
        {
            var route = await _context.Routes
                .Include(r => r.IntermediateStops)
                .FirstOrDefaultAsync(r => r.RouteId == routeId);

            if (route == null)
            {
                return NotFound();
            }

            if (route.OriginStationId == stationId || route.DestinationStationId == stationId)
            {
                TempData["ErrorMessage"] = "Intermediate stop cannot be the Origin or Destination station.";
                return RedirectToAction(nameof(ManageStops), new { id = routeId });
            }

            if (route.IntermediateStops.Any(s => s.StationId == stationId))
            {
                TempData["ErrorMessage"] = "This station is already added as a stop on this route.";
                return RedirectToAction(nameof(ManageStops), new { id = routeId });
            }

            var stop = new IntermediateStop
            {
                RouteId = routeId,
                StationId = stationId,
                SequenceOrder = sequenceOrder
            };

            _context.IntermediateStops.Add(stop);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Stop added successfully!";
            return RedirectToAction(nameof(ManageStops), new { id = routeId });
        }

        // POST: Routes/RemoveStop
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStop(int stopId, int routeId)
        {
            var stop = await _context.IntermediateStops.FindAsync(stopId);
            if (stop == null)
            {
                return NotFound();
            }

            _context.IntermediateStops.Remove(stop);
            await _context.SaveChangesAsync();

            var remainingStops = await _context.IntermediateStops
                .Where(s => s.RouteId == routeId)
                .OrderBy(s => s.SequenceOrder)
                .ToListAsync();

            for (int i = 0; i < remainingStops.Count; i++)
            {
                remainingStops[i].SequenceOrder = i + 1;
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Stop removed successfully!";
            return RedirectToAction(nameof(ManageStops), new { id = routeId });
        }

        // POST: Routes/UpdateStopsOrder
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStopsOrder(int routeId, Dictionary<int, int> stopOrders)
        {
            if (stopOrders == null || stopOrders.Count == 0)
            {
                return RedirectToAction(nameof(ManageStops), new { id = routeId });
            }

            foreach (var item in stopOrders)
            {
                var stop = await _context.IntermediateStops.FindAsync(item.Key);
                if (stop != null && stop.RouteId == routeId)
                {
                    stop.SequenceOrder = item.Value;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Stop sequence updated successfully!";
            return RedirectToAction(nameof(ManageStops), new { id = routeId });
        }

        // GET: Routes/ManagePoints/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManagePoints(int id)
        {
            var route = await _routeRepo.GetRouteWithDetailsAsync(id);
            if (route == null)
            {
                return NotFound();
            }

            var routeStations = new List<Station>();
            if (route.OriginStation != null) routeStations.Add(route.OriginStation);
            foreach (var stop in route.IntermediateStops.OrderBy(s => s.SequenceOrder))
            {
                if (stop.Station != null && !routeStations.Any(s => s.StationId == stop.StationId))
                    routeStations.Add(stop.Station);
            }
            if (route.DestinationStation != null && !routeStations.Any(s => s.StationId == route.DestinationStationId))
                routeStations.Add(route.DestinationStation);

            ViewBag.RouteStations = new SelectList(routeStations, "StationId", "StationName");
            return View(route);
        }

        // POST: Routes/AddPoint
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPoint(int routeId, int stationId, string pointName, string pointType)
        {
            if (string.IsNullOrWhiteSpace(pointName))
            {
                TempData["ErrorMessage"] = "Point Name is required.";
                return RedirectToAction(nameof(ManagePoints), new { id = routeId });
            }

            if (pointType.ToLower() == "pickup")
            {
                var pickup = new PickupPoint
                {
                    RouteId = routeId,
                    StationId = stationId,
                    PointName = pointName.Trim()
                };
                _context.PickupPoints.Add(pickup);
            }
            else if (pointType.ToLower() == "dropoff")
            {
                var dropoff = new DropOffPoint
                {
                    RouteId = routeId,
                    StationId = stationId,
                    PointName = pointName.Trim()
                };
                _context.DropOffPoints.Add(dropoff);
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid Point Type.";
                return RedirectToAction(nameof(ManagePoints), new { id = routeId });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Point added successfully!";
            return RedirectToAction(nameof(ManagePoints), new { id = routeId });
        }

        // POST: Routes/RemovePoint
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePoint(int pointId, string pointType, int routeId)
        {
            if (pointType.ToLower() == "pickup")
            {
                var pickup = await _context.PickupPoints.FindAsync(pointId);
                if (pickup != null)
                {
                    _context.PickupPoints.Remove(pickup);
                }
            }
            else if (pointType.ToLower() == "dropoff")
            {
                var dropoff = await _context.DropOffPoints.FindAsync(pointId);
                if (dropoff != null)
                {
                    _context.DropOffPoints.Remove(dropoff);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid Point Type.";
                return RedirectToAction(nameof(ManagePoints), new { id = routeId });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Point removed successfully!";
            return RedirectToAction(nameof(ManagePoints), new { id = routeId });
        }
    }
}
