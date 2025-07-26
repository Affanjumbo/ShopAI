// SellerProfileService.cs
using Microsoft.EntityFrameworkCore;
using ShopAI.Data;
using ShopAI.DTOs.SellerDTOs;
using ShopAI.Models;
using ShopAI.DTOs;

namespace ShopAI.Services
{
    public class SellerProfileService
    {
        private readonly ApplicationDbContext _context;

        public SellerProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<string>> UpdateSellerProfileAsync(SellerProfileUpdateDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Update Seller Phone (if changed)
                if (!string.IsNullOrEmpty(dto.PhoneNumber))
                {
                    var seller = await _context.Sellers.FindAsync(dto.SellerId);
                    if (seller == null)
                        return new ApiResponse<string>(404, "Seller not found.");

                    seller.PhoneNumber = dto.PhoneNumber;
                }

                // 2. Update Seller Address (if changed)
                if (!string.IsNullOrEmpty(dto.BusinessAddress))
                {
                    var address = await _context.SellerAddresses
                        .FirstOrDefaultAsync(a => a.SellerId == dto.SellerId);

                    if (address == null)
                    {
                        // Create new address if none exists
                        address = new SellerAddress
                        {
                            SellerId = dto.SellerId,
                            IsDefault = true
                        };
                        _context.SellerAddresses.Add(address);
                    }

                    // Parse the address text into components
                    var addressParts = dto.BusinessAddress.Split(',');
                    address.Street = addressParts.Length > 0 ? addressParts[0].Trim() : dto.BusinessAddress;
                    address.City = addressParts.Length > 1 ? addressParts[1].Trim() : null;
                    address.State = addressParts.Length > 2 ? addressParts[2].Trim() : null;

                    // Handle postal code if present (format: "City, State - PostalCode")
                    if (addressParts.Length > 2 && addressParts[2].Contains("-"))
                    {
                        var statePostalParts = addressParts[2].Split('-');
                        address.State = statePostalParts[0].Trim();
                        address.PostalCode = statePostalParts.Length > 1 ? statePostalParts[1].Trim() : null;
                    }
                }

                // 3. Update Shop Name (if changed)
                if (!string.IsNullOrEmpty(dto.ShopName))
                {
                    var shop = await _context.ShopDetails
                        .FirstOrDefaultAsync(s => s.SellerId == dto.SellerId);

                    if (shop != null)
                    {
                        shop.ShopName = dto.ShopName;
                    }
                }

                // 4. Update Bank Details (if any changed)
                if (!string.IsNullOrEmpty(dto.AccountHolder) ||
                    !string.IsNullOrEmpty(dto.AccountNumber) ||
                    !string.IsNullOrEmpty(dto.BankName) ||
                    !string.IsNullOrEmpty(dto.SwiftCode))
                {
                    var shop = await _context.ShopDetails
                        .FirstOrDefaultAsync(s => s.SellerId == dto.SellerId);

                    if (shop != null)
                    {
                        var bankDetails = await _context.BankDetails
                            .FirstOrDefaultAsync(b => b.ShopId == shop.Id);

                        if (bankDetails == null)
                        {
                            bankDetails = new BankDetails
                            {
                                ShopId = shop.Id,
                                IsPrimary = true
                            };
                            _context.BankDetails.Add(bankDetails);
                        }

                        if (!string.IsNullOrEmpty(dto.AccountHolder))
                            bankDetails.AccountHolder = dto.AccountHolder;

                        if (!string.IsNullOrEmpty(dto.AccountNumber))
                            bankDetails.IBAN = dto.AccountNumber;

                        if (!string.IsNullOrEmpty(dto.BankName))
                            bankDetails.BankName = dto.BankName;

                        if (!string.IsNullOrEmpty(dto.SwiftCode))
                            bankDetails.SwiftCode = dto.SwiftCode;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResponse<string>(200, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<string>(500, $"Error updating profile: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SellerProfileUpdateDTO>> GetSellerProfileAsync(int sellerId)
        {
            try
            {
                var seller = await _context.Sellers
                    .Include(s => s.SellerAddresses)
                    .Include(s => s.ShopDetails)
                    .ThenInclude(shop => shop.BankDetails)
                    .FirstOrDefaultAsync(s => s.Id == sellerId);

                if (seller == null)
                    return new ApiResponse<SellerProfileUpdateDTO>(404, "Seller not found");

                // Get primary address or first address
                var address = seller.SellerAddresses.FirstOrDefault(a => a.IsDefault) ??
                             seller.SellerAddresses.FirstOrDefault();

                // Format address as single string
                var formattedAddress = address != null ?
                    $"{address.Street}, {address.City}, {address.State} - {address.PostalCode}" :
                    string.Empty;

                // Get shop and bank details
                var shop = seller.ShopDetails.FirstOrDefault();
                var bankDetails = shop?.BankDetails.FirstOrDefault();

                var response = new SellerProfileUpdateDTO
                {
                    SellerId = seller.Id,
                    PhoneNumber = seller.PhoneNumber,
                    BusinessAddress = formattedAddress,
                    ShopName = shop?.ShopName,
                    AccountHolder = bankDetails?.AccountHolder,
                    AccountNumber = bankDetails?.IBAN,
                    BankName = bankDetails?.BankName,
                    SwiftCode = bankDetails?.SwiftCode
                };

                return new ApiResponse<SellerProfileUpdateDTO>(200, response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<SellerProfileUpdateDTO>(500, $"Error retrieving profile: {ex.Message}");
            }
        }
    }
}