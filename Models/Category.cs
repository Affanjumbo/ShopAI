using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace ShopAI.Models
{
    [Index(nameof(Name), Name = "IX_Name_Unique", IsUnique = true)]
    // Represents a product category
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Category Name must be between 3 and 100 characters.")]
        public required string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public required string Description { get; set; }

        public bool IsActive { get; set; }

        public ICollection<Product> Products { get; set; }

        public ICollection<SubCategories>? SubCategories { get; set; }
    }
}