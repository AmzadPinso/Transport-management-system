using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Repository.Application
{
    public class UserRepo : BaseRepository<User>, IUserRepo
    {
        public UserRepo(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);
        }

        public override async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<(IEnumerable<User>, int)> GetUsersPagedAsync(
            string? search,
            int? roleId,
            int pageNumber,
            int pageSize,
            string sortColumn,
            string sortDirection)
        {
            var searchParam = new SqlParameter("@Search", search ?? (object)DBNull.Value);
            var roleParam = new SqlParameter("@RoleId", roleId ?? (object)DBNull.Value);
            var pageParam = new SqlParameter("@PageNumber", pageNumber);
            var sizeParam = new SqlParameter("@PageSize", pageSize);
            var sortColumnParam = new SqlParameter("@SortColumn", sortColumn ?? "UserId");
            var sortDirectionParam = new SqlParameter("@SortDirection", sortDirection ?? "ASC");

            var totalParam = new SqlParameter
            {
                ParameterName = "@TotalRecords",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            // Using FromSqlRaw to execute the stored procedure
            var data = await _context.Users
                .FromSqlRaw("EXEC dbo.GetUsersPagedWithSearch @PageNumber, @PageSize, @Search, @RoleId, @SortColumn, @SortDirection, @TotalRecords OUTPUT",
                    pageParam, sizeParam, searchParam, roleParam, sortColumnParam, sortDirectionParam, totalParam)
                .AsNoTracking()
                .ToListAsync();

            // Manually load Roles for the fetched users to avoid the Include error with stored procedures
            var roleIds = data.Select(u => u.RoleId).Distinct().ToList();
            var roles = await _context.Roles.Where(r => roleIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id);
            
            foreach (var user in data)
            {
                if (roles.ContainsKey(user.RoleId))
                {
                    user.Role = roles[user.RoleId];
                }
            }

            int total = (int)(totalParam.Value ?? 0);

            return (data, total);
        }
    }
}
