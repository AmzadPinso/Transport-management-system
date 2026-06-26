using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly AppDbContext _context;
        private readonly IMaintenanceRepository _maintenanceRepo;

        public MaintenanceService(AppDbContext context, IMaintenanceRepository maintenanceRepo)
        {
            _context = context;
            _maintenanceRepo = maintenanceRepo;
        }

        public async Task<MaintenanceAlertViewModel> GetMaintenanceAlertsAsync()
        {
            var today = DateTime.Today;
            var upcomingThreshold = today.AddDays(7);

            var vm = new MaintenanceAlertViewModel();

            // 1. Fetch all active vehicles to check 90-day overdue alert
            var vehicles = await _context.Vehicles
                .Where(v => v.Status == VehicleStatus.Active)
                .ToListAsync();

            var overdueVehicleIds = new HashSet<int>();

            foreach (var vehicle in vehicles)
            {
                var daysSinceLastService = (today - vehicle.LastServiceDate).TotalDays;
                if (daysSinceLastService > 90)
                {
                    overdueVehicleIds.Add(vehicle.VehicleId);
                    vm.Alerts.Add(new VehicleMaintenanceAlert
                    {
                        VehicleId = vehicle.VehicleId,
                        VehicleName = vehicle.VehicleName,
                        VehicleNumber = vehicle.VehicleNumber,
                        AlertType = "Overdue",
                        Description = $"No service recorded for {(int)daysSinceLastService} days (Frequency: 90 days limit reached).",
                        TargetDate = vehicle.LastServiceDate.AddDays(90)
                    });
                }
            }

            // 2. Fetch specific maintenance records that are Overdue or Upcoming
            var activeRecords = await _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .Where(m => m.Status != MaintenanceStatus.Completed && m.Status != MaintenanceStatus.Cancelled)
                .ToListAsync();

            foreach (var record in activeRecords)
            {
                if (record.Vehicle == null) continue;

                if (record.NextServiceDate < today)
                {
                    // Check if we already registered an alert for this vehicle. If so, update description or add it if not exists.
                    if (!overdueVehicleIds.Contains(record.VehicleId))
                    {
                        overdueVehicleIds.Add(record.VehicleId);
                        vm.Alerts.Add(new VehicleMaintenanceAlert
                        {
                            VehicleId = record.VehicleId,
                            VehicleName = record.Vehicle.VehicleName,
                            VehicleNumber = record.Vehicle.VehicleNumber,
                            AlertType = "Overdue",
                            Description = $"Scheduled {record.MaintenanceType} is overdue since {record.NextServiceDate.ToString("yyyy-MM-dd")}.",
                            TargetDate = record.NextServiceDate
                        });
                    }
                }
                else if (record.NextServiceDate <= upcomingThreshold)
                {
                    vm.Alerts.Add(new VehicleMaintenanceAlert
                    {
                        VehicleId = record.VehicleId,
                        VehicleName = record.Vehicle.VehicleName,
                        VehicleNumber = record.Vehicle.VehicleNumber,
                        AlertType = "Upcoming",
                        Description = $"Upcoming scheduled {record.MaintenanceType} on {record.NextServiceDate.ToString("yyyy-MM-dd")}.",
                        TargetDate = record.NextServiceDate
                    });
                }
            }

            // Summarize counts
            vm.OverdueCount = vm.Alerts.Count(a => a.AlertType == "Overdue");
            vm.UpcomingCount = vm.Alerts.Count(a => a.AlertType == "Upcoming");
            vm.InProgressCount = await _maintenanceRepo.GetInProgressCountAsync();

            // Order alerts: Overdue first, then upcoming by date
            vm.Alerts = vm.Alerts
                .OrderBy(a => a.AlertType != "Overdue")
                .ThenBy(a => a.TargetDate)
                .ToList();

            return vm;
        }
    }
}
