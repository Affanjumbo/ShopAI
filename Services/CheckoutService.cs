using ShopAI.Data;
using ShopAI.DTOs.OrderDTOs;
using ShopAI.DTOs;
using ShopAI.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopAI.Services
{
    public class CheckoutService
    {
        private readonly ApplicationDbContext _context;
        private readonly OrderService _orderService;

        public CheckoutService(ApplicationDbContext context, OrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        public async Task<ApiResponse<string>> ProceedToCheckoutAsync(int cartId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartId && !c.IsCheckedOut);

            if (cart == null)
                return new ApiResponse<string>(404, "Cart not found or already checked out.");

            if (cart.CartItems == null || !cart.CartItems.Any())
                return new ApiResponse<string>(400, "Cart is empty.");

            var customer = await _context.Customers.FindAsync(cart.CustomerId);
            if (customer == null)
                return new ApiResponse<string>(404, "Customer not found.");

            var billingAddress = await _context.Addresses
                .FirstOrDefaultAsync(a => a.CustomerId == customer.Id && a.IsDefaultBilling);
            var shippingAddress = await _context.Addresses
                .FirstOrDefaultAsync(a => a.CustomerId == customer.Id && a.IsDefaultShipping);

            if (billingAddress == null || shippingAddress == null)
                return new ApiResponse<string>(400, "Customer does not have default billing or shipping address.");

            var orderDto = new OrderCreateDTO
            {
                CustomerId = customer.Id,
                BillingAddressId = billingAddress.Id,
                ShippingAddressId = shippingAddress.Id,
                OrderItems = cart.CartItems.Select(ci => new OrderItemCreateDTO
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity
                }).ToList()
            };

            var orderResult = await _orderService.CreateOrderAsync(orderDto);

            if (orderResult.StatusCode != 200)
                return new ApiResponse<string>(orderResult.StatusCode, orderResult.Message);

            return new ApiResponse<string>(200, "Order is placed!");
        }
    }
}
