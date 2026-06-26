using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Transport_Management_System.Models;
using Transport_Management_System.Models.ViewModels;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ExpensesController : Controller
    {
        private readonly IExpenseRepository _expenseRepo;
        private readonly IVehicleRepository _vehicleRepo;

        public ExpensesController(IExpenseRepository expenseRepo, IVehicleRepository vehicleRepo)
        {
            _expenseRepo = expenseRepo;
            _vehicleRepo = vehicleRepo;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId");
            return int.TryParse(claim, out var id) ? id : 0;
        }

        // GET: /Expenses
        public async Task<IActionResult> Index(
            string? search,
            int? vehicleId,
            ExpenseCategory? category,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var (expenses, totalCount) = await _expenseRepo.GetPagedAsync(
                search, vehicleId, category, startDate, endDate, pageNumber, pageSize);

            var vehicles = await _vehicleRepo.GetAllAsync();
            var totalExpenses = await _expenseRepo.GetTotalExpensesAsync(startDate, endDate);

            ViewBag.Search = search;
            ViewBag.SelectedVehicleId = vehicleId;
            ViewBag.SelectedCategory = category;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.TotalExpensesAmount = totalExpenses;

            ViewBag.VehicleList = new SelectList(vehicles, "VehicleId", "VehicleName", vehicleId);

            ViewData["Title"] = "Expense Tracking";
            return View(expenses);
        }

        // GET: /Expenses/Create
        public async Task<IActionResult> Create()
        {
            var vehicles = await _vehicleRepo.GetAllAsync();
            var vm = new ExpenseFormViewModel
            {
                VehicleList = vehicles.Select(v => new SelectListItem
                {
                    Value = v.VehicleId.ToString(),
                    Text = $"{v.VehicleName} ({v.VehicleNumber})"
                })
            };

            ViewData["Title"] = "Record Expense";
            return View(vm);
        }

        // POST: /Expenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var expense = new Expense
                {
                    VehicleId = model.VehicleId,
                    ExpenseCategory = model.ExpenseCategory,
                    Amount = model.Amount,
                    ExpenseDate = model.ExpenseDate,
                    Description = model.Description,
                    CreatedByUserId = GetCurrentUserId(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _expenseRepo.AddAsync(expense);
                await _expenseRepo.SaveAsync();

                TempData["SuccessMessage"] = "Expense recorded successfully.";
                return RedirectToAction(nameof(Index));
            }

            var vehicles = await _vehicleRepo.GetAllAsync();
            model.VehicleList = vehicles.Select(v => new SelectListItem
            {
                Value = v.VehicleId.ToString(),
                Text = $"{v.VehicleName} ({v.VehicleNumber})"
            });

            ViewData["Title"] = "Record Expense";
            return View(model);
        }

        // GET: /Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _expenseRepo.GetByIdAsync(id.Value);
            if (expense == null) return NotFound();

            var vehicles = await _vehicleRepo.GetAllAsync();
            var vm = new ExpenseFormViewModel
            {
                ExpenseId = expense.ExpenseId,
                VehicleId = expense.VehicleId,
                ExpenseCategory = expense.ExpenseCategory,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Description = expense.Description,
                VehicleList = vehicles.Select(v => new SelectListItem
                {
                    Value = v.VehicleId.ToString(),
                    Text = $"{v.VehicleName} ({v.VehicleNumber})"
                })
            };

            ViewData["Title"] = "Edit Expense";
            return View(vm);
        }

        // POST: /Expenses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseFormViewModel model)
        {
            if (id != model.ExpenseId) return NotFound();

            if (ModelState.IsValid)
            {
                var expense = await _expenseRepo.GetByIdAsync(id);
                if (expense == null) return NotFound();

                expense.VehicleId = model.VehicleId;
                expense.ExpenseCategory = model.ExpenseCategory;
                expense.Amount = model.Amount;
                expense.ExpenseDate = model.ExpenseDate;
                expense.Description = model.Description;
                expense.UpdatedAt = DateTime.Now;

                _expenseRepo.Update(expense);
                await _expenseRepo.SaveAsync();

                TempData["SuccessMessage"] = "Expense updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            var vehicles = await _vehicleRepo.GetAllAsync();
            model.VehicleList = vehicles.Select(v => new SelectListItem
            {
                Value = v.VehicleId.ToString(),
                Text = $"{v.VehicleName} ({v.VehicleNumber})"
            });

            ViewData["Title"] = "Edit Expense";
            return View(model);
        }

        // GET: /Expenses/Summary
        public async Task<IActionResult> Summary(int months = 6)
        {
            var categorySummary = await _expenseRepo.GetExpenseSummaryByCategoryAsync();
            var monthlySummary = await _expenseRepo.GetMonthlyExpensesSummaryAsync(months);
            
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var thisMonthTotal = await _expenseRepo.GetTotalExpensesAsync(startOfMonth, today);
            var overallTotal = await _expenseRepo.GetTotalExpensesAsync();

            ViewBag.CategorySummary = categorySummary;
            ViewBag.MonthlySummary = monthlySummary;
            ViewBag.ThisMonthTotal = thisMonthTotal;
            ViewBag.OverallTotal = overallTotal;
            ViewBag.Months = months;

            ViewData["Title"] = "Expense Summary Report";
            return View();
        }

        // POST: /Expenses/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _expenseRepo.GetByIdAsync(id);
            if (expense == null) return NotFound();

            _expenseRepo.Delete(expense);
            await _expenseRepo.SaveAsync();

            TempData["SuccessMessage"] = "Expense deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
