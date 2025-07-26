namespace ShopAI.DTOs.SellerDTOs
{
    public class SellerResponseDTO
    {
        public int Id { get; set; }
        public int SellerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string City { get; set; }
        public string DateOfBirth { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime RegisteredAt { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string Token { get; set; }
    }
}
