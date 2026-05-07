using System.ComponentModel.DataAnnotations;

namespace Transport_Management_System.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Role Description")]
        public string? RoleDescription { get; set; }

        // Navigation property for One-to-Many relationship
        public ICollection<User>? Users { get; set; }
    }
}
