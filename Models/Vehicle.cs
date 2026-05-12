using System.ComponentModel.DataAnnotations;

namespace Transport_Management_System.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Vehicle Name is required")]
        [Display(Name = "Vehicle Name")]
        [StringLength(100)]
        public string VehicleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle Number is required")]
        [Display(Name = "Vehicle Number")]
        [StringLength(50)]
        public string VehicleNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle Type is required")]
        [Display(Name = "Vehicle Type")]
        public VehicleType VehicleType { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int Capacity { get; set; }

        [Required]
        public VehicleStatus Status { get; set; } = VehicleStatus.Active;

        [Required(ErrorMessage = "Last Service Date is required")]
        [Display(Name = "Last Service Date")]
        [DataType(DataType.Date)]
        public DateTime LastServiceDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
