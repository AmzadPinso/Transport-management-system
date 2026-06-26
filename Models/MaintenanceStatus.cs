using System.ComponentModel.DataAnnotations;

namespace Transport_Management_System.Models
{
    public enum MaintenanceStatus
    {
        [Display(Name = "Scheduled")]
        Scheduled,

        [Display(Name = "In Progress")]
        InProgress,

        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "Overdue")]
        Overdue,

        [Display(Name = "Cancelled")]
        Cancelled
    }
}
