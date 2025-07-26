using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ShopAI.Models
{
    public class RecommendResponse
    {
        [JsonPropertyName("recommended_product_ids")]
        public List<int> RecommendedProductIds { get; set; }
    }
}
