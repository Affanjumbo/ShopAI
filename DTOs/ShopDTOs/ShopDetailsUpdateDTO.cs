namespace ShopAI.DTOs.ShopDTOs
{
    public class ShopDetailsUpdateDTO
    {
        public int Id { get; set; }

        public string ShopName { get; set; }

        public string? WebsiteUrl { get; set; }

        public bool IsPrimary { get; set; }
    }
}
