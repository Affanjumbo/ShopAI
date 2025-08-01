using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShopAI.Data;
using ShopAI.DTOs;
using ShopAI.DTOs.CustomerDTOs;
using ShopAI.Helpers;
using ShopAI.Models;

namespace ShopAI.Services
{
    public class CustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public CustomerService(ApplicationDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<ApiResponse<CustomerResponseDTO>> RegisterCustomerAsync(CustomerRegistrationDTO customerDto)
        {
            try
            {
                if (await _context.Customers.AnyAsync(c => c.Email.ToLower() == customerDto.Email.ToLower()))
                {
                    return new ApiResponse<CustomerResponseDTO>(400, "Email is already in use.");
                }

                if (await _context.Customers.AnyAsync(c => c.PhoneNumber == customerDto.PhoneNumber))
                {
                    return new ApiResponse<CustomerResponseDTO>(400, "Phone Number is already used");
                }

                var customer = new Customer
                {
                    FirstName = customerDto.FirstName,
                    LastName = customerDto.LastName,
                    Email = customerDto.Email,
                    PhoneNumber = customerDto.PhoneNumber,
                    DateOfBirth = customerDto.DateOfBirth,
                    IsActive = true,
                    Password = BCrypt.Net.BCrypt.HashPassword(customerDto.Password)
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                var customerResponse = new CustomerResponseDTO
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    DateOfBirth = customer.DateOfBirth
                };

                return new ApiResponse<CustomerResponseDTO>(200, customerResponse);
            }
            catch (Exception ex)
            {
                return new ApiResponse<CustomerResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<LoginResponseDTO>> LoginAsync(LoginDTO loginDto)
        {
            try
            {
                var customer = await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Email == loginDto.Email);

                if (customer == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, customer.Password))
                {
                    return new ApiResponse<LoginResponseDTO>(401, "Invalid email or password.");
                }

                //Use injected JwtSettings
                var token = JwtHelper.GenerateToken(customer.Id.ToString(), "Customer", _jwtSettings);

                var loginResponse = new LoginResponseDTO
                {
                    Message = "Login successful.",
                    CustomerId = customer.Id,
                    CustomerName = $"{customer.FirstName} {customer.LastName}",
                    Token = token
                };

                return new ApiResponse<LoginResponseDTO>(200, loginResponse);
            }
            catch (Exception ex)
            {
                return new ApiResponse<LoginResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CustomerResponseDTO>> GetCustomerByIdAsync(int id)
        {
            try
            {
                var customer = await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive == true);

                if (customer == null)
                {
                    return new ApiResponse<CustomerResponseDTO>(404, "Customer not found.");
                }

                var customerResponse = new CustomerResponseDTO
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    DateOfBirth = customer.DateOfBirth
                };

                return new ApiResponse<CustomerResponseDTO>(200, customerResponse);
            }
            catch (Exception ex)
            {
                return new ApiResponse<CustomerResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> UpdateCustomerAsync(CustomerUpdateDTO customerDto)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerDto.CustomerId);
                if (customer == null)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, "Customer not found.");
                }

                if (customer.Email != customerDto.Email && await _context.Customers.AnyAsync(c => c.Email == customerDto.Email))
                {
                    return new ApiResponse<ConfirmationResponseDTO>(400, "Email is already in use.");
                }

                customer.FirstName = customerDto.FirstName;
                customer.LastName = customerDto.LastName;
                customer.Email = customerDto.Email;
                customer.PhoneNumber = customerDto.PhoneNumber;
                customer.DateOfBirth = customerDto.DateOfBirth;

                await _context.SaveChangesAsync();

                var confirmationMessage = new ConfirmationResponseDTO
                {
                    Message = $"Customer with Id {customerDto.CustomerId} updated successfully."
                };

                return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> DeleteCustomerAsync(int id)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (customer == null)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, "Customer not found.");
                }

                customer.IsActive = false;
                await _context.SaveChangesAsync();

                var confirmationMessage = new ConfirmationResponseDTO
                {
                    Message = $"Customer with Id {id} deleted successfully."
                };

                return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> ChangePasswordAsync(ChangePasswordDTO changePasswordDto)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(changePasswordDto.CustomerId);
                if (customer == null || !customer.IsActive)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, "Customer not found or inactive.");
                }

                bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, customer.Password);
                if (!isCurrentPasswordValid)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(401, "Current password is incorrect.");
                }

                customer.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

                await _context.SaveChangesAsync();

                var confirmationMessage = new ConfirmationResponseDTO
                {
                    Message = "Password changed successfully."
                };

                return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<CustomerInsightDTO> GetCustomerInsightsAsync(int sellerId)
        {
            var sellerProductIds = await _context.Products
                .Where(p => p.SellerId == sellerId)
                .Select(p => p.Id)
                .ToListAsync();

            var customerIds = await _context.OrderItems
                .Where(oi => sellerProductIds.Contains(oi.ProductId))
                .Select(oi => oi.Order.CustomerId)
                .Distinct()
                .ToListAsync();

            var totalCustomers = customerIds.Count;

            var repeatCustomers = await _context.OrderItems
                .Where(oi => sellerProductIds.Contains(oi.ProductId))
                .GroupBy(oi => oi.Order.CustomerId)
                .Where(g => g.Count() > 1)
                .CountAsync();

            double avgRating = await _context.Feedbacks
                .Where(f => sellerProductIds.Contains(f.ProductId))
                .AnyAsync()
                ? (double)await _context.Feedbacks
                    .Where(f => sellerProductIds.Contains(f.ProductId))
                    .AverageAsync(f => f.Rating)
                : 0;

            double repeatPercent = totalCustomers > 0
                ? (repeatCustomers * 100.0) / totalCustomers
                : 0;

            return new CustomerInsightDTO
            {
                TotalCustomers = totalCustomers,
                RepeatCustomers = repeatCustomers,
                RepeatCustomerPercentage = Math.Round(repeatPercent, 2),
                AvgRating = Math.Round(avgRating, 1)
            };
        }

        public async Task<List<CustomerWithOrdersDTO>> GetCustomerOrderSummaryAsync(int sellerId)
        {
            var sellerProductIds = await _context.Products
                .Where(p => p.SellerId == sellerId)
                .Select(p => p.Id)
                .ToListAsync();

            var customerIds = await _context.OrderItems
                .Where(oi => sellerProductIds.Contains(oi.ProductId))
                .Select(oi => oi.Order.CustomerId)
                .Distinct()
                .ToListAsync();

            return await _context.Customers
                .Where(c => customerIds.Contains(c.Id))
                .Select(c => new CustomerWithOrdersDTO
                {
                    CustomerId = c.Id,
                    CustomerName = c.FirstName + " " + c.LastName,
                    TotalOrders = _context.OrderItems
                        .Count(oi => sellerProductIds.Contains(oi.ProductId) && oi.Order.CustomerId == c.Id),
                    TotalSpent = _context.OrderItems
                        .Where(oi => sellerProductIds.Contains(oi.ProductId) && oi.Order.CustomerId == c.Id)
                        .Sum(oi => (decimal?)oi.TotalPrice) ?? 0
                })
                .ToListAsync();
        }
    }
}
