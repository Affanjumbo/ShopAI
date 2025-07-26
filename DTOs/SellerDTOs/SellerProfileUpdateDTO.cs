// SellerProfileUpdateDTO.cs
namespace ShopAI.DTOs.SellerDTOs
{
    public class SellerProfileUpdateDTO
    {
        public int SellerId { get; set; }
        public string? PhoneNumber { get; set; }          // From sellerPhone input
        public string? BusinessAddress { get; set; }      // From sellerAddress textarea
        public string? ShopName { get; set; }            // From shopName input
        public string? AccountHolder { get; set; }        // From accountHolder input
        public string? AccountNumber { get; set; }        // From accountNumber input
        public string? BankName { get; set; }            // From bankName input
        public string? SwiftCode { get; set; }           // From ifscCode input (mapped to SwiftCode)
    }
}