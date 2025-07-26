using Microsoft.EntityFrameworkCore;
using ShopAI.Data;
using ShopAI.DTOs.ShopDTOs;
using ShopAI.Models;
using ShopAI.DTOs;

namespace ShopAI.Services
{
    public class ShopDetailsService
    {
        private readonly ApplicationDbContext _context;

        public ShopDetailsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<ShopDetailsResponseDTO>> CreateShopAsync(ShopDetailsCreateDTO dto)
        {
            try
            {
                bool shopExists = await _context.ShopDetails
            .AnyAsync(s => s.ShopName.ToLower() == dto.ShopName.ToLower());

                if (shopExists)
                {
                    return new ApiResponse<ShopDetailsResponseDTO>(409, "A shop with this name already exists.");
                }

                var shop = new ShopDetails
                {
                    SellerId = dto.SellerId,
                    ShopName = dto.ShopName,
                    WebsiteUrl = dto.WebsiteUrl,
                    IsPrimary = dto.IsPrimary,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ShopDetails.Add(shop);
                await _context.SaveChangesAsync();

                var response = new ShopDetailsResponseDTO
                {
                    Id = shop.Id,
                    SellerId = shop.SellerId,
                    ShopName = shop.ShopName,
                    WebsiteUrl = shop.WebsiteUrl,
                    IsPrimary = shop.IsPrimary,
                    CreatedAt = shop.CreatedAt
                };

                return new ApiResponse<ShopDetailsResponseDTO>(200, response);
            }
            catch (DbUpdateException ex)
            {
                return new ApiResponse<ShopDetailsResponseDTO>(500, $"Database update error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopDetailsResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ShopDetailsResponseDTO>> GetShopByIdAsync(int id)
        {
            try
            {
                var shop = await _context.ShopDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (shop == null)
                    return new ApiResponse<ShopDetailsResponseDTO>(404, "Shop not found.");

                var response = new ShopDetailsResponseDTO
                {
                    Id = shop.Id,
                    SellerId = shop.SellerId,
                    ShopName = shop.ShopName,
                    WebsiteUrl = shop.WebsiteUrl,
                    IsPrimary = shop.IsPrimary,
                    CreatedAt = shop.CreatedAt
                };

                return new ApiResponse<ShopDetailsResponseDTO>(200, response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopDetailsResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ShopDetailsResponseDTO>>> GetShopsBySellerIdAsync(int sellerId)
        {
            try
            {
                var shops = await _context.ShopDetails
                    .Where(s => s.SellerId == sellerId)
                    .AsNoTracking()
                    .ToListAsync();

                var result = shops.Select(s => new ShopDetailsResponseDTO
                {
                    Id = s.Id,
                    SellerId = s.SellerId,
                    ShopName = s.ShopName,
                    WebsiteUrl = s.WebsiteUrl,
                    IsPrimary = s.IsPrimary,
                    CreatedAt = s.CreatedAt
                }).ToList();

                return new ApiResponse<List<ShopDetailsResponseDTO>>(200, result);
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ShopDetailsResponseDTO>>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ShopDetailsConfirmationResponseDTO>> UpdateShopAsync(ShopDetailsUpdateDTO dto)
        {
            try
            {
                var shop = await _context.ShopDetails.FindAsync(dto.Id);
                if (shop == null)
                    return new ApiResponse<ShopDetailsConfirmationResponseDTO>(404, "Shop not found.");

                shop.ShopName = dto.ShopName;
                shop.WebsiteUrl = dto.WebsiteUrl;
                shop.IsPrimary = dto.IsPrimary;

                await _context.SaveChangesAsync();

                return new ApiResponse<ShopDetailsConfirmationResponseDTO>(200,
                    new ShopDetailsConfirmationResponseDTO { Message = $"Shop with Id {dto.Id} updated successfully." });
            }
            catch (DbUpdateException ex)
            {
                return new ApiResponse<ShopDetailsConfirmationResponseDTO>(500, $"Database update error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopDetailsConfirmationResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ShopDetailsConfirmationResponseDTO>> DeleteShopAsync(int id)
        {
            try
            {
                var shop = await _context.ShopDetails.FindAsync(id);
                if (shop == null)
                    return new ApiResponse<ShopDetailsConfirmationResponseDTO>(404, "Shop not found.");

                _context.ShopDetails.Remove(shop);
                await _context.SaveChangesAsync();

                return new ApiResponse<ShopDetailsConfirmationResponseDTO>(200,
                    new ShopDetailsConfirmationResponseDTO { Message = $"Shop with Id {id} deleted successfully." });
            }
            catch (DbUpdateException ex)
            {
                return new ApiResponse<ShopDetailsConfirmationResponseDTO>(500, $"Database update error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopDetailsConfirmationResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
