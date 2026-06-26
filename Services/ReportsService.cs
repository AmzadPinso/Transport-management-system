using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;

namespace Transport_Management_System.Services
{
    public class ReportsService : IReportsService
    {
        private readonly AppDbContext _context;
        private readonly IMaintenanceService _maintenanceService;

        public ReportsService(AppDbContext context, IMaintenanceService maintenanceService)
        {
            _context = context;
            _maintenanceService = maintenanceService;
        }

        public async Task<RevenueSummaryViewModel> GetRevenueSummaryAsync(DateTime? startDate, DateTime? endDate)
        {
            var summary = new RevenueSummaryViewModel();
            var today = DateTime.Today;

            // 1. Booking Income (Only paid bookings)
            var bookingsQuery = _context.Bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid);
            if (startDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate >= startDate.Value);
            if (endDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate <= endDate.Value);

            summary.TotalIncome = await bookingsQuery.SumAsync(b => b.TotalAmount);

            // 2. Expenses (Operational Expenses + Maintenance cost)
            var expensesQuery = _context.Expenses.AsQueryable();
            if (startDate.HasValue)
                expensesQuery = expensesQuery.Where(e => e.ExpenseDate >= startDate.Value);
            if (endDate.HasValue)
                expensesQuery = expensesQuery.Where(e => e.ExpenseDate <= endDate.Value);

            var opExpenses = await expensesQuery.SumAsync(e => e.Amount);

            var maintenanceQuery = _context.MaintenanceRecords.Where(m => m.Status == MaintenanceStatus.Completed);
            if (startDate.HasValue)
                maintenanceQuery = maintenanceQuery.Where(m => m.ServiceDate >= startDate.Value);
            if (endDate.HasValue)
                maintenanceQuery = maintenanceQuery.Where(m => m.ServiceDate <= endDate.Value);

            var maintExpenses = await maintenanceQuery.SumAsync(m => m.Cost);

            summary.TotalExpenses = opExpenses + maintExpenses;

