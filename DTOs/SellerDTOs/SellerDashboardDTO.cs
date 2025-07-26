namespace ShopAI.DTOs.SellerDTOs
{
    public class SellerDashboardDto
    {
        public int ThisMonthsOrders { get; set; }
        public string OrderTrend { get; set; }

        public decimal TotalRevenue { get; set; }
        public string RevenueTrend { get; set; }

        public int SellerRank { get; set; }
        public string RankTrend { get; set; }

        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

}
