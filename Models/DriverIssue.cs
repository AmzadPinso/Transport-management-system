using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport_Management_System.Models
{
    public class DriverIssue
    {
        [Key]
        public int DriverIssueId { get; set; }

        [Required(ErrorMessage = "Driver is required")]
        [Display(Name = "Driver")]
        public int DriverId { get; set; }

        [ForeignKey("DriverId")]
        public Driver? Driver { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

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
        [Display(Name = "Priority")]
        public IssuePriority Priority { get; set; } = IssuePriority.Medium;

        [Required]
        [Display(Name = "Status")]
        public IssueStatus Status { get; set; } = IssueStatus.Open;

        [StringLength(2000)]
        [Display(Name = "Resolution Notes")]
        public string? ResolutionNotes { get; set; }

        [Display(Name = "Resolved At")]
        public DateTime? ResolvedAt { get; set; }

        [Display(Name = "Reported By User")]
        public int? ReportedByUserId { get; set; }

        [ForeignKey("ReportedByUserId")]
        public User? ReportedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
