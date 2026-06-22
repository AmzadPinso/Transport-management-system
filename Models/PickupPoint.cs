using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport_Management_System.Models
{
    public class PickupPoint
    {
        [Key]
        public int PickupPointId { get; set; }

        [Required]
        public int RouteId { get; set; }
        
        [ForeignKey("RouteId")]
        public Route? Route { get; set; }

        [Required]
        [Display(Name = "Station")]
        public int StationId { get; set; }
        
        [ForeignKey("StationId")]
        public Station? Station { get; set; }

        [Required(ErrorMessage = "Pickup point name is required")]
        [StringLength(150)]
        [Display(Name = "Pickup Point Name")]
        public string PointName { get; set; } = string.Empty;
    }
}
