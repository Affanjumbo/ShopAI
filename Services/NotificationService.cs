using ShopAI.Data;
using ShopAI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopAI.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get notifications for a user (Customer or Seller)
        public async Task<List<Notification>> GetNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        // Mark a notification as read
        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        // Create a notification for a user (Customer or Seller)
        public async Task CreateNotificationAsync(string userId, string role, string title, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Role = role,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        // Notify Customer when an order is placed
        public async Task NotifyCustomerOrderPlacedAsync(string customerId, string orderNumber)
        {
            var title = "Order Placed Successfully";
            var message = $"Your order with Order Number {orderNumber} has been placed successfully.";
            await CreateNotificationAsync(customerId, "Customer", title, message);
        }

        // Notify Seller when an order is placed for their products
        public async Task NotifySellerOrderPlacedAsync(string sellerId, string orderNumber)
        {
            var title = "New Order Placed";
            var message = $"An order has been placed for your product(s) with Order Number {orderNumber}.";
            await CreateNotificationAsync(sellerId, "Seller", title, message);
        }

        // Notify Customer when the order status changes (e.g., Shipped, Delivered)
        public async Task NotifyCustomerOrderStatusChangedAsync(string customerId, string orderNumber, string newStatus)
        {
            var title = $"Order {newStatus}";
            var message = $"Your order with Order Number {orderNumber} is now {newStatus}.";
            await CreateNotificationAsync(customerId, "Customer", title, message);
        }

        // Notify Seller when the order status changes (e.g., Shipped, Delivered)
        public async Task NotifySellerOrderStatusChangedAsync(string sellerId, string orderNumber, string newStatus)
        {
            var title = $"Order {newStatus}";
            var message = $"The status of your order with Order Number {orderNumber} is now {newStatus}.";
            await CreateNotificationAsync(sellerId, "Seller", title, message);
        }

        // Notify Seller when a new product is added
        public async Task NotifySellerNewProductAsync(string sellerId, string productName)
        {
            var title = "New Product Added";
            var message = $"A new product '{productName}' has been added to your store.";
            await CreateNotificationAsync(sellerId, "Seller", title, message);
        }

        // Notify Seller when their product goes out of stock
        public async Task NotifySellerOutOfStockAsync(string sellerId, string productName)
        {
            var title = "Product Out of Stock";
            var message = $"Your product '{productName}' is now out of stock.";
            await CreateNotificationAsync(sellerId, "Seller", title, message);
        }

        // Notify Seller when their product is restocked
        public async Task NotifySellerProductReplenishedAsync(string sellerId, string productName)
        {
            var title = "Product Re-Added!";
            var message = $"Your product '{productName}' is now back in stock.";
            await CreateNotificationAsync(sellerId, "Seller", title, message);
        }

        public async Task NotifySellerProductEventAsync(string sellerId, string productId, string eventType)
        {
            string title = string.Empty;
            string message = string.Empty;

            switch (eventType)
            {
                case "ProductCreated":
                    title = "Product Created";
                    message = $"Your product with ID {productId} has been successfully created.";
                    break;
                case "ProductUpdated":
                    title = "Product Updated";
                    message = $"Your product with ID {productId} has been updated.";
                    break;
                case "StockChanged":
                    title = "Product Stock Changed";
                    message = $"The stock for your product with ID {productId} has been updated.";
                    break;
                default:
                    throw new ArgumentException("Invalid event type");
            }

            // Create the notification for the seller
            await CreateNotificationAsync(sellerId, "Seller", title, message);
        }
    }
}
