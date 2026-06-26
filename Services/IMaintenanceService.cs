using Transport_Management_System.Models;

namespace Transport_Management_System.Services
{
    public interface IMaintenanceService
    {
        Task<MaintenanceAlertViewModel> GetMaintenanceAlertsAsync();
    }

    public class MaintenanceAlertViewModel
    {
        public int OverdueCount { get; set; }
        public int UpcomingCount { get; set; }
        public int InProgressCount { get; set; }
        public List<VehicleMaintenanceAlert> Alerts { get; set; } = new List<VehicleMaintenanceAlert>();
    }

    public class VehicleMaintenanceAlert
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty; // "Overdue" or "Upcoming"
        public string Description { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
    }
}
