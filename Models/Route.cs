using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport_Management_System.Models
{
    public class Route
    {
        [Key]
        public int RouteId { get; set; }

        [Required(ErrorMessage = "Route Name is required")]
        [StringLength(150)]
        [Display(Name = "Route Name")]
        public string RouteName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Origin Station is required")]
        [Display(Name = "Origin Station")]
        public int OriginStationId { get; set; }
        
        [ForeignKey("OriginStationId")]
        public Station? OriginStation { get; set; }

        [Required(ErrorMessage = "Destination Station is required")]
        [Display(Name = "Destination Station")]
        public int DestinationStationId { get; set; }

        [ForeignKey("DestinationStationId")]
        public Station? DestinationStation { get; set; }

        [Required(ErrorMessage = "Total Distance is required")]
        [Range(0.1, 10000, ErrorMessage = "Distance must be greater than zero")]
        [Display(Name = "Total Distance (km)")]
        public double DistanceKm { get; set; }

        [Required(ErrorMessage = "Estimated Travel Duration is required")]
        [Range(1, 10000, ErrorMessage = "Travel duration must be positive")]
        [Display(Name = "Estimated Travel Duration (minutes)")]
        public int EstimatedDurationMinutes { get; set; }

        [Required]
        [Display(Name = "Route Status")]
        public RouteStatus Status { get; set; } = RouteStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public ICollection<IntermediateStop> IntermediateStops { get; set; } = new List<IntermediateStop>();
        public ICollection<PickupPoint> PickupPoints { get; set; } = new List<PickupPoint>();
        public ICollection<DropOffPoint> DropOffPoints { get; set; } = new List<DropOffPoint>();
    }
}
