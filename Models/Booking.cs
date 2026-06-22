using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport_Management_System.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Booking Reference")]
        public string BookingReference { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Passenger")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [Display(Name = "Trip")]
        public int TripId { get; set; }

        [ForeignKey("TripId")]
        public Trip? Trip { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Seat Number")]
        public string SeatNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Booking Status")]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [Required]
        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [Range(0, double.MaxValue)]
        [Display(Name = "Total Amount (BDT)")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; } = 0;

        [StringLength(300)]
        [Display(Name = "Cancellation Reason")]
        public string? CancellationReason { get; set; }

        [Display(Name = "Cancelled At")]
        public DateTime? CancelledAt { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