            // 3. Time-based Income Cards (ignore the custom range filters to keep KPIs stable, or apply where relevant)
            var dailyQuery = _context.Bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid && b.BookingDate.Date == today);
            summary.DailyRevenue = await dailyQuery.SumAsync(b => b.TotalAmount);

            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var weeklyQuery = _context.Bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid && b.BookingDate.Date >= startOfWeek);
            summary.WeeklyRevenue = await weeklyQuery.SumAsync(b => b.TotalAmount);

            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var monthlyQuery = _context.Bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid && b.BookingDate.Date >= startOfMonth);
            summary.MonthlyRevenue = await monthlyQuery.SumAsync(b => b.TotalAmount);

            var startOfYear = new DateTime(today.Year, 1, 1);
            var yearlyQuery = _context.Bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid && b.BookingDate.Date >= startOfYear);
            summary.YearlyRevenue = await yearlyQuery.SumAsync(b => b.TotalAmount);

            return summary;
        }

        public async Task<List<RevenueTrendPoint>> GetRevenueTrendAsync(DateTime? startDate, DateTime? endDate)
        {
            var points = new List<RevenueTrendPoint>();
            var today = DateTime.Today;

            // Default to last 6 months if not specified
            var start = startDate ?? today.AddMonths(-5).AddDays(-today.Day + 1); // Start of 6 months ago
            var end = endDate ?? today;

            // Loop through each month in the range
            var currentMonth = new DateTime(start.Year, start.Month, 1);
            var endMonth = new DateTime(end.Year, end.Month, 1);

            while (currentMonth <= endMonth)
            {
                var nextMonth = currentMonth.AddMonths(1);
                var label = currentMonth.ToString("MMM yyyy");

                // Booking revenue in this month
                var monthlyIncome = await _context.Bookings
                    .Where(b => b.PaymentStatus == PaymentStatus.Paid && b.BookingDate >= currentMonth && b.BookingDate < nextMonth)
                    .SumAsync(b => b.TotalAmount);

                // Expenses in this month
                var monthlyOpExpenses = await _context.Expenses
                    .Where(e => e.ExpenseDate >= currentMonth && e.ExpenseDate < nextMonth)
                    .SumAsync(e => e.Amount);

                var monthlyMaintExpenses = await _context.MaintenanceRecords
                    .Where(m => m.Status == MaintenanceStatus.Completed && m.ServiceDate >= currentMonth && m.ServiceDate < nextMonth)
                    .SumAsync(m => m.Cost);

                points.Add(new RevenueTrendPoint
                {
                    Period = label,
                    Income = monthlyIncome,
                    Expenses = monthlyOpExpenses + monthlyMaintExpenses
                });

                currentMonth = nextMonth;
            }

            return points;
        }

        public async Task<List<RouteRevenuePoint>> GetRevenueByRouteAsync(DateTime? startDate, DateTime? endDate)
        {
            var bookingsQuery = _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                .Where(b => b.PaymentStatus == PaymentStatus.Paid && b.Trip != null && b.Trip.Route != null);

            if (startDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate >= startDate.Value);
            if (endDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate <= endDate.Value);

            var grouped = await bookingsQuery
                .GroupBy(b => b.Trip!.Route!.RouteName)
                .Select(g => new RouteRevenuePoint
                {
                    RouteName = g.Key,
                    Revenue = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(r => r.Revenue)
                .Take(10) // Limit to top 10 routes
                .ToListAsync();

            return grouped;
        }

        public async Task<List<RevenueDistributionPoint>> GetRevenueDistributionAsync(DateTime? startDate, DateTime? endDate)
        {
            // Distribution of booking revenue by Vehicle Type
            var bookingsQuery = _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Vehicle)
                .Where(b => b.PaymentStatus == PaymentStatus.Paid && b.Trip != null && b.Trip.Vehicle != null);

            if (startDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate >= startDate.Value);
            if (endDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate <= endDate.Value);

            var grouped = await bookingsQuery
                .GroupBy(b => b.Trip!.Vehicle!.VehicleType)
                .Select(g => new RevenueDistributionPoint
                {
                    Category = g.Key.ToString(),
                    Value = g.Sum(b => b.TotalAmount)
                })
                .ToListAsync();

            return grouped;
        }

        public async Task<VehicleUtilizationViewModel> GetVehicleUtilizationAsync()
        {
            var vm = new VehicleUtilizationViewModel();

            var vehicles = await _context.Vehicles.ToListAsync();
            vm.TotalVehicles = vehicles.Count;

            if (vm.TotalVehicles > 0)
            {
                var activeCount = vehicles.Count(v => v.Status == VehicleStatus.Active);
                vm.AvailabilityRate = Math.Round((double)activeCount / vm.TotalVehicles * 100, 1);
            }
            else
            {
                vm.AvailabilityRate = 0;
            }

            // Get total trips count per vehicle
            var tripCounts = await _context.Trips
                .GroupBy(t => t.VehicleId)
                .Select(g => new { VehicleId = g.Key, TripCount = g.Count() })
                .ToDictionaryAsync(x => x.VehicleId, x => x.TripCount);

            // Get maintenance counts and cost per vehicle
            var maintenanceSummary = await _context.MaintenanceRecords
                .Where(m => m.Status == MaintenanceStatus.Completed)
                .GroupBy(m => m.VehicleId)
                .Select(g => new
                {
                    VehicleId = g.Key,
                    Count = g.Count(),
                    TotalCost = g.Sum(m => m.Cost)
                })
                .ToDictionaryAsync(x => x.VehicleId, x => x);

            var items = vehicles.Select(v => new VehicleUtilizationItem
            {
                VehicleId = v.VehicleId,
                VehicleName = v.VehicleName,
                VehicleNumber = v.VehicleNumber,
                TotalTrips = tripCounts.ContainsKey(v.VehicleId) ? tripCounts[v.VehicleId] : 0,
                Status = v.Status.ToString(),
                // Rough utilization metric: ratio of trips to overall trips, capped or normalized
                UtilizationRate = tripCounts.ContainsKey(v.VehicleId) ? Math.Min(tripCounts[v.VehicleId] * 5, 100) : 0 
            }).ToList();

            vm.TopVehicles = items.OrderByDescending(x => x.TotalTrips).Take(5).ToList();
            vm.UnderutilizedVehicles = items.OrderBy(x => x.TotalTrips).Take(5).ToList();

            vm.MaintenanceFrequency = vehicles.Select(v => new VehicleMaintenanceFrequencyPoint
            {
                VehicleId = v.VehicleId,
                VehicleName = v.VehicleName,
                VehicleNumber = v.VehicleNumber,
                MaintenanceCount = maintenanceSummary.ContainsKey(v.VehicleId) ? maintenanceSummary[v.VehicleId].Count : 0,
                TotalMaintenanceCost = maintenanceSummary.ContainsKey(v.VehicleId) ? maintenanceSummary[v.VehicleId].TotalCost : 0
            })
            .OrderByDescending(x => x.MaintenanceCount)
            .Take(10)
            .ToList();

            return vm;
        }

        public async Task<ExecutiveDashboardViewModel> GetExecutiveDashboardAsync()
        {
            var vm = new ExecutiveDashboardViewModel();

            // Totals
            vm.TotalUsers = await _context.Users.CountAsync();
            vm.TotalDrivers = await _context.Drivers.CountAsync();
            vm.TotalVehicles = await _context.Vehicles.CountAsync();
            vm.TotalRoutes = await _context.Routes.CountAsync();
            vm.TotalTrips = await _context.Trips.CountAsync();
            vm.TotalBookings = await _context.Bookings.CountAsync();

            vm.TotalRevenue = await _context.Bookings
                .Where(b => b.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(b => b.TotalAmount);

            var opExpenses = await _context.Expenses.SumAsync(e => e.Amount);
            var maintExpenses = await _context.MaintenanceRecords
                .Where(m => m.Status == MaintenanceStatus.Completed)
                .SumAsync(m => m.Cost);
            vm.TotalExpenses = opExpenses + maintExpenses;

            var alerts = await _maintenanceService.GetMaintenanceAlertsAsync();
            vm.MaintenanceAlertsCount = alerts.OverdueCount + alerts.UpcomingCount;

            vm.OpenDriverIssuesCount = await _context.DriverIssues
                .CountAsync(di => di.Status == IssueStatus.Open || di.Status == IssueStatus.InProgress);

            // Recent Lists
            vm.RecentBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Trip)
                .OrderByDescending(b => b.BookingDate)
                .Take(5)
                .ToListAsync();

            vm.RecentTrips = await _context.Trips
                .Include(t => t.Route)
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            var today = DateTime.Today;
            vm.UpcomingTrips = await _context.Trips
                .Include(t => t.Route)
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Where(t => t.DepartureDate >= today && t.Status == TripStatus.Scheduled)
                .OrderBy(t => t.DepartureDate)
                .ThenBy(t => t.DepartureTime)
                .Take(5)
                .ToListAsync();

            vm.RecentMaintenanceActivities = await _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .OrderByDescending(m => m.ServiceDate)
                .Take(5)
                .ToListAsync();

            vm.RecentDriverReports = await _context.DriverIssues
                .Include(d => d.Driver)
                .Include(d => d.Vehicle)
                .OrderByDescending(d => d.CreatedAt)
                .Take(5)
                .ToListAsync();

            return vm;
        }
    }
}
