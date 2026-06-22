using System.ComponentModel.DataAnnotations;

namespace Transport_Management_System.Models
{
    public enum BookingStatus
    {
        [Display(Name = "Pending")]
        Pending,

        [Display(Name = "Confirmed")]
        Confirmed,

        [Display(Name = "Cancelled")]
        Cancelled,

        [Display(Name = "Completed")]
        Completed
    }
}
