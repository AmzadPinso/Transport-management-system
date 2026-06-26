using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport_Management_System.Models
{
    public class Expense
    {
        [Key]
        public int ExpenseId { get; set; }

        [Display(Name = "Vehicle")]
        public int? VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        [Required(ErrorMessage = "Expense category is required")]
        [Display(Name = "Expense Category")]
        public ExpenseCategory ExpenseCategory { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount (BDT)")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Expense date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Expense Date")]
        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Recorded By")]
        public int? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
