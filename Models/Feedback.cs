using System.ComponentModel.DataAnnotations;

namespace ShopAI.Models
{
    // Represents customer feedback for a product
    public class Feedback
    {
        public int Id { get; set; }

        // Foreign key to Customer
        [Required]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // Foreign key to Product
        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        // Rating between 1 and 5
        [Required]

        [Range(1.0, 5.0, ErrorMessage = "Rating must be between 1.0 and 5.0")]
        public decimal Rating { get; set; }

        // Optional comment with maximum length
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string? Comment { get; set; }

        // Timestamp of feedback submission
        public DateTime CreatedAt { get; set; }

        // Timestamp of feedback updation
        public DateTime UpdatedAt { get; set; }
    }
}