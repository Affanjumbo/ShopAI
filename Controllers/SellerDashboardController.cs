using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ShopAI.Controllers
{
    

    [ApiController]
    [Route("api/seller/dashboard")]
    public class SellerDashboardController : ControllerBase
    {
        private readonly SellerDashboardService _dashboardService;

        public SellerDashboardController(SellerDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("{sellerId}")]
        public async Task<IActionResult> GetDashboard(int sellerId)
        {
            var result = await _dashboardService.GetDashboardAsync(sellerId);
            return Ok(result);
        }
    }

}
