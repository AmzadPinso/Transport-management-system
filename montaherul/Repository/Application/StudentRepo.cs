using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using montaherul.Models;
using montaherul.Repository.Interface;

namespace montaherul.Repository.Application
{
    public class StudentRepo : BaseRepository<StudentModel>, IStudentRepo
    {
        private readonly MyDBContext _context;

        public StudentRepo(MyDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<StudentModel>, int)> GetStudents(
            string? search,
            int pageNumber,
            int pageSize,
            string sortColumn,
            string sortDirection)
        {
            var searchParam = new SqlParameter("@Search", search ?? "");
            var pageParam = new SqlParameter("@PageNumber", pageNumber);
            var sizeParam = new SqlParameter("@PageSize", pageSize);

            var sortColumnParam = new SqlParameter("@SortColumn", sortColumn ?? "Id");
            var sortDirectionParam = new SqlParameter("@SortDirection", sortDirection ?? "ASC");

            var totalParam = new SqlParameter
            {
                ParameterName = "@TotalRecords",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output
            };


            var data = await _context.Students
    .FromSqlRaw(
        "EXEC dbo.GetStudentsPagedWithSearch @Search, @PageNumber, @PageSize, @SortColumn, @SortDirection, @TotalRecords OUTPUT",
        searchParam,
        pageParam,
        sizeParam,
        sortColumnParam,
        sortDirectionParam,
        totalParam
    )
    .AsNoTracking()
    .ToListAsync();

            int total = (int)(totalParam.Value ?? 0);

            return (data, total);
        }
    }
}