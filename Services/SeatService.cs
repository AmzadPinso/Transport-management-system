namespace Transport_Management_System.Services
{
    public class SeatService : ISeatService
    {
        private static readonly string[] RowLabels =
            { "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T" };

        // Standard bus: 2 left | aisle | 2 right = 4 seats per row
        // Window seats are Col 1 and Col 4
        private const int SeatsPerRow = 4;

        // ── Layout generation ──────────────────────────────────────
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
                    int seatIndex = r * SeatsPerRow + c;
                    if (seatIndex > capacity) break;

                    var seatNumber = $"{RowLabels[r]}{c}";
                    row.Add(new SeatInfo
                    {
                        SeatNumber = seatNumber,
                        IsBooked   = booked.Contains(seatNumber),
                        IsWindow   = (c == 1 || c == SeatsPerRow),   // leftmost & rightmost columns
                        Row        = r,
                        Col        = c
                    });
                }
                layout.Add(row);
            }

            return layout;
        }

        // ── Booking reference ──────────────────────────────────────
        public string GenerateBookingReference()
        {
            var datePart   = DateTime.Now.ToString("yyyyMMdd");
            var randomPart = new Random().Next(1000, 9999).ToString();
            return $"BK-{datePart}-{randomPart}";
        }

        // ── Seat recommendation ────────────────────────────────────
        /// <summary>
        /// Rules:
        ///   1. Never recommend an occupied seat.
        ///   2. Single pax → prefer window seats; if none, pick from least-crowded row.
        ///   3. Group pax  → prefer same row, adjacent; if not possible, take closest cluster.
        ///   4. Sets IsRecommended on each matched SeatInfo in the layout.
        /// </summary>
        public SeatRecommendation RecommendSeats(List<List<SeatInfo>> layout, int passengerCount)
        {
            var rec = new SeatRecommendation();

            if (layout == null || !layout.Any() || passengerCount <= 0)
            {
                rec.Message = "No seats available.";
                return rec;
            }

            // Clear any previous recommendations
            foreach (var seat in layout.SelectMany(r => r))
                seat.IsRecommended = false;

            if (passengerCount == 1)
                return RecommendSingle(layout, rec);

            return RecommendGroup(layout, rec, passengerCount);
        }

        // ── Single passenger ──────────────────────────────────────
        private static SeatRecommendation RecommendSingle(List<List<SeatInfo>> layout, SeatRecommendation rec)
        {
            // Rule: window seat first (Col 1 or Col 4), from front to back
            foreach (var row in layout)
            {
                var window = row.FirstOrDefault(s => !s.IsBooked && s.IsWindow);
                if (window != null)
                {
                    window.IsRecommended = true;
                    rec.SuggestedSeats.Add(window.SeatNumber);
                    rec.IsWindow = true;
                    rec.Message  = $"Recommended Seat: {window.SeatNumber} (Window Seat)";
                    return rec;
                }
            }

            // Fallback: least-crowded row → first available seat
            var bestRow = layout
                .Where(row => row.Any(s => !s.IsBooked))
                .OrderBy(row => row.Count(s => s.IsBooked))  // fewest booked = least crowded
                .FirstOrDefault();

            if (bestRow != null)
            {
                var seat = bestRow.First(s => !s.IsBooked);
                seat.IsRecommended = true;
                rec.SuggestedSeats.Add(seat.SeatNumber);
                rec.IsWindow = false;
                rec.Message  = $"Recommended Seat: {seat.SeatNumber}";
                return rec;
            }

            rec.Message = "No seats available.";
            return rec;
        }

        // ── Group passengers ──────────────────────────────────────
        private static SeatRecommendation RecommendGroup(List<List<SeatInfo>> layout, SeatRecommendation rec, int passengerCount)
        {
            rec.IsGroup = true;

            // Strategy 1: Find a row with enough adjacent seats
            foreach (var row in layout)
            {
                var available = row.Where(s => !s.IsBooked).ToList();
                if (available.Count < passengerCount) continue;

                // Try all windows of size=passengerCount within this row (by col order)
                var sorted = available.OrderBy(s => s.Col).ToList();
                for (int start = 0; start <= sorted.Count - passengerCount; start++)
                {
                    var window = sorted.Skip(start).Take(passengerCount).ToList();
                    // Check they are truly adjacent (no gaps in column numbers)
                    bool adjacent = true;
                    for (int k = 1; k < window.Count; k++)
                    {
                        if (window[k].Col - window[k - 1].Col > 1)
                        {
                            adjacent = false;
                            break;
                        }
                    }
                    if (adjacent)
                    {
                        foreach (var s in window) s.IsRecommended = true;
                        rec.SuggestedSeats = window.Select(s => s.SeatNumber).ToList();
                        rec.Message = $"Recommended Group Seats: {string.Join(", ", rec.SuggestedSeats)}";
                        return rec;
                    }
                }

                // No perfectly adjacent block — take any N from this row
                var selected = available.Take(passengerCount).ToList();
                foreach (var s in selected) s.IsRecommended = true;
                rec.SuggestedSeats = selected.Select(s => s.SeatNumber).ToList();
                rec.Message = $"Recommended Group Seats: {string.Join(", ", rec.SuggestedSeats)}";
                return rec;
            }

            // Strategy 2: Cross-row — take the nearest available seats
            var flat = layout.SelectMany(r => r)
                             .Where(s => !s.IsBooked)
                             .Take(passengerCount)
                             .ToList();

            if (flat.Count >= passengerCount)
            {
                foreach (var s in flat) s.IsRecommended = true;
                rec.SuggestedSeats = flat.Select(s => s.SeatNumber).ToList();
                rec.Message = $"Recommended Group Seats: {string.Join(", ", rec.SuggestedSeats)} (split across rows)";
                return rec;
            }

            rec.Message = "Not enough available seats to accommodate the group.";
            return rec;
        }
    }
}
