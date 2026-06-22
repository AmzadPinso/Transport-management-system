using System.ComponentModel.DataAnnotations;

namespace Transport_Management_System.Models.ViewModels
{
    public class StationFormViewModel
    {
        public int StationId { get; set; }

        [Required(ErrorMessage = "Station Name is required")]
        [StringLength(150)]
        [Display(Name = "Station Name")]
        public string StationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string? District { get; set; }

        [Required(ErrorMessage = "Full Address is required")]
        [StringLength(300)]
        [Display(Name = "Full Address")]
        public string Address { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;
    }
}
