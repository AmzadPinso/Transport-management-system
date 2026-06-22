using Transport_Management_System.Models;
using Transport_Management_System.Services;

namespace Transport_Management_System.Models.ViewModels
{
    public class BookingSeatViewModel
    {
        public Trip Trip { get; set; } = null!;
        public List<List<SeatInfo>> SeatLayout { get; set; } = new();
        public string? SelectedSeat { get; set; }
        public int TotalAvailable { get; set; }
        public int TotalBooked { get; set; }
    }
}
