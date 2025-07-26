namespace ShopAI.DTOs.CustomerDTOs
{

    public class CustomerInsightDTO
    {
        public int TotalCustomers { get; set; }
        public int RepeatCustomers { get; set; }
        public double RepeatCustomerPercentage { get; set; }
        public double AvgRating { get; set; } // Example: 4.7
    }

    public class CustomerWithOrdersDTO
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
