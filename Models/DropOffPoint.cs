using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport_Management_System.Models
{
    public class DropOffPoint
    {
        [Key]
        public int DropOffPointId { get; set; }

        [Required]
        public int RouteId { get; set; }
        
        [ForeignKey("RouteId")]
        public Route? Route { get; set; }

        [Required]
        [Display(Name = "Station")]
        public int StationId { get; set; }
        
        [ForeignKey("StationId")]
        public Station? Station { get; set; }

        [Required(ErrorMessage = "Drop-off point name is required")]
        [StringLength(150)]
        [Display(Name = "Drop-Off Point Name")]
        public string PointName { get; set; } = string.Empty;
    }
}
