using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Transport_Management_System.Models;

namespace Transport_Management_System.Models.ViewModels
{
    public class ExpenseFormViewModel
    {
        public int ExpenseId { get; set; }

        [Display(Name = "Vehicle (optional)")]
        public int? VehicleId { get; set; }

        [Required(ErrorMessage = "Expense category is required")]
        [Display(Name = "Expense Category")]
        public ExpenseCategory ExpenseCategory { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount (BDT)")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Expense date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Expense Date")]
        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Display(Name = "Description / Notes")]
        public string? Description { get; set; }

        // For dropdowns
        public IEnumerable<SelectListItem> VehicleList { get; set; } = [];
    }
}
