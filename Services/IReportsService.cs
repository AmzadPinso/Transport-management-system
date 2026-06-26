using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Transport_Management_System.Models;

namespace Transport_Management_System.Services
{
    public interface IReportsService
    {
        Task<RevenueSummaryViewModel> GetRevenueSummaryAsync(DateTime? startDate, DateTime? endDate);
        Task<List<RevenueTrendPoint>> GetRevenueTrendAsync(DateTime? startDate, DateTime? endDate);
        Task<List<RouteRevenuePoint>> GetRevenueByRouteAsync(DateTime? startDate, DateTime? endDate);
        Task<List<RevenueDistributionPoint>> GetRevenueDistributionAsync(DateTime? startDate, DateTime? endDate);
        Task<VehicleUtilizationViewModel> GetVehicleUtilizationAsync();
        Task<ExecutiveDashboardViewModel> GetExecutiveDashboardAsync();
    }

    public class RevenueSummaryViewModel
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetRevenue => TotalIncome - TotalExpenses;
        public decimal DailyRevenue { get; set; }
        public decimal WeeklyRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }
    }

    public class RevenueTrendPoint
    {
        public string Period { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
    }

    public class RouteRevenuePoint
    {
        public string RouteName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class RevenueDistributionPoint
    {
        public string Category { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class VehicleUtilizationViewModel
    {
        public int TotalVehicles { get; set; }
        public double AvailabilityRate { get; set; }
        public List<VehicleUtilizationItem> TopVehicles { get; set; } = new();
        public List<VehicleUtilizationItem> UnderutilizedVehicles { get; set; } = new();
        public List<VehicleMaintenanceFrequencyPoint> MaintenanceFrequency { get; set; } = new();
    }

    public class VehicleUtilizationItem
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public int TotalTrips { get; set; }
        public double UtilizationRate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class VehicleMaintenanceFrequencyPoint
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public int MaintenanceCount { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
    }

    public class ExecutiveDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalDrivers { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalRoutes { get; set; }
        public int TotalTrips { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public int MaintenanceAlertsCount { get; set; }
        public int OpenDriverIssuesCount { get; set; }

        public List<Booking> RecentBookings { get; set; } = new();
        public List<Trip> RecentTrips { get; set; } = new();
        public List<Trip> UpcomingTrips { get; set; } = new();
        public List<MaintenanceRecord> RecentMaintenanceActivities { get; set; } = new();
        public List<DriverIssue> RecentDriverReports { get; set; } = new();
    }
}
