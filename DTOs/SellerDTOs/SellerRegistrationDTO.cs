namespace ShopAI.DTOs.SellerDTOs
{
    public class SellerRegistrationDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // Plain-text, will be hashed
        public string? PhoneNumber { get; set; }
        public string City { get; set; }
        public string DateOfBirth { get; set; } // Format: yyyy-MM-dd
    }
}
