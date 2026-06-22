using System.ComponentModel.DataAnnotations;

namespace Transport_Management_System.Models
{
    public enum PaymentStatus
    {
        [Display(Name = "Pending")]
        Pending,

        [Display(Name = "Paid")]
        Paid,

        [Display(Name = "Failed")]
        Failed,

        [Display(Name = "Refunded")]
        Refunded
    }
}
