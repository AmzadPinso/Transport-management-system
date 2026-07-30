using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Transport_Management_System.Models;
using Transport_Management_System.Models.ViewModels;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DriverIssuesController : Controller
    {
        private readonly IDriverIssueRepository _driverIssueRepo;
        private readonly IDriverRepository _driverRepo;
        private readonly IVehicleRepository _vehicleRepo;

        public DriverIssuesController(
            IDriverIssueRepository driverIssueRepo,
            IDriverRepository driverRepo,
            IVehicleRepository vehicleRepo)
        {
            _driverIssueRepo = driverIssueRepo;
            _driverRepo = driverRepo;
            _vehicleRepo = vehicleRepo;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId");
            return int.TryParse(claim, out var id) ? id : 0;
        }

        // GET: /DriverIssues
        public async Task<IActionResult> Index(
            string? search,
            int? vehicleId,
            int? driverId,
            IssueStatus? status,
            IssuePriority? priority,
            int pageNumber = 1,
            int pageSize = 10)
        {
            // If standard user, they can see all issues but only resolve/edit their own if they logged them.
            // But let's show all or filter if needed. Typically standard users can report and view.
            int? filterDriverId = driverId;
            
            // Optional: if the logged-in user is a driver (matched by email), we could auto-filter.
            // Let's support general view with standard filters.
            var (issues, totalCount) = await _driverIssueRepo.GetPagedAsync(
                search, vehicleId, filterDriverId, status, priority, pageNumber, pageSize);

            var vehicles = await _vehicleRepo.GetAllAsync();
            var (drivers, _) = await _driverRepo.GetDriversPagedAsync("", null, "", 1, 1000, "FullName", "asc");

            ViewBag.Search = search;
            ViewBag.SelectedVehicleId = vehicleId;
            ViewBag.SelectedDriverId = driverId;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedPriority = priority;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            ViewBag.VehicleList = new SelectList(vehicles, "VehicleId", "VehicleName", vehicleId);
            ViewBag.DriverList = new SelectList(drivers, "DriverId", "FullName", driverId);

            ViewData["Title"] = "Driver Issue Logbook";
            return View(issues);
        }

        // GET: /DriverIssues/Create
        public async Task<IActionResult> Create()
        {
            var vehicles = await _vehicleRepo.GetAllAsync();
            var (drivers, _) = await _driverRepo.GetDriversPagedAsync("", null, "", 1, 1000, "FullName", "asc");

            var vm = new DriverIssueFormViewModel
            {
                VehicleList = vehicles.Select(v => new SelectListItem
                {
                    Value = v.VehicleId.ToString(),
                    Text = $"{v.VehicleName} ({v.VehicleNumber})"
                }),
                DriverList = drivers.Select(d => new SelectListItem
                {
                    Value = d.DriverId.ToString(),
                    Text = d.FullName
                })
            };

            ViewData["Title"] = "Report New Issue";
            return View(vm);
        }

        // POST: /DriverIssues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DriverIssueFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var issue = new DriverIssue
                {
                    DriverId = model.DriverId,
                    VehicleId = model.VehicleId,
                    IssueCategory = model.IssueCategory,
                    IssueDescription = model.IssueDescription,
                    ReportDate = model.ReportDate,
                    Priority = model.Priority,
                    Status = IssueStatus.Open,
                    ReportedByUserId = GetCurrentUserId(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _driverIssueRepo.AddAsync(issue);
                await _driverIssueRepo.SaveAsync();

                TempData["SuccessMessage"] = "Issue reported successfully. Maintenance team has been notified.";
                return RedirectToAction(nameof(Index));
            }

            var vehicles = await _vehicleRepo.GetAllAsync();
            var (drivers, _) = await _driverRepo.GetDriversPagedAsync("", null, "", 1, 1000, "FullName", "asc");

            model.VehicleList = vehicles.Select(v => new SelectListItem
            {
                Value = v.VehicleId.ToString(),
                Text = $"{v.VehicleName} ({v.VehicleNumber})"
            });
            model.DriverList = drivers.Select(d => new SelectListItem
            {
                Value = d.DriverId.ToString(),
                Text = d.FullName
            });

            ViewData["Title"] = "Report New Issue";
            return View(model);
        }

        // GET: /DriverIssues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var issue = await _driverIssueRepo.GetByIdAsync(id.Value);
            if (issue == null) return NotFound();

            ViewData["Title"] = "Issue Details";
            return View(issue);
        }

        // GET: /DriverIssues/UpdateStatus/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int? id)
        {
            if (id == null) return NotFound();

            var issue = await _driverIssueRepo.GetByIdAsync(id.Value);
            if (issue == null) return NotFound();

            var vm = new UpdateIssueStatusViewModel
            {
                DriverIssueId = issue.DriverIssueId,
                Status = issue.Status,
                ResolutionNotes = issue.ResolutionNotes
            };

            ViewBag.Issue = issue;
            ViewData["Title"] = "Update Issue Status";
            return View(vm);
        }

        // POST: /DriverIssues/UpdateStatus/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, UpdateIssueStatusViewModel model)
        {
            if (id != model.DriverIssueId) return NotFound();

            if (ModelState.IsValid)
            {
                var issue = await _driverIssueRepo.GetByIdAsync(id);
                if (issue == null) return NotFound();

                issue.Status = model.Status;
                issue.ResolutionNotes = model.ResolutionNotes;
                issue.UpdatedAt = DateTime.Now;

                if (model.Status == IssueStatus.Resolved || model.Status == IssueStatus.Closed)
                {
                    issue.ResolvedAt = DateTime.Now;
                }
                else
                {
                    issue.ResolvedAt = null;
                }

                _driverIssueRepo.Update(issue);
                await _driverIssueRepo.SaveAsync();

                TempData["SuccessMessage"] = "Issue status updated successfully.";
                return RedirectToAction(nameof(Details), new { id = issue.DriverIssueId });
            }

            ViewBag.Issue = await _driverIssueRepo.GetByIdAsync(id);
            ViewData["Title"] = "Update Issue Status";
            return View(model);
        }

        // POST: /DriverIssues/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var issue = await _driverIssueRepo.GetByIdAsync(id);
            if (issue == null) return NotFound();

            _driverIssueRepo.Delete(issue);
            await _driverIssueRepo.SaveAsync();

            TempData["SuccessMessage"] = "Issue deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
