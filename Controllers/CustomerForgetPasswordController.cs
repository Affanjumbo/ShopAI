using Microsoft.AspNetCore.Mvc;
using ShopAI.DTOs.CustomerForgetPasswordDTOs;
using ShopAI.Services;
using System.Threading.Tasks;

namespace ShopAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerForgetPasswordController : ControllerBase
    {
        private readonly CustomerForgetPasswordService _forgetPasswordService;

        public CustomerForgetPasswordController(CustomerForgetPasswordService forgetPasswordService)
        {
            _forgetPasswordService = forgetPasswordService;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] ForgetPasswordRequestDto request)
        {
            var result = await _forgetPasswordService.SendOtpAsync(request);
            if (result == "Customer not found")
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationDto request)
        {
            var isValid = await _forgetPasswordService.VerifyOtpAsync(request);
            if (!isValid)
                return BadRequest("Invalid or expired OTP");

            return Ok("OTP verified successfully");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            var result = await _forgetPasswordService.ResetPasswordAsync(request);

            if (result == "Passwords do not match")
                return BadRequest(result);

            if (result == "Invalid or expired OTP" || result == "Customer not found")
                return NotFound(result);

            return Ok(result);
        }
    }
}
