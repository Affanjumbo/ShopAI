using System.ComponentModel.DataAnnotations;

namespace ShopAI.DTOs.CustomerDTOs
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "CustomerId is required.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Current Password is required.")]
        public required string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "New Password must be at least 8 characters and include uppercase, lowercase, number, and special character.")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm New Password is required.")]
        [Compare("NewPassword", ErrorMessage = "New Password and Confirm New Password do not match.")]
        public required string ConfirmNewPassword { get; set; }
    }
}
