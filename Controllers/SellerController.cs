using Microsoft.AspNetCore.Mvc;
using ShopAI.DTOs.SellerDTOs;
using ShopAI.Services;
using ShopAI.DTOs;
using ShopAI.DTOs.CustomerDTOs;

namespace ShopAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SellerController : ControllerBase
    {
        private readonly SellerService _sellerService;

        public SellerController(SellerService sellerService)
        {
            _sellerService = sellerService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<SellerResponseDTO>>> Register(SellerRegistrationDTO dto)
        {
            var response = await _sellerService.RegisterSellerAsync(dto);
            return StatusCode(response.StatusCode, response);


        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login(SellerLoginDTO dto)
        {
            var response = await _sellerService.LoginAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SellerResponseDTO>>> GetById(int id)
        {
            var response = await _sellerService.GetSellerByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("update")]
        public async Task<ActionResult<ApiResponse<SellerConfirmationResponseDTO>>> Update(SellerUpdateDTO dto)
        {
            var response = await _sellerService.UpdateSellerAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<SellerConfirmationResponseDTO>>> Delete(int id)
        {
            var response = await _sellerService.DeleteSellerAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<SellerConfirmationResponseDTO>>> ChangePassword(SellerChangePasswordDTO dto)
        {
            var response = await _sellerService.ChangePasswordAsync(dto);
            return StatusCode(response.StatusCode, response);
        }
    }
}
