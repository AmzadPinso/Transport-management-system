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

        public SeatRecommendation RecommendSeats(List<List<SeatInfo>> layout, int passengerCount)
        {
            var rec = new SeatRecommendation();
            if (layout == null || !layout.Any() || passengerCount <= 0)
            {
                rec.Message = "No seats available.";
                return rec;
            }

            // 1. Single passenger recommendation
            if (passengerCount == 1)
            {
                // Look for window seats first (Col 1 or Col 4) starting from the front rows
                foreach (var row in layout)
                {
                    var windowSeat = row.FirstOrDefault(s => !s.IsBooked && (s.Col == 1 || s.Col == 4));
                    if (windowSeat != null)
                    {
                        rec.SuggestedSeats.Add(windowSeat.SeatNumber);
                        rec.IsWindow = true;
                        rec.Message = $"Recommended Seat: {windowSeat.SeatNumber} (Window Seat)";
                        return rec;
                    }
                }

                // If no window seats, find the first available seat
                foreach (var row in layout)
                {
                    var anySeat = row.FirstOrDefault(s => !s.IsBooked);
                    if (anySeat != null)
                    {
                        rec.SuggestedSeats.Add(anySeat.SeatNumber);
                        rec.IsWindow = false;
                        rec.Message = $"Recommended Seat: {anySeat.SeatNumber}";
                        return rec;
                    }
                }

                rec.Message = "No seats available.";
                return rec;
            }

            // 2. Group seat suggestion logic (2 or more passengers)
            // Try to find N seats in the same row
            foreach (var row in layout)
            {
                var availableInRow = row.Where(s => !s.IsBooked).ToList();
                if (availableInRow.Count >= passengerCount)
                {
                    // Prioritize actual adjacency inside the row
                    List<SeatInfo> selected = new List<SeatInfo>();

                    if (passengerCount == 2)
                    {
                        // Try 1 and 2
                        var seat1 = availableInRow.FirstOrDefault(s => s.Col == 1);
                        var seat2 = availableInRow.FirstOrDefault(s => s.Col == 2);
                        if (seat1 != null && seat2 != null)
                        {
                            selected.Add(seat1);
                            selected.Add(seat2);
                        }
                        else
                        {
                            // Try 3 and 4
                            var seat3 = availableInRow.FirstOrDefault(s => s.Col == 3);
                            var seat4 = availableInRow.FirstOrDefault(s => s.Col == 4);
                            if (seat3 != null && seat4 != null)
                            {
                                selected.Add(seat3);
                                selected.Add(seat4);
                            }
                            else
                            {
                                // Try 2 and 3 (across aisle)
                                if (seat2 != null && seat3 != null)
                                {
                                    selected.Add(seat2);
                                    selected.Add(seat3);
                                }
                            }
                        }
                    }
                    else if (passengerCount == 3)
                    {
                        // Try 1, 2, 3 or 2, 3, 4
                        var s1 = availableInRow.FirstOrDefault(s => s.Col == 1);
                        var s2 = availableInRow.FirstOrDefault(s => s.Col == 2);
                        var s3 = availableInRow.FirstOrDefault(s => s.Col == 3);
                        var s4 = availableInRow.FirstOrDefault(s => s.Col == 4);

                        if (s1 != null && s2 != null && s3 != null)
                        {
                            selected.Add(s1); selected.Add(s2); selected.Add(s3);
                        }
                        else if (s2 != null && s3 != null && s4 != null)
                        {
                            selected.Add(s2); selected.Add(s3); selected.Add(s4);
                        }
                    }

                    // Fallback if priority checks didn't match but we have enough seats in the row
                    if (selected.Count < passengerCount)
                    {
                        selected = availableInRow.Take(passengerCount).ToList();
                    }

                    rec.SuggestedSeats = selected.Select(s => s.SeatNumber).ToList();
                    rec.Message = $"Recommended Group Seats: {string.Join(", ", rec.SuggestedSeats)}";
                    return rec;
                }
            }

            // If we can't fit the entire group in a single row, find the closest available seats in adjacent rows
            var flatAvailable = layout.SelectMany(r => r).Where(s => !s.IsBooked).ToList();
            if (flatAvailable.Count >= passengerCount)
            {
                // Sort by distance/row order to keep them as close together as possible
                var selected = flatAvailable.Take(passengerCount).ToList();
                rec.SuggestedSeats = selected.Select(s => s.SeatNumber).ToList();
                rec.Message = $"Recommended Group Seats: {string.Join(", ", rec.SuggestedSeats)} (Note: split across rows due to availability)";
                return rec;
            }

            rec.Message = "Not enough adjacent or available seats to accommodate the group.";
            return rec;
        }
    }
}

