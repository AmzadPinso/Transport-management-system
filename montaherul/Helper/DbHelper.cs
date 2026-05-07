using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace montaherul.Helper
{
    public class DbHelper
    {
        private readonly MyDBContext _context;

        public DbHelper(MyDBContext context)
        {
            _context = context;
        }

        public async Task<List<T>> ExecuteSPAsync<T>(string procedureName, params SqlParameter[] parameters) where T : class
        {
            try
            {
                // Build parameter placeholders (@p0, @p1 ...)
                string paramNames = string.Join(", ", parameters.Select(p => p.ParameterName));

                string sql = $"EXEC {procedureName} {paramNames}";

                return await _context.Database.SqlQueryRaw<T>(sql, parameters).ToListAsync();
            }
            catch (Exception ex)
            {
                // Log error here if you have logger
                throw new Exception($"Error executing stored procedure '{procedureName}': {ex.Message}", ex);
            }
        }

        // Execute Raw SQL Query (Returns Scalar - single value like string, int, etc.)
        public async Task<T> ExecuteScalarAsync<T>(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    await _context.Database.OpenConnectionAsync();
                    var result = await command.ExecuteScalarAsync();
                    await _context.Database.CloseConnectionAsync();

                    if (result == null || result == DBNull.Value)
                        return default(T);

                    return (T)Convert.ChangeType(result, typeof(T));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing scalar query: {ex.Message}", ex);
            }
        }

        public async Task<List<T>> ExecuteRawQueryAsync<T>(string sql, params SqlParameter[] parameters) where T : class
        {
            try
            {
                return await _context.Set<T>()
                                     .FromSqlRaw(sql, parameters)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing SQL query: {ex.Message}", ex);
            }
        }

    }
}
