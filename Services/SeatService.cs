namespace Transport_Management_System.Services
{
    public class SeatService : ISeatService
    {
        private static readonly string[] RowLabels = 
            { "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T" };

        // Standard bus layout: 2 left | aisle | 2 right = 4 seats per row
        private const int SeatsPerRow = 4;

        public List<List<SeatInfo>> GenerateSeatLayout(int capacity, IEnumerable<string> bookedSeats)
        {
            var booked = new HashSet<string>(bookedSeats, StringComparer.OrdinalIgnoreCase);
            var layout = new List<List<SeatInfo>>();

            int totalRows = (int)Math.Ceiling((double)capacity / SeatsPerRow);

            for (int r = 0; r < totalRows && r < RowLabels.Length; r++)
            {
                var row = new List<SeatInfo>();
                for (int c = 1; c <= SeatsPerRow; c++)
                {
                    // Only create as many seats as vehicle capacity allows
                    int seatIndex = r * SeatsPerRow + c;
                    if (seatIndex > capacity) break;

                    var seatNumber = $"{RowLabels[r]}{c}";
                    row.Add(new SeatInfo
                    {
                        SeatNumber = seatNumber,
                        IsBooked   = booked.Contains(seatNumber),
                        Row        = r,
                        Col        = c
                    });
                }
                layout.Add(row);
            }

            return layout;
        }

        public string GenerateBookingReference()
        {
            var datePart   = DateTime.Now.ToString("yyyyMMdd");
            var randomPart = new Random().Next(1000, 9999).ToString();
            return $"BK-{datePart}-{randomPart}";
        }
    }
}
