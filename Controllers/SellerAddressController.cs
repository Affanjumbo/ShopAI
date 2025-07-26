using Microsoft.AspNetCore.Mvc;
using ShopAI.Services;

namespace ShopAI.Controllers
{
    [Route("api/seller/addresses")]
    [ApiController]
    public class SellerAddressController : ControllerBase
    {
        private readonly SellerAddressService _addressService;

        public SellerAddressController(SellerAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddAddress([FromBody] SellerAddressCreateDTO dto)
        {
            var result = await _addressService.AddSellerAddressAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{sellerId}")]
        public async Task<IActionResult> GetAddresses(int sellerId)
        {
            var result = await _addressService.GetAddressesBySellerIdAsync(sellerId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateAddress([FromBody] SellerAddressUpdateDTO dto)
        {
            var result = await _addressService.UpdateSellerAddressAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var result = await _addressService.DeleteSellerAddressAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
