using System.ComponentModel.DataAnnotations;

public class SellerAddressUpdateDTO
{
    [Required]
    public int Id { get; set; }

    [Required]
    public int SellerId { get; set; }

    [Required]
    [StringLength(100)]
    public string AddressType { get; set; }

    [Required]
    [StringLength(200)]
    public string Street { get; set; }

    [Required(ErrorMessage = "City must contain only letters and spaces.")]
    public string City { get; set; }

    [Required(ErrorMessage = "State must contain only letters and spaces.")]
    public string State { get; set; }

    [Required(ErrorMessage = "Postal code must be 5 to 10 digits.")]
    public string PostalCode { get; set; }

    [Required(ErrorMessage = "Country must contain only letters and spaces.")]
    public string Country { get; set; }

    public bool IsDefault { get; set; }
}
