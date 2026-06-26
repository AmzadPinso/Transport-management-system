using System.ComponentModel.DataAnnotations;

namespace Transport_Management_System.Models
{
    public enum IssueCategory
    {
        [Display(Name = "Engine Problem")]
        Engine,

        [Display(Name = "Brake Issue")]
        Brakes,

        [Display(Name = "Tire Problem")]
        Tires,

        [Display(Name = "Electrical Fault")]
        Electrical,

        [Display(Name = "Fuel System")]
        Fuel,

        [Display(Name = "Accident / Damage")]
        Accident,

        [Display(Name = "Air Conditioning")]
        AirConditioning,

        [Display(Name = "Transmission")]
        Transmission,

        [Display(Name = "Other")]
        Other
    }
}
