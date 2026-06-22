using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport_Management_System.Models
{
    public class IntermediateStop
    {
        [Key]
        public int IntermediateStopId { get; set; }

        [Required]
        public int RouteId { get; set; }
        
        [ForeignKey("RouteId")]
        public Route? Route { get; set; }

        [Required]
        [Display(Name = "Station")]
        public int StationId { get; set; }
        
        [ForeignKey("StationId")]
        public Station? Station { get; set; }

        [Required]
        [Display(Name = "Sequence Order")]
        public int SequenceOrder { get; set; }
    }
}
