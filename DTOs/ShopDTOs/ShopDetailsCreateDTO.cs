namespace ShopAI.DTOs.ShopDTOs
{
    public class ShopDetailsCreateDTO
    {
        public int SellerId { get; set; }

        public string ShopName { get; set; }

        public string? WebsiteUrl { get; set; }

        public bool IsPrimary { get; set; }
    }
}
