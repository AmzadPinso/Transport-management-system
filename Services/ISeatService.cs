namespace Transport_Management_System.Services
{
    public class SeatInfo
    {
        public string SeatNumber { get; set; } = string.Empty;
        public bool IsBooked { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
    }

    public interface ISeatService
    {
        /// <summary>
        /// Generates a seat layout grid for a given vehicle capacity.
        /// Returns a 2D list of SeatInfo representing the seat map.
        /// </summary>
        List<List<SeatInfo>> GenerateSeatLayout(int capacity, IEnumerable<string> bookedSeats);

        /// <summary>Generates a unique booking reference number.</summary>
        string GenerateBookingReference();
    }
}
