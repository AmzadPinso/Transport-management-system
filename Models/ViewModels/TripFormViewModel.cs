using System.ComponentModel.DataAnnotations;
using Transport_Management_System.Models;

namespace Transport_Management_System.Models.ViewModels
{
    public class TripFormViewModel
    {
        public int? TripId { get; set; }

        [Required(ErrorMessage = "Trip Name/Number is required")]
        [StringLength(100, ErrorMessage = "Trip Name cannot exceed 100 characters")]
        [Display(Name = "Trip Name / Number")]
        public string TripName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Route selection is required")]
        [Display(Name = "Select Route")]
        public int RouteId { get; set; }

        [Required(ErrorMessage = "Vehicle assignment is required")]
        [Display(Name = "Select Vehicle")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Driver assignment is required")]
        [Display(Name = "Select Driver")]
        public int DriverId { get; set; }

        [Required(ErrorMessage = "Departure Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Departure Date")]
        public DateTime DepartureDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Departure Time is required")]
        [DataType(DataType.Time)]
        [Display(Name = "Departure Time")]
        public TimeSpan DepartureTime { get; set; }

        [Required(ErrorMessage = "Estimated Arrival Time is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Estimated Arrival Date & Time")]
        public DateTime EstimatedArrivalTime { get; set; } = DateTime.Now.AddHours(2);

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        public string? Remarks { get; set; }

        [Required(ErrorMessage = "Ticket Price is required")]
        [Display(Name = "Ticket Price (৳)")]
        [Range(0, 10000, ErrorMessage = "Ticket Price must be between 0 and 10000")]
        public decimal TicketPrice { get; set; } = 0;

        [Display(Name = "Trip Status")]
        public TripStatus Status { get; set; } = TripStatus.Scheduled;
    }
}
