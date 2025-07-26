namespace ShopAI.DTOs.CustomerDTOs
{
    // DTO for returning customer details.
    public class CustomerResponseDTO
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required DateTime DateOfBirth { get; set; }
    }
}