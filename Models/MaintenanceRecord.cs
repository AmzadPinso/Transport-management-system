using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport_Management_System.Models
{
    public class MaintenanceRecord
    {
        [Key]
        public int MaintenanceRecordId { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        [Required(ErrorMessage = "Maintenance type is required")]
        [Display(Name = "Maintenance Type")]
        public MaintenanceType MaintenanceType { get; set; }

        [Required(ErrorMessage = "Service date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Service Date")]
        public DateTime ServiceDate { get; set; }

        [Required(ErrorMessage = "Next service date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Next Service Date")]
        public DateTime NextServiceDate { get; set; }

        [StringLength(200)]
        [Display(Name = "Service Provider")]
        public string? ServiceProvider { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Cost must be a positive value")]
        [Display(Name = "Maintenance Cost (BDT)")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; } = 0;

        [StringLength(1000)]
        [Display(Name = "Service Notes")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Status")]
        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Scheduled;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
