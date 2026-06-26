using System.Collections.Generic;
using System.Threading.Tasks;
using Transport_Management_System.Models;
using Route = Transport_Management_System.Models.Route;

namespace Transport_Management_System.Services
{
    public interface ISearchService
    {
        Task<GlobalSearchResultsViewModel> SearchAllAsync(string query);
        Task<List<SearchSuggestionDto>> GetSearchSuggestionsAsync(string query);
    }

    public class GlobalSearchResultsViewModel
    {
        public string Query { get; set; } = string.Empty;
        public List<User> Users { get; set; } = new();
        public List<Driver> Drivers { get; set; } = new();
        public List<Vehicle> Vehicles { get; set; } = new();
        public List<Station> Stations { get; set; } = new();
        public List<Route> Routes { get; set; } = new();
        public List<Trip> Trips { get; set; } = new();
        public List<Booking> Bookings { get; set; } = new();
        public List<MaintenanceRecord> MaintenanceRecords { get; set; } = new();

        public int TotalResults => Users.Count + Drivers.Count + Vehicles.Count + Stations.Count + Routes.Count + Trips.Count + Bookings.Count + MaintenanceRecords.Count;
    }

    public class SearchSuggestionDto
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // e.g. "Route", "Vehicle", "Trip", "Booking", "Driver", "User", "Maintenance", "Station"
        public string Url { get; set; } = string.Empty;
    }
}
