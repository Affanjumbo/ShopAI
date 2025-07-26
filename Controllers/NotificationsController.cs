using Microsoft.AspNetCore.Mvc;
using ShopAI.Services;

namespace ShopAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // Get all notifications for a specific user
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotifications(string userId)
        {
            var notifications = await _notificationService.GetNotificationsAsync(userId);
            return Ok(notifications);
        }

        // Mark a notification as read
        [HttpPost("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { message = "Notification marked as read." });
        }

        // Create a notification (used for creating notifications manually)
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] NotificationCreateModel model)
        {
            await _notificationService.CreateNotificationAsync(model.UserId, model.Role, model.Title, model.Message);
            return Ok(new { message = "Notification sent." });
        }

        // Notify customer when their order is placed
        [HttpPost("notify-customer-order-placed")]
        public async Task<IActionResult> NotifyCustomerOrderPlaced([FromBody] CustomerOrderNotificationModel model)
        {
            await _notificationService.NotifyCustomerOrderPlacedAsync(model.CustomerId, model.OrderNumber);
            return Ok(new { message = "Notification sent to customer about order placement." });
        }

        // Notify seller when their product is ordered
        [HttpPost("notify-seller-order-placed")]
        public async Task<IActionResult> NotifySellerOrderPlaced([FromBody] SellerOrderNotificationModel model)
        {
            await _notificationService.NotifySellerOrderPlacedAsync(model.SellerId, model.OrderNumber);
            return Ok(new { message = "Notification sent to seller about order placement." });
        }

        // Notify seller about product-related events (e.g., product creation, update, or stock change)
        [HttpPost("notify-seller-product-event")]
        public async Task<IActionResult> NotifySellerProductEvent([FromBody] SellerProductNotificationModel model)
        {
            await _notificationService.NotifySellerProductEventAsync(model.SellerId, model.ProductId, model.EventType);
            return Ok(new { message = "Notification sent to seller about product event." });
        }
    }

    // Models for customer and seller notifications

    public class NotificationCreateModel
    {
        public string UserId { get; set; }
        public string Role { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }

    // Model to notify customer about order placement
    public class CustomerOrderNotificationModel
    {
        public string CustomerId { get; set; }
        public string OrderNumber { get; set; }
    }

    // Model to notify seller about order placement
    public class SellerOrderNotificationModel
    {
        public string SellerId { get; set; }
        public string OrderNumber { get; set; }
    }

    // Model to notify seller about product events
    public class SellerProductNotificationModel
    {
        public string SellerId { get; set; }
        public string ProductId { get; set; }
        public string EventType { get; set; } // e.g., "ProductCreated", "ProductUpdated", "StockChanged"
    }
}
