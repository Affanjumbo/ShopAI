using Microsoft.AspNetCore.Mvc;
using ShopAI.DTOs.SellerDTOs;
using ShopAI.Services;

namespace ShopAI.Controllers
{
    [Route("api/seller-profile")]
    [ApiController]
    public class SellerProfileController : ControllerBase
    {
        private readonly SellerProfileService _profileService;

        public SellerProfileController(SellerProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("{sellerId}")]
        public async Task<IActionResult> GetProfile(int sellerId)
        {
            var response = await _profileService.GetSellerProfileAsync(sellerId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] SellerProfileUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _profileService.UpdateSellerProfileAsync(dto);
            return StatusCode(response.StatusCode, response);
        }
    }
}