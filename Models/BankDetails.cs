using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopAI.Models
{
    public class BankDetails
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ShopId { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[\p{L}\s.'-]{2,100}$", ErrorMessage = "Account holder name contains invalid characters")]
        public string AccountHolder { get; set; }

        [Required]
        [StringLength(50)]
        public string BankName { get; set; }

        [Required]
        [StringLength(34)]
        [RegularExpression(@"^[A-Z]{2}\d{2}[A-Z0-9]{1,30}$", ErrorMessage = "Invalid IBAN format")]
        public string IBAN { get; set; }

        [StringLength(11)]
        public string? SwiftCode { get; set; }

        public string StatementUrl { get; set; }

        public bool IsPrimary { get; set; } = false;

        public DateTime AddedOn { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("ShopId")]
        public ShopDetails ShopDetails { get; set; }
    }
}
