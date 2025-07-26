using System.Xml;
using Microsoft.EntityFrameworkCore;
using ShopAI.Data;
using ShopAI.DTOs;
using ShopAI.Models;

namespace ShopAI.Services
{
    public class SellerAddressService
    {
        private readonly ApplicationDbContext _context;

        public SellerAddressService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<SellerAddressResponseDTO>> AddSellerAddressAsync(SellerAddressCreateDTO dto)
        {
            try
            {
                // Check if seller exists
                var sellerExists = await _context.Sellers.AnyAsync(s => s.Id == dto.SellerId
                );
                if (!sellerExists)
                {
                    return new ApiResponse<SellerAddressResponseDTO>(404, "Seller not found.");
                }

                var address = new SellerAddress
                {
                    SellerId = dto.SellerId,
                    AddressType = dto.AddressType,
                    Street = dto.Street,
                    City = dto.City,
                    State = dto.State,
                    PostalCode = dto.PostalCode,
                    IsDefault = dto.IsDefault,
                    Country = dto.Country
                };

                _context.SellerAddresses.Add(address);
                await _context.SaveChangesAsync();

                var response = new SellerAddressResponseDTO
                {
                    Id = address.Id,
                    SellerId = address.SellerId,
                    AddressType = address.AddressType,
                    Street = address.Street,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    IsDefault = address.IsDefault,
                    Country = address.Country
                };

                return new ApiResponse<SellerAddressResponseDTO>(200, response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<SellerAddressResponseDTO>(500, $"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<SellerAddressResponseDTO>>> GetAddressesBySellerIdAsync(int sellerId)
        {
            try
            {
                var addresses = await _context.SellerAddresses
                    .Where(a => a.SellerId == sellerId)
                    .Select(a => new SellerAddressResponseDTO
                    {
                        Id = a.Id,
                        SellerId = a.SellerId,
                        AddressType = a.AddressType,
                        Street = a.Street,
                        City = a.City,
                        State = a.State,
                        PostalCode = a.PostalCode,
                        IsDefault = a.IsDefault,
                        Country = a.Country
                    })
                    .ToListAsync();

                return new ApiResponse<List<SellerAddressResponseDTO>>(200, addresses);
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<SellerAddressResponseDTO>>(500, $"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SellerConfirmationResponseDTO>> UpdateSellerAddressAsync(SellerAddressUpdateDTO dto)
        {
            try
            {
                var address = await _context.SellerAddresses.FindAsync(dto.Id);
                if (address == null)
                {
                    return new ApiResponse<SellerConfirmationResponseDTO>(404, "Address not found.");
                }

                address.Street = dto.Street;
                address.AddressType = dto.AddressType;
                address.City = dto.City;
                address.State = dto.State;
                address.PostalCode = dto.PostalCode;
                address.IsDefault = dto.IsDefault;
                address.Country = dto.Country;

                await _context.SaveChangesAsync();

                return new ApiResponse<SellerConfirmationResponseDTO>(200, new SellerConfirmationResponseDTO
                {
                    Message = $"Address with ID {dto.Id} updated successfully."
                });
            }
            catch (Exception ex)
            {
                return new ApiResponse<SellerConfirmationResponseDTO>(500, $"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SellerConfirmationResponseDTO>> DeleteSellerAddressAsync(int id)
        {
            try
            {
                var address = await _context.SellerAddresses.FindAsync(id);
                if (address == null)
                {
                    return new ApiResponse<SellerConfirmationResponseDTO>(404, "Address not found.");
                }

                _context.SellerAddresses.Remove(address);
                await _context.SaveChangesAsync();

                return new ApiResponse<SellerConfirmationResponseDTO>(200, new SellerConfirmationResponseDTO
                {
                    Message = $"Address with ID {id} deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return new ApiResponse<SellerConfirmationResponseDTO>(500, $"Error: {ex.Message}");
            }
        }
    }
}
