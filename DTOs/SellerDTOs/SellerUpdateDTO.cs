namespace ShopAI.DTOs.SellerDTOs
{
    public class SellerUpdateDTO
    {
        public int SellerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string City { get; set; }
        public string DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
