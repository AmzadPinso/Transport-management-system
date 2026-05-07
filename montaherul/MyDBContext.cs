using Microsoft.EntityFrameworkCore;
using montaherul.Models;

namespace montaherul
{
    public class MyDBContext:DbContext
    {
        public MyDBContext(DbContextOptions<MyDBContext> options)
    : base(options)
        {
        }

        public DbSet<StudentModel> Students { get; set; }
         public DbSet<TeacherModel> TeacherModel { get; set; } 
        public DbSet<CourseModel> Courses { get; set; }
    }
}
