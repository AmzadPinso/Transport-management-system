using System.ComponentModel.DataAnnotations;
using Transport_Management_System.Models;

namespace Transport_Management_System.Models.ViewModels
{
    public class TripSearchViewModel
    {
        [Display(Name = "From (Origin)")]
        public int? OriginStationId { get; set; }

        [Display(Name = "To (Destination)")]
        public int? DestinationStationId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Departure Date")]
        public DateTime? DepartureDate { get; set; }

        // Populated by controller for dropdowns
        public IEnumerable<Station> Stations { get; set; } = new List<Station>();

        // Populated after search
        public IEnumerable<Trip>? SearchResults { get; set; }
    }
}
