using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport_Management_System.Models
{
    public class Trip
    {
        [Key]
        public int TripId { get; set; }

        [Required(ErrorMessage = "Trip Name / Number is required")]
        [StringLength(100)]
        [Display(Name = "Trip Name / Number")]
        public string TripName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Route selection is required")]
        [Display(Name = "Route")]
        public int RouteId { get; set; }

        [ForeignKey("RouteId")]
        public Route? Route { get; set; }

        [Required(ErrorMessage = "Vehicle assignment is required")]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        [Required(ErrorMessage = "Driver assignment is required")]
        [Display(Name = "Driver")]
        public int DriverId { get; set; }

        [ForeignKey("DriverId")]
        public Driver? Driver { get; set; }

        [Required(ErrorMessage = "Departure Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Departure Date")]
        public DateTime DepartureDate { get; set; }

        [Required(ErrorMessage = "Departure Time is required")]
        [DataType(DataType.Time)]
        [Display(Name = "Departure Time")]
        public TimeSpan DepartureTime { get; set; }

        [Required(ErrorMessage = "Estimated Arrival Time is required")]
        [Display(Name = "Estimated Arrival Time")]
        public DateTime EstimatedArrivalTime { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Available capacity cannot be negative")]
        [Display(Name = "Available Capacity")]
        public int AvailableCapacity { get; set; }

        [Required]
        public TripStatus Status { get; set; } = TripStatus.Scheduled;

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
