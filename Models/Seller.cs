using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopAI.Models
{
    public class Seller
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "First name must contain only letters.")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "Last name must contain only letters.")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email format is invalid.")]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; } 

        [StringLength(15)]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Phone number must be between 10 and 15 digits.")]
        public string? PhoneNumber { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "City must contain only letters and spaces.")]
        public string City { get; set; }

        [Required]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date of Birth must be in the format yyyy-MM-dd.")]
        public string DateOfBirth { get; set; }

        public bool IsVerified { get; set; } = false;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Optional profile picture URL
        [Url]
        public string? ProfileImageUrl { get; set; }

        // Relationship: Seller can have many products
        public ICollection<Product> Products { get; set; }
        public ICollection<SellerAddress> SellerAddresses { get; set; }

        public ICollection<ShopDetails> ShopDetails { get; set; }

        public ICollection<BankDetails> BankDetails { get; set; }

    }
}
