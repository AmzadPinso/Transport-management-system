using System.ComponentModel.DataAnnotations;
using Transport_Management_System.Models;

namespace Transport_Management_System.Models.ViewModels
{
    public class BookingConfirmViewModel
    {
        public int TripId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string? Remarks { get; set; }

        // Display-only trip info (populated by controller)
        public Trip? Trip { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string PassengerEmail { get; set; } = string.Empty;
    }
}
