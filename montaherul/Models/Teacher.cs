using System.ComponentModel.DataAnnotations;

namespace montaherul.Models
{
    public class TeacherModel
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string? Name { get; set; }

        [Required]
        public string? Subject { get; set; }

        public int Age { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public string? Address { get; set; }


        public string? ProfileImage { get; set; }


    }
}