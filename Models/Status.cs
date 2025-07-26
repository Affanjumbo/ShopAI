using System.ComponentModel.DataAnnotations;
namespace ShopAI.Models
{
    // Represents the status master
    public class Status
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Name { get; set; }
    }
}