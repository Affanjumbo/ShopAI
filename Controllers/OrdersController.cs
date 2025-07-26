using Microsoft.AspNetCore.Mvc;
using ShopAI.DTOs.OrderDTOs;
using ShopAI.DTOs;
using ShopAI.Services;

namespace ShopAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        // Inject the OrderService.
        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        // Creates a new order.
        // POST: api/Orders/CreateOrder
        [HttpPost("CreateOrder")]
        public async Task<ActionResult<ApiResponse<OrderResponseDTO>>> CreateOrder([FromBody] OrderCreateDTO orderDto)
        {
            var response = await _orderService.CreateOrderAsync(orderDto);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Retrieves an order by its ID.
        // GET: api/Orders/GetOrderById/{id}
        [HttpGet("GetOrderById/{id}")]
        public async Task<ActionResult<ApiResponse<OrderResponseDTO>>> GetOrderById(int id)
        {
            var response = await _orderService.GetOrderByIdAsync(id);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Updates the status of an existing order.
        // PUT: api/Orders/UpdateOrderStatus
        [HttpPost("UpdateOrderStatus/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId)
        {
            var result = await _orderService.UpdateOrderStatusAsync(orderId);
            if (!result)
                return BadRequest("Invalid status transition or order not found.");
            return Ok("Order status updated successfully.");
        }

        [HttpPost("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var result = await _orderService.CancelOrderAsync(orderId);
            if (!result)
                return BadRequest("Order cannot be cancelled.");
            return Ok("Order cancelled successfully.");
        }


        // Retrieves all orders.
        // GET: api/Orders/GetAllOrders
        [HttpGet("GetAllOrders")]
        public async Task<ActionResult<ApiResponse<List<OrderResponseDTO>>>> GetAllOrders()
        {
            var response = await _orderService.GetAllOrdersAsync();
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Retrieves all orders for a specific customer.
        // GET: api/Orders/GetOrdersByCustomer/{customerId}
        [HttpGet("GetOrdersByCustomer/{customerId}")]
        public async Task<ActionResult<ApiResponse<List<OrderResponseDTO>>>> GetOrdersByCustomer(int customerId)
        {
            var response = await _orderService.GetOrdersByCustomerAsync(customerId);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        [HttpGet("GetOrdersBySeller/{sellerId}")]
        public async Task<ActionResult<ApiResponse<List<OrderResponseDTO>>>> GetOrdersBySeller(int sellerId)
        {
            var response = await _orderService.GetOrdersBySellerAsync(sellerId);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        [HttpGet("GetTopSellingProducts")]
        public async Task<ActionResult<ApiResponse<List<TopSellingProductDTO>>>> GetTopSellingProducts()
        {
            var response = await _orderService.GetTopSellingProductsAsync();
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        [HttpGet("GetTopSellingProductsBySeller/{sellerId}")]
        public async Task<ActionResult<ApiResponse<List<TopSellingProductDTO>>>> GetTopSellingProductsBySeller(int sellerId)
        {
            var response = await _orderService.GetTopSellingProductsBySellerAsync(sellerId);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        [HttpGet("GetRevenueStatsBySeller/{sellerId}")]
        public async Task<ActionResult<ApiResponse<SellerRevenueDTO>>> GetRevenueStatsBySeller(int sellerId)
        {
            var response = await _orderService.GetRevenueStatsBySellerAsync(sellerId);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        [HttpGet("sales-report")]
        public async Task<IActionResult> GetSalesReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int sellerId)
        {
            var result = await _orderService.GetSalesReportAsync(startDate, endDate, sellerId);
            if (result.StatusCode == 200)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }



    }
}