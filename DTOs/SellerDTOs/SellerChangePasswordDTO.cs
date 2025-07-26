namespace ShopAI.DTOs.SellerDTOs
{
    public class SellerChangePasswordDTO
    {
        public int SellerId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
