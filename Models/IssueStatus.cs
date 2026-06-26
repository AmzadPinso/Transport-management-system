using System.ComponentModel.DataAnnotations;

namespace Transport_Management_System.Models
{
    public enum IssueStatus
    {
        [Display(Name = "Open")]
        Open,

        [Display(Name = "Under Review")]
        UnderReview,

        [Display(Name = "In Progress")]
        InProgress,

        [Display(Name = "Resolved")]
        Resolved,

        [Display(Name = "Closed")]
        Closed
    }
}
