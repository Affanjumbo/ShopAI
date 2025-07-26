using System.ComponentModel.DataAnnotations;

namespace ShopAI.DTOs.BankDTOs
{
    public class BankDetailsCreateDTO
    {
        
        public int ShopId { get; set; }

        public string AccountHolder { get; set; }

       
        public string BankName { get; set; }

        public string IBAN { get; set; }

        public string? SwiftCode { get; set; }

        public string StatementUrl { get; set; }

        public bool IsPrimary { get; set; } = false;
    }
}
