using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using ShopAI.Data;
using ShopAI.DTOs.OrderDTOs;
using ShopAI.DTOs;
using ShopAI.Models;

namespace ShopAI.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _context;

        // Allowed order status transitions for validating status changes.
        private static readonly Dictionary<OrderStatus, List<OrderStatus>> AllowedStatusTransitions = new()
        {
            { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Canceled } },
            { OrderStatus.Processing, new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.Canceled } },
            { OrderStatus.Shipped, new List<OrderStatus> { OrderStatus.Delivered } },
            { OrderStatus.Delivered, new List<OrderStatus>() }, // Terminal state
            { OrderStatus.Canceled, new List<OrderStatus>() }   // Terminal state
        };

        // Inject the ApplicationDbContext.
        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Creates a new order from the provided OrderCreateDTO.
        // This method validates customer and address data, checks product stock,
        // calculates financial totals, deducts product stock, and marks any active cart as checked out.
        public async Task<ApiResponse<OrderResponseDTO>> CreateOrderAsync(OrderCreateDTO orderDto)
        {
            try
            {
                // Validate that the customer exists.
                var customer = await _context.Customers.FindAsync(orderDto.CustomerId);
                if (customer == null)
                {
                    return new ApiResponse<OrderResponseDTO>(404, "Customer does not exist.");
                }

                // Validate that the billing address exists and belongs to the customer.
                var billingAddress = await _context.Addresses.FindAsync(orderDto.BillingAddressId);
                if (billingAddress == null || billingAddress.CustomerId != orderDto.CustomerId)
                {
                    return new ApiResponse<OrderResponseDTO>(400, "Billing Address is invalid or does not belong to the customer.");
                }

                // Validate that the shipping address exists and belongs to the customer.
                var shippingAddress = await _context.Addresses.FindAsync(orderDto.ShippingAddressId);
                if (shippingAddress == null || shippingAddress.CustomerId != orderDto.CustomerId)
                {
                    return new ApiResponse<OrderResponseDTO>(400, "Shipping Address is invalid or does not belong to the customer.");
                }

                // Initialize financial tracking.
                decimal totalBaseAmount = 0;
                decimal totalDiscountAmount = 0;
                decimal shippingCost = 10.00m; // Example fixed shipping cost.
                decimal totalAmount = 0;

                // Generate a unique order number.
                string orderNumber = GenerateOrderNumber();

                // List to hold order items.
                var orderItems = new List<OrderItem>();

                // Process each order item from the DTO.
                foreach (var itemDto in orderDto.OrderItems)
                {
                    // Check if the product exists.
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        return new ApiResponse<OrderResponseDTO>(404, $"Product with ID {itemDto.ProductId} does not exist.");
                    }

                    // Check if sufficient stock is available.
                    if (product.StockQuantity < itemDto.Quantity)
                    {
                        return new ApiResponse<OrderResponseDTO>(400, $"Insufficient stock for product {product.Name}.");
                    }

                    // Calculate base price, discount, and total price for the order item.
                    decimal basePrice = itemDto.Quantity * product.Price;
                    decimal discount = (product.DiscountPercentage / 100.0m) * basePrice;
                    decimal totalPrice = basePrice - discount;

                    // Create a new OrderItem.
                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.Price,
                        Discount = discount,
                        TotalPrice = totalPrice
                    };

                    // Add the order item to the list.
                    orderItems.Add(orderItem);

                    // Update the running totals.
                    totalBaseAmount += basePrice;
                    totalDiscountAmount += discount;

                    // Deduct the purchased quantity from the product’s stock.
                    product.StockQuantity -= itemDto.Quantity;
                    _context.Products.Update(product);
                }

                // Calculate the final total amount.
                totalAmount = totalBaseAmount - totalDiscountAmount + shippingCost;

                // Manually map from DTO to Order model.
                var order = new Order
                {
                    OrderNumber = orderNumber,
                    CustomerId = orderDto.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    BillingAddressId = orderDto.BillingAddressId,
                    ShippingAddressId = orderDto.ShippingAddressId,
                    TotalBaseAmount = totalBaseAmount,
                    TotalDiscountAmount = totalDiscountAmount,
                    ShippingCost = shippingCost,
                    TotalAmount = totalAmount,
                    OrderStatus = OrderStatus.Pending,
                    OrderItems = orderItems
                };

                // Add the order to the database.
                _context.Orders.Add(order);

                // Mark the customer's active cart as checked out (if it exists).
                var cart = await _context.Carts.FirstOrDefaultAsync(c => c.CustomerId == orderDto.CustomerId && !c.IsCheckedOut);
                if (cart != null)
                {
                    cart.IsCheckedOut = true;
                    cart.UpdatedAt = DateTime.UtcNow;
                    _context.Carts.Update(cart);
                }

                // Save all changes.
                await _context.SaveChangesAsync();

                

                // Map the saved order to OrderResponseDTO.
                var orderResponse = MapOrderToDTO(order, customer, billingAddress, shippingAddress);
                return new ApiResponse<OrderResponseDTO>(200, orderResponse);
            }
            catch (Exception ex)
            {
                return new ApiResponse<OrderResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        // Retrieves an order by its ID along with related entities.
        public async Task<ApiResponse<OrderResponseDTO>> GetOrderByIdAsync(int orderId)
        {
            try
            {
                // Retrieve the order with its items, customer, and addresses details.
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.Customer)
                    .Include(o => o.BillingAddress)
                    .Include(o => o.ShippingAddress)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return new ApiResponse<OrderResponseDTO>(404, "Order not found.");
                }

                // Map the order to a DTO.
                var orderResponse = MapOrderToDTO(order, order.Customer, order.BillingAddress, order.ShippingAddress);
                return new ApiResponse<OrderResponseDTO>(200, orderResponse);
            }
            catch (Exception ex)
            {
                return new ApiResponse<OrderResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        // Updates the status of an existing order.
        // Validates allowed status transitions before applying the update.
        public async Task<bool> UpdateOrderStatusAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            switch (order.OrderStatus)
            {
                case OrderStatus.Pending:
                    order.OrderStatus = OrderStatus.Processing;
                    break;
                case OrderStatus.Processing:
                    order.OrderStatus = OrderStatus.Shipped;
                    break;
                case OrderStatus.Shipped:
                    order.OrderStatus = OrderStatus.Delivered;
                    break;
                case OrderStatus.Delivered:
                case OrderStatus.Canceled:
                    // Cannot change from final states
                    return false;
                default:
                    return false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            if (order.OrderStatus == OrderStatus.Shipped ||
                order.OrderStatus == OrderStatus.Delivered ||
                order.OrderStatus == OrderStatus.Canceled)
                return false;

            order.OrderStatus = OrderStatus.Canceled;
            await _context.SaveChangesAsync();
            return true;
        }



        // Retrieves all orders in the system.
        public async Task<ApiResponse<List<OrderResponseDTO>>> GetAllOrdersAsync()
        {
            try
            {
                // Retrieve all orders with related entities.
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.Customer)
                    .Include(o => o.BillingAddress)
                    .Include(o => o.ShippingAddress)
                    .AsNoTracking()
                    .ToListAsync();

                // Map each order to its corresponding DTO.
                var orderList = orders.Select(o => MapOrderToDTO(o, o.Customer, o.BillingAddress, o.ShippingAddress)).ToList();
                return new ApiResponse<List<OrderResponseDTO>>(200, orderList);
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<OrderResponseDTO>>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        // Retrieves all orders associated with a specific customer.
        public async Task<ApiResponse<List<OrderResponseDTO>>> GetOrdersByCustomerAsync(int customerId)
        {
            try
            {
                // Retrieve the customer along with their orders and related data.
                var customer = await _context.Customers
                    .Include(c => c.Orders)
                        .ThenInclude(o => o.OrderItems)
                            .ThenInclude(oi => oi.Product)
                    .Include(c => c.Addresses)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null)
                {
                    return new ApiResponse<List<OrderResponseDTO>>(404, "Customer not found.");
                }

                // Map each order to a DTO.
                var orders = customer.Orders.Select(o => MapOrderToDTO(o, customer, o.BillingAddress, o.ShippingAddress)).ToList();
                return new ApiResponse<List<OrderResponseDTO>>(200, orders);
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<OrderResponseDTO>>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<OrderResponseDTO>>> GetOrdersBySellerAsync(int sellerId)
        {
            try
            {
                // First verify the seller exists
                var sellerExists = await _context.Products.AnyAsync(p => p.SellerId == sellerId);
                if (!sellerExists)
                {
                    return new ApiResponse<List<OrderResponseDTO>>(404, "Seller not found or has no products.");
                }

                // Get orders that contain products from this seller
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.Customer)
                    .Include(o => o.BillingAddress)
                    .Include(o => o.ShippingAddress)
                    .Where(o => o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
                    .AsNoTracking()
                    .ToListAsync();

                if (!orders.Any())
                {
                    return new ApiResponse<List<OrderResponseDTO>>(404, "No orders found for this seller.");
                }

                var orderList = orders.Select(o => MapOrderToDTO(o, o.Customer, o.BillingAddress, o.ShippingAddress)).ToList();
                return new ApiResponse<List<OrderResponseDTO>>(200, orderList);
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<OrderResponseDTO>>(500,
                    $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        // Maps an Order entity to an OrderResponseDTO.
        private OrderResponseDTO MapOrderToDTO(Order order, Customer customer, Address billingAddress, Address shippingAddress)
        {
            // Map order items.
            var orderItemsDto = order.OrderItems.Select(oi => new OrderItemResponseDTO
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                ProductImage = oi.Product.ProductImage,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Discount = oi.Discount,
                TotalPrice = oi.TotalPrice
            }).ToList();

            // Create and return the DTO.
            return new OrderResponseDTO
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                CustomerId = order.CustomerId,
                CustomerName = $"{customer.FirstName} {customer.LastName}",
                BillingAddressId = order.BillingAddressId,
                ShippingAddressId = order.ShippingAddressId,
                TotalBaseAmount = order.TotalBaseAmount,
                TotalDiscountAmount = order.TotalDiscountAmount,
                ShippingCost = order.ShippingCost,
                TotalAmount = Math.Round(order.TotalAmount, 2),
                OrderStatus = order.OrderStatus,
                OrderItems = orderItemsDto
            };
        }

        public async Task<ApiResponse<List<TopSellingProductDTO>>> GetTopSellingProductsAsync()
        {
            try
            {
                var topProducts = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new TopSellingProductDTO
                    {
                        ProductId = g.Key,
                        ProductName = g.First().Product.Name,
                        TotalQuantitySold = g.Sum(oi => oi.Quantity),
                        TotalRevenue = g.Sum(oi => oi.TotalPrice),
                        ProductImage = g.First().Product.ProductImage
                    })
                    .OrderByDescending(p => p.TotalQuantitySold)
                    .Take(5)
                    .ToListAsync();

                return new ApiResponse<List<TopSellingProductDTO>>(200, topProducts);
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<TopSellingProductDTO>>(500,
                    $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<TopSellingProductDTO>>> GetTopSellingProductsBySellerAsync(int sellerId)
        {
            try
            {
                // First verify the seller exists
                var sellerExists = await _context.Products.AnyAsync(p => p.SellerId == sellerId);
                if (!sellerExists)
                {
                    return new ApiResponse<List<TopSellingProductDTO>>(404, "Seller not found.");
                }

                var topProducts = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .Where(oi => oi.Product.SellerId == sellerId) // Filter by seller
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new TopSellingProductDTO
                    {
                        ProductId = g.Key,
                        ProductName = g.First().Product.Name,
                        TotalQuantitySold = g.Sum(oi => oi.Quantity),
                        TotalRevenue = g.Sum(oi => oi.TotalPrice),
                        ProductImage = g.First().Product.ProductImage,
                        SellerId = sellerId
                    })
                    .OrderByDescending(p => p.TotalQuantitySold)
                    .Take(5)
                    .ToListAsync();

                if (!topProducts.Any())
                {
                    return new ApiResponse<List<TopSellingProductDTO>>(404, "No sales data found for this seller.");
                }

                return new ApiResponse<List<TopSellingProductDTO>>(200, topProducts);
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<TopSellingProductDTO>>(500,
                    $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> GetRevenueStatsBySellerAsync(int sellerId)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Where(o => o.OrderStatus == OrderStatus.Delivered &&
                                o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
                    .AsNoTracking()
                    .ToListAsync();

                if (!orders.Any())
                {
                    return new ApiResponse<object>(404, "No delivered orders found for this seller.");
                }

                var monthlyRevenue = orders
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Revenue = g.Sum(o => o.OrderItems
                            .Where(oi => oi.Product.SellerId == sellerId)
                            .Sum(oi => oi.TotalPrice))
                    })
                    .OrderBy(r => r.Year).ThenBy(r => r.Month)
                    .ToList();

                var yearlyRevenue = monthlyRevenue
                    .GroupBy(r => r.Year)
                    .Select(g => new
                    {
                        Year = g.Key,
                        Revenue = g.Sum(x => x.Revenue)
                    })
                    .OrderBy(r => r.Year)
                    .ToList();

                var totalRevenue = yearlyRevenue.Sum(y => y.Revenue);

                return new ApiResponse<object>(200, new
                {
                    MonthlyRevenue = monthlyRevenue,
                    YearlyRevenue = yearlyRevenue,
                    TotalRevenue = totalRevenue
                });
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>(500, $"An error occurred while calculating revenue: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SalesReportDTO>> GetSalesReportAsync(DateTime startDate, DateTime endDate, int sellerId)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.OrderItems.Any(i => i.Product.SellerId == sellerId)
                             && o.OrderDate >= startDate
                             && o.OrderDate <= endDate
                             && o.OrderStatus == OrderStatus.Delivered) // Only count successful orders
                    .ToListAsync();

                var totalRevenue = orders.Sum(o => o.TotalAmount);
                var totalOrders = orders.Count;
                var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

                // Conversion rate logic can be added if you track visits or views.
                var report = new SalesReportDTO
                {
                    TotalRevenue = totalRevenue,
                    TotalOrders = totalOrders,
                    AverageOrderValue = averageOrderValue,
                };

                return new ApiResponse<SalesReportDTO>(200, report);
            }
            catch (Exception ex)
            {
                return new ApiResponse<SalesReportDTO>(500, $"Failed to generate sales report: {ex.Message}");
            }
        }




        #region Helper Methods

        // Generates a unique order number using the current UTC date/time and a random number.
        // Format: ORD-yyyyMMdd-HHmmss-XXXX
        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")}-{RandomNumber(1000, 9999)}";
        }

        // Generates a random number between min and max.
        private int RandomNumber(int min, int max)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return Math.Abs(BitConverter.ToInt32(bytes, 0) % (max - min + 1)) + min;
            }
        }

        #endregion
    }
}