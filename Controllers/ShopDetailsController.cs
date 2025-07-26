using Microsoft.AspNetCore.Mvc;
using ShopAI.DTOs.ShopDTOs;
using ShopAI.Services;

namespace ShopAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShopDetailsController : ControllerBase
    {
        private readonly ShopDetailsService _shopService;

        public ShopDetailsController(ShopDetailsService shopService)
        {
            _shopService = shopService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateShop([FromBody] ShopDetailsCreateDTO dto)
        {
            var result = await _shopService.CreateShopAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShopById(int id)
        {
            var result = await _shopService.GetShopByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("seller/{sellerId}")]
        public async Task<IActionResult> GetShopsBySeller(int sellerId)
        {
            var result = await _shopService.GetShopsBySellerIdAsync(sellerId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateShop([FromBody] ShopDetailsUpdateDTO dto)
        {
            var result = await _shopService.UpdateShopAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteShop(int id)
        {
            var result = await _shopService.DeleteShopAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
