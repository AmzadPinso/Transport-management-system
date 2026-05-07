using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace montaherul.Models
{
    public class CourseModel
    {
        [Key]
        public int Id { get; set; }

       // [Required, MaxLength(100)]
        public string? CourseName { get; set; }

        // Foreign Key
      //  [Display(Name = "Teacher")]
        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public TeacherModel? Teacher { get; set; }
    }
    public class CourseVM
    {
        
        public int Id { get; set; }

        // [Required, MaxLength(100)]
        public string? CourseName { get; set; }

        // Foreign Key
        //  [Display(Name = "Teacher")]
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public long RowNum { get; set; }
        public int TOTALCOUNT { get; set; }
       
    }
}