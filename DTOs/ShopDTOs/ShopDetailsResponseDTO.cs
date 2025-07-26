namespace ShopAI.DTOs.ShopDTOs
{
    public class ShopDetailsResponseDTO
    {
        public int Id { get; set; }

        public int SellerId { get; set; }

        public string ShopName { get; set; }

        public string? WebsiteUrl { get; set; }

        public bool IsPrimary { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
