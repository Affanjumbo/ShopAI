using Microsoft.AspNetCore.Mvc;
using ShopAI.DTOs.BankDTOs;
using ShopAI.Services;

namespace ShopAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankDetailsController : ControllerBase
    {
        private readonly BankDetailsService _bankDetailsService;

        public BankDetailsController(BankDetailsService bankDetailsService)
        {
            _bankDetailsService = bankDetailsService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBankDetails([FromBody] BankDetailsCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _bankDetailsService.AddBankDetailsAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBankDetailsById(int id)
        {
            var response = await _bankDetailsService.GetBankDetailsByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("shop/{shopId}")]
        public async Task<IActionResult> GetBankDetailsByShopId(int shopId)
        {
            var response = await _bankDetailsService.GetBankDetailsByShopIdAsync(shopId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateBankDetails([FromBody] BankDetailsUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _bankDetailsService.UpdateBankDetailsAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBankDetails(int id)
        {
            var response = await _bankDetailsService.DeleteBankDetailsAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
