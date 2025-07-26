using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopAI.Models
{
    // Represents a payment transaction
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Required]
        [StringLength(50)]
        public required string PaymentMethod { get; set; } 

        [StringLength(50)]
        public string? TransactionId { get; set; } // From payment gateway

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [StringLength(20)]
        public PaymentStatus Status { get; set; } // "Completed", "Pending", "Failed", "Refunded"

        public Refund? Refund { get; set; } // Navigational property to Refund
    }
}