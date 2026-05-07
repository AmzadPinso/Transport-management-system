using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace montaherul.Models
{
    public class StudentModel
    {
        [Key]
        public int Id { get; set; }

        // [Required, MaxLength(100)]
        public string? Name { get; set; }

        public int Age { get; set; }

       // [EmailAddress]
        public string? Email { get; set; }


        public string? Address { get; set; }



        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public CourseModel? Course { get; set; }

    }
}