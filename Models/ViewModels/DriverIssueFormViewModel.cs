using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Transport_Management_System.Models;

namespace Transport_Management_System.Models.ViewModels
{
    public class DriverIssueFormViewModel
    {
        public int DriverIssueId { get; set; }

        [Required(ErrorMessage = "Driver is required")]
        [Display(Name = "Driver")]
        public int DriverId { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Issue category is required")]
        [Display(Name = "Issue Category")]
        public IssueCategory IssueCategory { get; set; }

        [Required(ErrorMessage = "Issue description is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be at least 10 characters")]
        [Display(Name = "Issue Description")]
        public string IssueDescription { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Report Date")]
        public DateTime ReportDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Priority Level")]
        public IssuePriority Priority { get; set; } = IssuePriority.Medium;

        // For dropdowns
        public IEnumerable<SelectListItem> DriverList { get; set; } = [];
        public IEnumerable<SelectListItem> VehicleList { get; set; } = [];
    }

    public class UpdateIssueStatusViewModel
    {
        public int DriverIssueId { get; set; }

        [Required]
        [Display(Name = "Status")]
        public IssueStatus Status { get; set; }

        [StringLength(2000)]
        [Display(Name = "Resolution Notes")]
        public string? ResolutionNotes { get; set; }
    }
}
