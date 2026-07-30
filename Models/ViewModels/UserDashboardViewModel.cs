using System.Collections.Generic;
using Transport_Management_System.Models;

namespace Transport_Management_System.Models.ViewModels
{
    public class UserDashboardViewModel
    {
        public User? UserProfile { get; set; }
        public List<Booking> RecentBookings { get; set; } = new();
        public List<Booking> UpcomingBookings { get; set; } = new();
        public List<Trip> AvailableTrips { get; set; } = new();
        public int TotalBookingsCount { get; set; }
        public int ConfirmedBookingsCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
