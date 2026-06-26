using System.ComponentModel.DataAnnotations;

namespace Transport_Management_System.Models
{
    public enum IssuePriority
    {
        [Display(Name = "Low")]
        Low,

        [Display(Name = "Medium")]
        Medium,

        [Display(Name = "High")]
        High,

        [Display(Name = "Critical")]
        Critical
    }
}
