namespace montaherul.Models
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
    }
}