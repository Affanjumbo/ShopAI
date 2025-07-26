using Microsoft.AspNetCore.Mvc;
using ShopAI.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ShopAI.Controllers
{
    [ApiController]
    [Route("ai/[controller]")]
    public class AIFeaturesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AIFeaturesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("recommend")]
        public async Task<IActionResult> Recommend([FromQuery] int customer_id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync($"http://localhost:8000/recommend?customer_id={customer_id}", null);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpPost("expense-manager")]
        public async Task<IActionResult> ExpenseManager([FromBody] BudgetRequest payload)
        {
            var client = _httpClientFactory.CreateClient();
            var json = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:8001/expense-manager", json);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpPost("smart-suggestions")]
        public async Task<IActionResult> SmartSuggestions([FromQuery] int product_id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync($"http://localhost:8002/smart-suggestions?product_id={product_id}", null);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}
