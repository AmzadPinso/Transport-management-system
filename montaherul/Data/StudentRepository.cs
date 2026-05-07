//using System.Data;
//using Microsoft.Data.SqlClient;
//using montaherul.Models;

//namespace montaherul.Data
//{
//    public class StudentRepository
//    {
//        private readonly string _connectionString;

//        public StudentRepository(IConfiguration configuration)
//        {
//            _connectionString = configuration.GetConnectionString("DefaultConnection");
//        }

//        public async Task<(List<StudentDto>, int)> GetStudents(string? search, int pageNumber, int pageSize)
//        {
//            var list = new List<StudentDto>();
//            int total = 0;

//            using (SqlConnection con = new SqlConnection(_connectionString))
//            using (SqlCommand cmd = new SqlCommand("GetStudentsPagedWithSearch", con))
//            {
//                cmd.CommandType = CommandType.StoredProcedure;

//                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
//                cmd.Parameters.AddWithValue("@PageSize", pageSize);
//                cmd.Parameters.AddWithValue("@Search", (object?)search ?? DBNull.Value);

//                await con.OpenAsync();

//                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
//                {
//                    while (await reader.ReadAsync())
//                    {
//                        list.Add(new StudentDto
//                        {
//                            Id = Convert.ToInt32(reader["Id"]),
//                            Name = reader["Name"]?.ToString(),
//                            Age = Convert.ToInt32(reader["Age"]),
//                            Email = reader["Email"]?.ToString(),
//                            Address = reader["Address"]?.ToString(),
//                            CourseId = Convert.ToInt32(reader["CourseId"]),
//                            CourseName = reader["CourseName"]?.ToString()
//                        });
//                    }

//                    if (await reader.NextResultAsync())
//                    {
//                        if (await reader.ReadAsync())
//                        {
//                            total = Convert.ToInt32(reader["TotalRecords"]);
//                        }
//                    }
//                }
//            }

//            return (list, total);
//        }
//    }

//}