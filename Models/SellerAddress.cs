using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopAI.Models
{
    public class SellerAddress
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Seller")]
        public int SellerId { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Address type must contain only letters and spaces.")]
        public string AddressType { get; set; } // e.g. "Warehouse", "Business"

        [Required]
        [StringLength(200)]
        public string Street { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "City must contain only letters and spaces.")]
        public string City { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "State must contain only letters and spaces.")]
        public string State { get; set; }

        [Required]
        [StringLength(10)]
        [RegularExpression(@"^\d{4,10}$", ErrorMessage = "Postal code must be between 4 to 10 digits.")]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Country must contain only letters and spaces.")]
        public string Country { get; set; }

        public bool IsDefault { get; set; } = false;

        // Navigation property
        public Seller Seller { get; set; }
    }
}
