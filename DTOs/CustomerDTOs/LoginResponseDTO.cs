namespace ShopAI.DTOs.CustomerDTOs
{
    public class LoginResponseDTO
    {
        public int CustomerId { get; set; }
        public required string CustomerName { get; set; }
        public required string Message { get; set; }

        public string Token { get; set; }
    }
}