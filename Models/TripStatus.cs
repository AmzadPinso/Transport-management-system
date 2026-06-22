using System.ComponentModel.DataAnnotations;

namespace Transport_Management_System.Models
{
    public enum TripStatus
    {
        [Display(Name = "Scheduled")]
        Scheduled,

        [Display(Name = "Ready for Dispatch")]
        ReadyForDispatch,

        [Display(Name = "Ongoing")]
        Ongoing,

        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "Cancelled")]
        Cancelled,

        [Display(Name = "Delayed")]
        Delayed
    }
}
