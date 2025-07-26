using Microsoft.AspNetCore.Mvc;
using ShopAI.Services;
using ShopAI.DTOs;

namespace ShopAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly CheckoutService _checkoutService;

        public CheckoutController(CheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        [HttpPost("ProceedToCheckout/{cartId}")]
        public async Task<ActionResult<ApiResponse<string>>> ProceedToCheckout(int cartId)
        {
            var result = await _checkoutService.ProceedToCheckoutAsync(cartId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
