﻿public class SellerAddressResponseDTO
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public string AddressType { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public bool IsDefault { get; set; }
}
