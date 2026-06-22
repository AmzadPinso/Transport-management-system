using System.ComponentModel.DataAnnotations;

namespace Transport_Management_System.Models
{
    public class Station
    {
        [Key]
        public int StationId { get; set; }

        [Required(ErrorMessage = "Station Name is required")]
        [StringLength(150)]
        [Display(Name = "Station Name")]
        public string StationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string? District { get; set; }

        [Required(ErrorMessage = "Full Address is required")]
        [StringLength(300)]
        [Display(Name = "Full Address")]
        public string Address { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public ICollection<Route> OriginRoutes { get; set; } = new List<Route>();
        public ICollection<Route> DestinationRoutes { get; set; } = new List<Route>();
        public ICollection<IntermediateStop> IntermediateStops { get; set; } = new List<IntermediateStop>();
        public ICollection<PickupPoint> PickupPoints { get; set; } = new List<PickupPoint>();
        public ICollection<DropOffPoint> DropOffPoints { get; set; } = new List<DropOffPoint>();
    }
}
