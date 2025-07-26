namespace ShopAI.DTOs.OrderDTOs
{
    public class SalesReportDTO
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

}
