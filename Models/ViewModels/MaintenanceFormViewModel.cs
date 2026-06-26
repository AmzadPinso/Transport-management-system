using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Transport_Management_System.Models;

namespace Transport_Management_System.Models.ViewModels
{
    public class MaintenanceFormViewModel
    {
        public int MaintenanceRecordId { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Maintenance type is required")]
        [Display(Name = "Maintenance Type")]
        public MaintenanceType MaintenanceType { get; set; }

        [Required(ErrorMessage = "Service date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Service Date")]
        public DateTime ServiceDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Next service date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Next Service Date")]
        public DateTime NextServiceDate { get; set; } = DateTime.Now.AddMonths(3);

        [StringLength(200)]
        [Display(Name = "Service Provider")]
        public string? ServiceProvider { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Cost must be a positive value")]
        [Display(Name = "Maintenance Cost (BDT)")]
        [DataType(DataType.Currency)]
        public decimal Cost { get; set; } = 0;

        [StringLength(1000)]
        [Display(Name = "Service Notes")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Status")]
        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Scheduled;

        // For dropdowns
        public IEnumerable<SelectListItem> VehicleList { get; set; } = [];
    }
}
