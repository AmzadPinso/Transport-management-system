using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;
using Transport_Management_System.Services;

namespace Transport_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly IReportsService _reportsService;
        private readonly IBookingRepository _bookingRepo;
        private readonly IExpenseRepository _expenseRepo;
        private readonly AppDbContext _context;

        public ReportsController(
            IReportsService reportsService,
            IBookingRepository bookingRepo,
            IExpenseRepository expenseRepo,
            AppDbContext context)
        {
            _reportsService = reportsService;
            _bookingRepo = bookingRepo;
            _expenseRepo = expenseRepo;
            _context = context;
        }

        // GET: Reports
        public IActionResult Index()
        {
            ViewData["Title"] = "Reports & Analytics Center";
            return View();
        }

        // GET: Reports/Revenue
        public async Task<IActionResult> Revenue(DateTime? startDate, DateTime? endDate)
        {
            ViewData["Title"] = "Financial & Revenue Reports";

            var summary = await _reportsService.GetRevenueSummaryAsync(startDate, endDate);
            var trend = await _reportsService.GetRevenueTrendAsync(startDate, endDate);
            var routeRevenue = await _reportsService.GetRevenueByRouteAsync(startDate, endDate);
            var distribution = await _reportsService.GetRevenueDistributionAsync(startDate, endDate);

            var model = new RevenueReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                Summary = summary,
                Trend = trend,
                RouteRevenue = routeRevenue,
                Distribution = distribution
            };

            return View(model);
        }

        // GET: Reports/VehicleUtilization
        public async Task<IActionResult> VehicleUtilization()
        {
            ViewData["Title"] = "Vehicle Utilization & Fleet Analytics";
            var model = await _reportsService.GetVehicleUtilizationAsync();
            return View(model);
        }

        // GET: Reports/Bookings
        public async Task<IActionResult> Bookings(DateTime? startDate, DateTime? endDate, BookingStatus? status)
        {
            ViewData["Title"] = "Passenger Booking Reports";

            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                        .ThenInclude(r => r!.OriginStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                        .ThenInclude(r => r!.DestinationStation)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(b => b.BookingDate.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                query = query.Where(b => b.BookingDate.Date <= endDate.Value.Date);
            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);

            var bookings = await query.OrderByDescending(b => b.BookingDate).ToListAsync();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;

            return View(bookings);
        }

        // GET: Reports/Expenses
        public async Task<IActionResult> Expenses(DateTime? startDate, DateTime? endDate, ExpenseCategory? category)
        {
            ViewData["Title"] = "Expense Reports & Audits";

            var query = _context.Expenses
                .Include(e => e.Vehicle)
                .Include(e => e.CreatedByUser)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.ExpenseDate.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                query = query.Where(e => e.ExpenseDate.Date <= endDate.Value.Date);
            if (category.HasValue)
                query = query.Where(e => e.ExpenseCategory == category.Value);

            var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();

            // Additional stats for expenses view
            ViewBag.TotalExpenseAmount = expenses.Sum(e => e.Amount);
            ViewBag.CategorySummary = expenses
                .GroupBy(e => e.ExpenseCategory)
                .Select(g => new { Category = g.Key.ToString(), Amount = g.Sum(e => e.Amount) })
                .ToList();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Category = category;

            return View(expenses);
        }

        // GET: Reports/Maintenance
        public async Task<IActionResult> Maintenance(DateTime? startDate, DateTime? endDate, MaintenanceStatus? status)
        {
            ViewData["Title"] = "Maintenance Reports & Fleet Health";

            var query = _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(m => m.ServiceDate.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                query = query.Where(m => m.ServiceDate.Date <= endDate.Value.Date);
            if (status.HasValue)
                query = query.Where(m => m.Status == status.Value);

            var records = await query.OrderByDescending(m => m.ServiceDate).ToListAsync();

            ViewBag.TotalCost = records.Sum(r => r.Cost);
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;

            return View(records);
        }
    }

    public class RevenueReportViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public RevenueSummaryViewModel Summary { get; set; } = new();
        public List<RevenueTrendPoint> Trend { get; set; } = new();
        public List<RouteRevenuePoint> RouteRevenue { get; set; } = new();
        public List<RevenueDistributionPoint> Distribution { get; set; } = new();
    }
}
