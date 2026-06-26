using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Repository.Application
{
    public class ExpenseRepository : BaseRepository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<Expense?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(e => e.Vehicle)
                .Include(e => e.CreatedByUser)
                .FirstOrDefaultAsync(e => e.ExpenseId == id);
        }

        public async Task<(IEnumerable<Expense> Expenses, int TotalCount)> GetPagedAsync(
            string? search,
            int? vehicleId,
            ExpenseCategory? category,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber,
            int pageSize)
        {
            var query = _dbSet
                .Include(e => e.Vehicle)
                .Include(e => e.CreatedByUser)
                .AsQueryable();

            if (vehicleId.HasValue)
            {
                query = query.Where(e => e.VehicleId == vehicleId.Value);
            }

            if (category.HasValue)
            {
                query = query.Where(e => e.ExpenseCategory == category.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate <= endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    (e.Vehicle != null && (e.Vehicle.VehicleName.Contains(search) || e.Vehicle.VehicleNumber.Contains(search))) ||
                    (e.Description != null && e.Description.Contains(search))
                );
            }

            var totalCount = await query.CountAsync();
            var expenses = await query
                .OrderByDescending(e => e.ExpenseDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (expenses, totalCount);
        }

        public async Task<decimal> GetTotalExpensesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate <= endDate.Value);
            }

            return await query.SumAsync(e => e.Amount);
        }

        public async Task<decimal> GetTotalExpensesByCategoryAsync(ExpenseCategory category, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(e => e.ExpenseCategory == category);

            if (startDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate <= endDate.Value);
            }

            return await query.SumAsync(e => e.Amount);
        }

        public async Task<IEnumerable<Expense>> GetByVehicleIdAsync(int vehicleId)
        {
            return await _dbSet
                .Include(e => e.Vehicle)
                .Where(e => e.VehicleId == vehicleId)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        public async Task<Dictionary<ExpenseCategory, decimal>> GetExpenseSummaryByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate <= endDate.Value);
            }

            var groups = await query
                .GroupBy(e => e.ExpenseCategory)
                .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
                .ToListAsync();

            return groups.ToDictionary(g => g.Category, g => g.Total);
        }

        public async Task<Dictionary<string, decimal>> GetMonthlyExpensesSummaryAsync(int monthsToLookBack = 6)
        {
            var today = DateTime.Today;
            var startDate = new DateTime(today.Year, today.Month, 1).AddMonths(-monthsToLookBack + 1);

            var rawData = await _dbSet
                .Where(e => e.ExpenseDate >= startDate)
                .ToListAsync(); // Pull to memory to evaluate dates locally with string formatting

            var dict = new Dictionary<string, decimal>();

            for (int i = 0; i < monthsToLookBack; i++)
            {
                var monthDate = startDate.AddMonths(i);
                var key = monthDate.ToString("yyyy-MM");
                dict[key] = rawData
                    .Where(e => e.ExpenseDate.Year == monthDate.Year && e.ExpenseDate.Month == monthDate.Month)
                    .Sum(e => e.Amount);
            }

            return dict;
        }
    }
}
