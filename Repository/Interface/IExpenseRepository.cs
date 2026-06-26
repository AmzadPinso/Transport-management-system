using Transport_Management_System.Models;

namespace Transport_Management_System.Repository.Interface
{
    public interface IExpenseRepository : IBaseRepository<Expense>
    {
        Task<(IEnumerable<Expense> Expenses, int TotalCount)> GetPagedAsync(
            string? search,
            int? vehicleId,
            ExpenseCategory? category,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber,
            int pageSize);

        Task<decimal> GetTotalExpensesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalExpensesByCategoryAsync(ExpenseCategory category, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Expense>> GetByVehicleIdAsync(int vehicleId);
        Task<Dictionary<ExpenseCategory, decimal>> GetExpenseSummaryByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, decimal>> GetMonthlyExpensesSummaryAsync(int monthsToLookBack = 6);
    }
}
