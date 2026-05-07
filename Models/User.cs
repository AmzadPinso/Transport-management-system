using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport_Management_System.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Address { get; set; }

        // Foreign Key
        [Required]
        [Display(Name = "Role")]
        public int RoleId { get; set; }

        // Navigation property
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("RoleId")]
        public Role? Role { get; set; }

        [Display(Name = "Last LogIN")]
        public DateTime? LastLogIN { get; set; }
    }
}
