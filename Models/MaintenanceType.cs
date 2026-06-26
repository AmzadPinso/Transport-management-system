using System.ComponentModel.DataAnnotations;

namespace Transport_Management_System.Models
{
    public enum MaintenanceType
    {
        [Display(Name = "Oil Change")]
        OilChange,

        [Display(Name = "Tire Rotation")]
        TireRotation,

        [Display(Name = "Brake Service")]
        BrakeService,

        [Display(Name = "Engine Overhaul")]
        EngineOverhaul,

        [Display(Name = "Electrical Repair")]
        Electrical,

        [Display(Name = "General Inspection")]
        Inspection,

        [Display(Name = "Air Filter Replacement")]
        AirFilter,

        [Display(Name = "Battery Replacement")]
        Battery,

        [Display(Name = "Cooling System")]
        CoolingSystem,

        [Display(Name = "Other")]
        Other
    }
}
