using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopAI.Models
{
    public class ShopDetails
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SellerId { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[\w\s&'-]{2,100}$", ErrorMessage = "Shop name contains invalid characters")]
        public string ShopName { get; set; }

        [Url]
        [StringLength(255)]
        public string? WebsiteUrl { get; set; }

        public bool IsPrimary { get; set; } = false;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("SellerId")]
        public Seller Seller { get; set; }

        public ICollection<BankDetails> BankDetails { get; set; }
    }
}
