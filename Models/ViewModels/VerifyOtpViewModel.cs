using System.ComponentModel.DataAnnotations;

namespace Transport_Management_System.Models.ViewModels
{
    public class VerifyOtpViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "OTP must contain only numbers.")]
        public string Otp { get; set; } = string.Empty;
    }
}
