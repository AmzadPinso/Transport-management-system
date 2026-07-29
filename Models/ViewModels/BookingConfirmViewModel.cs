using Transport_Management_System.Models;

namespace Transport_Management_System.Models.ViewModels
{
    public class BookingConfirmViewModel
    {
        public int     TripId     { get; set; }
        public string? Remarks    { get; set; }

        /// <summary>Comma-separated seat numbers submitted from SelectSeat form.</summary>
        public string SeatNumbers { get; set; } = string.Empty;

        /// <summary>Convenience list parsed from SeatNumbers.</summary>
        public List<string> SeatList =>
            SeatNumbers
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToList();

        // Display-only (populated by controller)
        public Trip?   Trip           { get; set; }
        public string  PassengerName  { get; set; } = string.Empty;
        public string  PassengerEmail { get; set; } = string.Empty;
    }
}
