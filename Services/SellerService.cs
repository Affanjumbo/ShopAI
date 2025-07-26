using Microsoft.EntityFrameworkCore;
using ShopAI.Data;
using ShopAI.DTOs.SellerDTOs;
using ShopAI.DTOs;
using ShopAI.Models;
using ShopAI.Helpers;
using Microsoft.Extensions.Options;

namespace ShopAI.Services
{
    public class SellerService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public SellerService(ApplicationDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<ApiResponse<SellerResponseDTO>> RegisterSellerAsync(SellerRegistrationDTO sellerDto)
        {
            try
            {
                if (await _context.Sellers.AnyAsync(s => s.Email.ToLower() == sellerDto.Email.ToLower()))
                {
                    return new ApiResponse<SellerResponseDTO>(400, "Email is already in use.");
                }

                if (await _context.Sellers.AnyAsync(s => s.PhoneNumber == sellerDto.PhoneNumber))
                {
                    return new ApiResponse<SellerResponseDTO>(400, "Phone Number is already in use.");
                }

                if (await _context.Sellers.AnyAsync(s => s.Email.ToLower() == sellerDto.Email.ToLower() && s.PhoneNumber == sellerDto.PhoneNumber))
                {
                    return new ApiResponse<SellerResponseDTO>(400, "Email and Phone Number are already in use.");
                }

                var seller = new Seller
                {
                    FirstName = sellerDto.FirstName,
                    LastName = sellerDto.LastName,
                    Email = sellerDto.Email,
                    PhoneNumber = sellerDto.PhoneNumber,
                    City = sellerDto.City,
                    DateOfBirth = sellerDto.DateOfBirth,
                    IsVerified = false,
                    IsActive = true,
                    RegisteredAt = DateTime.UtcNow,
                    Password = BCrypt.Net.BCrypt.HashPassword(sellerDto.Password),
                };

                _context.Sellers.Add(seller);
                await _context.SaveChangesAsync();

                // Generate token after successful registration
                var token = JwtHelper.GenerateToken(seller.Id.ToString(), "Seller", _jwtSettings);

                var response = new SellerResponseDTO
                {
                    Id = seller.Id,
                    FirstName = seller.FirstName,
                    LastName = seller.LastName,
                    Email = seller.Email,
                    PhoneNumber = seller.PhoneNumber,
                    City = seller.City,
                    DateOfBirth = seller.DateOfBirth,
                    IsVerified = seller.IsVerified,
                    IsActive = seller.IsActive,
                    RegisteredAt = seller.RegisteredAt,
                    ProfileImageUrl = seller.ProfileImageUrl
                };

                // Include token in the response along with other seller info
                return new ApiResponse<SellerResponseDTO>(200, response, token);
            }
            catch (Exception ex)
            {
                return new ApiResponse<SellerResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }


        public async Task<ApiResponse<SellerLoginResponseDTO>> LoginAsync(SellerLoginDTO loginDto)
        {
            try
            {
                var seller = await _context.Sellers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Email == loginDto.Email);

                if (seller == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, seller.Password))
                {
                    return new ApiResponse<SellerLoginResponseDTO>(401, "Invalid email or password.");
                }

                // Generate token using the injected JwtSettings
                var token = JwtHelper.GenerateToken(seller.Id.ToString(), "Seller", _jwtSettings);

                var response = new SellerLoginResponseDTO
                {
                    Message = "Login successful.",
                    SellerId = seller.Id,
                    SellerName = $"{seller.FirstName} {seller.LastName}",
                    Token = token // Include the token in the response
                };

                return new ApiResponse<SellerLoginResponseDTO>(200, response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<SellerLoginResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }


        public async Task<ApiResponse<SellerResponseDTO>> GetSellerByIdAsync(int id)
        {
            try
            {
                var seller = await _context.Sellers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

                if (seller == null)
                {
                    return new ApiResponse<SellerResponseDTO>(404, "Seller not found.");
                }

                var response = new SellerResponseDTO
                {
                    Id = seller.Id,
                    FirstName = seller.FirstName,
                    LastName = seller.LastName,
                    Email = seller.Email,
                    PhoneNumber = seller.PhoneNumber,
                    City = seller.City,
                    DateOfBirth = seller.DateOfBirth,
                    IsVerified = seller.IsVerified,
                    IsActive = seller.IsActive,
                    RegisteredAt = seller.RegisteredAt,
                    ProfileImageUrl = seller.ProfileImageUrl
                };

                return new ApiResponse<SellerResponseDTO>(200, response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<SellerResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SellerConfirmationResponseDTO>> UpdateSellerAsync(SellerUpdateDTO sellerDto)
        {
            try
            {
                var seller = await _context.Sellers.FindAsync(sellerDto.SellerId);
                if (seller == null)
                {
                    return new ApiResponse<SellerConfirmationResponseDTO>(404, "Seller not found.");
                }

                if (seller.Email != sellerDto.Email &&
                    await _context.Sellers.AnyAsync(s => s.Email == sellerDto.Email))
                {
                    return new ApiResponse<SellerConfirmationResponseDTO>(400, "Email is already in use.");
                }

                seller.FirstName = sellerDto.FirstName;
                seller.LastName = sellerDto.LastName;
                seller.Email = sellerDto.Email;
                seller.PhoneNumber = sellerDto.PhoneNumber;
                seller.City = sellerDto.City;
                seller.DateOfBirth = sellerDto.DateOfBirth;
                seller.ProfileImageUrl = sellerDto.ProfileImageUrl;

                await _context.SaveChangesAsync();

                return new ApiResponse<SellerConfirmationResponseDTO>(200,
                    new SellerConfirmationResponseDTO { Message = $"Seller with Id {sellerDto.SellerId} updated successfully." });
            }
            catch (Exception ex)
            {
                return new ApiResponse<SellerConfirmationResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SellerConfirmationResponseDTO>> DeleteSellerAsync(int id)
        {
            try
            {
                var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.Id == id);
                if (seller == null)
                {
                    return new ApiResponse<SellerConfirmationResponseDTO>(404, "Seller not found.");
                }

                seller.IsActive = false;
                await _context.SaveChangesAsync();

                return new ApiResponse<SellerConfirmationResponseDTO>(200,
                    new SellerConfirmationResponseDTO { Message = $"Seller with Id {id} deleted successfully." });
            }
            catch (Exception ex)
            {
                return new ApiResponse<SellerConfirmationResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SellerConfirmationResponseDTO>> ChangePasswordAsync(SellerChangePasswordDTO changePasswordDto)
        {
            try
            {
                var seller = await _context.Sellers.FindAsync(changePasswordDto.SellerId);
                if (seller == null || !seller.IsActive)
                {
                    return new ApiResponse<SellerConfirmationResponseDTO>(404, "Seller not found or inactive.");
                }

                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, seller.Password))
                {
                    return new ApiResponse<SellerConfirmationResponseDTO>(401, "Current password is incorrect.");
                }

                seller.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                await _context.SaveChangesAsync();

                return new ApiResponse<SellerConfirmationResponseDTO>(200,
                    new SellerConfirmationResponseDTO { Message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return new ApiResponse<SellerConfirmationResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
