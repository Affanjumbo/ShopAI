namespace ShopAI.DTOs.ProductDTOs
{
    // DTO for returning product details.
    public class ProductResponseDTO
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ProductImage { get; set; }
        public int DiscountPercentage { get; set; }
        public int CategoryId { get; set; }

        public int? SubCategoryId { get; set; } // nullable, only required for category 16

        public bool IsAvailable { get; set; }

        public int SellerId { get; set; }
    }

}