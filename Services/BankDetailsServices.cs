using Microsoft.EntityFrameworkCore;
using ShopAI.Data;
using ShopAI.DTOs.BankDTOs;
using ShopAI.Models;
using ShopAI.DTOs;

namespace ShopAI.Services
{
    public class BankDetailsService
    {
        private readonly ApplicationDbContext _context;

        public BankDetailsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<BankDetailsResponseDTO>> AddBankDetailsAsync(BankDetailsCreateDTO dto)
        {
            try
            {
                // Check if the related Shop exists
                var shop = await _context.ShopDetails.FirstOrDefaultAsync(s => s.Id == dto.ShopId);
                if (shop == null)
                {
                    return new ApiResponse<BankDetailsResponseDTO>(404, "Shop not found.");
                }

                bool ibanOrSwiftExists = await _context.BankDetails
            .AnyAsync(b => b.IBAN == dto.IBAN || b.SwiftCode == dto.SwiftCode);

                if (ibanOrSwiftExists)
                {
                    return new ApiResponse<BankDetailsResponseDTO>(409, "IBAN or Swift Code already exists.");
                }

                var bank = new BankDetails
                {
                    ShopId = dto.ShopId,
                    AccountHolder = dto.AccountHolder,
                    BankName = dto.BankName,
                    IBAN = dto.IBAN,
                    SwiftCode = dto.SwiftCode,
                    StatementUrl = dto.StatementUrl,
                    IsPrimary = dto.IsPrimary
                };

                _context.BankDetails.Add(bank);
                await _context.SaveChangesAsync();

                var response = new BankDetailsResponseDTO
                {
                    Id = bank.Id,
                    ShopId = bank.ShopId,
                    AccountHolder = bank.AccountHolder,
                    BankName = bank.BankName,
                    IBAN = bank.IBAN,
                    SwiftCode = bank.SwiftCode,
                    StatementUrl = bank.StatementUrl,
                    IsPrimary = bank.IsPrimary,
                    AddedOn = bank.AddedOn
                };

                return new ApiResponse<BankDetailsResponseDTO>(200, response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<BankDetailsResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankDetailsResponseDTO>> GetBankDetailsByIdAsync(int id)
        {
            try
            {
                var bank = await _context.BankDetails.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                if (bank == null)
                {
                    return new ApiResponse<BankDetailsResponseDTO>(404, "Bank details not found.");
                }

                var response = new BankDetailsResponseDTO
                {
                    Id = bank.Id,
                    ShopId = bank.ShopId,
                    AccountHolder = bank.AccountHolder,
                    BankName = bank.BankName,
                    IBAN = bank.IBAN,
                    SwiftCode = bank.SwiftCode,
                    StatementUrl = bank.StatementUrl,
                    IsPrimary = bank.IsPrimary,
                    AddedOn = bank.AddedOn
                };

                return new ApiResponse<BankDetailsResponseDTO>(200, response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<BankDetailsResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankDetailsResponseDTO>>> GetBankDetailsByShopIdAsync(int shopId)
        {
            try
            {
                var list = await _context.BankDetails
                    .Where(b => b.ShopId == shopId)
                    .Select(bank => new BankDetailsResponseDTO
                    {
                        Id = bank.Id,
                        ShopId = bank.ShopId,
                        AccountHolder = bank.AccountHolder,
                        BankName = bank.BankName,
                        IBAN = bank.IBAN,
                        SwiftCode = bank.SwiftCode,
                        StatementUrl = bank.StatementUrl,
                        IsPrimary = bank.IsPrimary,
                        AddedOn = bank.AddedOn
                    })
                    .ToListAsync();

                return new ApiResponse<List<BankDetailsResponseDTO>>(200, list);
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<BankDetailsResponseDTO>>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankDetailsResponseDTO>> UpdateBankDetailsAsync(BankDetailsUpdateDTO dto)
        {
            try
            {
                var bank = await _context.BankDetails.FirstOrDefaultAsync(b => b.Id == dto.Id && b.ShopId == dto.ShopId);
                if (bank == null)
                {
                    return new ApiResponse<BankDetailsResponseDTO>(404, "Bank details not found.");
                }

                bank.AccountHolder = dto.AccountHolder;
                bank.BankName = dto.BankName;
                bank.IBAN = dto.IBAN;
                bank.SwiftCode = dto.SwiftCode;
                bank.StatementUrl = dto.StatementUrl;
                bank.IsPrimary = dto.IsPrimary;

                await _context.SaveChangesAsync();

                var response = new BankDetailsResponseDTO
                {
                    Id = bank.Id,
                    ShopId = bank.ShopId,
                    AccountHolder = bank.AccountHolder,
                    BankName = bank.BankName,
                    IBAN = bank.IBAN,
                    SwiftCode = bank.SwiftCode,
                    StatementUrl = bank.StatementUrl,
                    IsPrimary = bank.IsPrimary,
                    AddedOn = bank.AddedOn
                };

                return new ApiResponse<BankDetailsResponseDTO>(200, response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<BankDetailsResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> DeleteBankDetailsAsync(int id)
        {
            try
            {
                var bank = await _context.BankDetails.FindAsync(id);
                if (bank == null)
                {
                    return new ApiResponse<string>(404, "Bank details not found.");
                }

                _context.BankDetails.Remove(bank);
                await _context.SaveChangesAsync();

                return new ApiResponse<string>(200, $"Bank details with Id {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
