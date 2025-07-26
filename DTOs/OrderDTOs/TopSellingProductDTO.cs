public class TopSellingProductDTO
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public string ProductImage { get; set; }

    public int SellerId { get; set; }
}