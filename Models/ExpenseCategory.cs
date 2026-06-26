using System.ComponentModel.DataAnnotations;

namespace Transport_Management_System.Models
{
    public enum ExpenseCategory
    {
        [Display(Name = "Fuel")]
        Fuel,

        [Display(Name = "Maintenance")]
        Maintenance,

        [Display(Name = "Repair")]
        Repair,

        [Display(Name = "Toll Charges")]
        TollCharges,

        [Display(Name = "Insurance")]
        Insurance,

        [Display(Name = "Driver Allowance")]
        DriverAllowance,

        [Display(Name = "Parking")]
        Parking,

        [Display(Name = "Miscellaneous")]
        Miscellaneous
    }
}
