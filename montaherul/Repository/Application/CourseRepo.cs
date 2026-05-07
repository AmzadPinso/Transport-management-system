using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using montaherul.Helper;
using montaherul.Models;
using montaherul.Repository.Interface;

namespace montaherul.Repository.Application
{
    public class CourseRepo : BaseRepository<CourseModel>, ICourseRepo
    {
        public CourseRepo(MyDBContext context) : base(context)
        {
        }

        public async Task<List<CourseVM>> GetAllCourseAsync(string searchTerm, int page = 1, int size = 5)
        {
            try
            {
                var dbHelper = new DbHelper(_context);

                var param2 = new SqlParameter("@Search", searchTerm);
                var param3 = new SqlParameter("@DisplayLength", size);
                var param4 = new SqlParameter("@DisplayStart", page);

                var Courses = await dbHelper.ExecuteSPAsync<CourseVM>(
                    "Get_All_Course",
                    param2, param3, param4
                );

                return Courses;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while retrieving courses: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving courses: {ex.Message}", ex);
            }
        }
    }
}