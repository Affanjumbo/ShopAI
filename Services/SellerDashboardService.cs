using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopAI.Data;
using ShopAI.DTOs.SellerDTOs;
using ShopAI.Models;
using ShopAI.Services;

public class SellerDashboardService
{
    private readonly ApplicationDbContext _context;

    public SellerDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SellerDashboardDto> GetDashboardAsync(int sellerId)
    {
        var currentDate = DateTime.UtcNow;
        var currentMonth = currentDate.Month;
        var currentYear = currentDate.Year;

        var previousDate = currentDate.AddMonths(-1);
        var previousMonth = previousDate.Month;
        var previousYear = previousDate.Year;

        var sellerProductIds = await _context.Products
            .Where(p => p.SellerId == sellerId)
            .Select(p => p.Id)
            .ToListAsync();

        // Filter OrderItems where the order is delivered
        var sellerOrderItems = await _context.OrderItems
            .Where(oi => sellerProductIds.Contains(oi.ProductId) &&
                         oi.Order.OrderStatus == OrderStatus.Delivered)
            .Include(oi => oi.Order)
            .ToListAsync();

        var thisMonthOrders = sellerOrderItems
            .Where(oi => oi.Order.OrderDate.Month == currentMonth && oi.Order.OrderDate.Year == currentYear)
            .Select(oi => oi.OrderId)
            .Distinct()
            .Count();

        var lastMonthOrders = sellerOrderItems
            .Where(oi => oi.Order.OrderDate.Month == previousMonth && oi.Order.OrderDate.Year == previousYear)
            .Select(oi => oi.OrderId)
            .Distinct()
            .Count();

        string orderTrend = GetTrend(thisMonthOrders, lastMonthOrders);

        // Revenue (only from delivered orders)
        var totalRevenue = sellerOrderItems.Sum(oi => oi.Quantity * oi.TotalPrice);

        var lastMonthRevenue = sellerOrderItems
            .Where(oi => oi.Order.OrderDate.Month == previousMonth && oi.Order.OrderDate.Year == previousYear)
            .Sum(oi => oi.Quantity * oi.TotalPrice);

        string revenueTrend = GetTrend((int)totalRevenue, (int)lastMonthRevenue);

        // Seller Rank (based only on delivered orders)
        var sellerRevenuesThisMonth = await _context.Products
            .Where(p => p.SellerId != null)
            .GroupBy(p => p.SellerId)
            .Select(g => new
            {
                SellerId = g.Key,
                Revenue = g
                    .SelectMany(p => _context.OrderItems
                        .Where(oi => oi.ProductId == p.Id &&
                                     oi.Order.OrderStatus == OrderStatus.Delivered &&
                                     oi.Order.OrderDate.Month == currentMonth &&
                                     oi.Order.OrderDate.Year == currentYear))
                    .Sum(oi => oi.Quantity * oi.TotalPrice)
            })
            .OrderByDescending(x => x.Revenue)
            .ToListAsync();

        var currentRank = sellerRevenuesThisMonth.FindIndex(x => x.SellerId == sellerId) + 1;

        // Save rank if not already stored
        var existingRank = await _context.MonthlySellerRanks.FirstOrDefaultAsync(x =>
            x.SellerId == sellerId && x.Month == currentMonth && x.Year == currentYear);

        if (existingRank == null)
        {
            var newRank = new MonthlySellerRank
            {
                SellerId = sellerId,
                Month = currentMonth,
                Year = currentYear,
                Rank = currentRank
            };
            _context.MonthlySellerRanks.Add(newRank);
            await _context.SaveChangesAsync();
        }

        // Rank trend
        var lastMonthRank = await _context.MonthlySellerRanks
            .Where(x => x.SellerId == sellerId && x.Month == previousMonth && x.Year == previousYear)
            .Select(x => x.Rank)
            .FirstOrDefaultAsync();

        string rankTrend = GetRankTrend(currentRank, lastMonthRank);

        // Rating
        var feedbacks = await _context.Feedbacks
            .Where(f => sellerProductIds.Contains(f.ProductId))
            .ToListAsync();

        decimal avgRating = feedbacks.Any() ? feedbacks.Average(f => f.Rating) : 0;
        int totalReviews = feedbacks.Count;

        return new SellerDashboardDto
        {
            ThisMonthsOrders = thisMonthOrders,
            OrderTrend = orderTrend,
            TotalRevenue = totalRevenue,
            RevenueTrend = revenueTrend,
            SellerRank = currentRank,
            RankTrend = rankTrend,
            AverageRating = avgRating,
            TotalReviews = totalReviews
        };
    }

    private string GetTrend(int current, int previous)
    {
        if (previous == 0) return current > 0 ? "+100%" : "0%";
        int change = (int)(((double)(current - previous) / previous) * 100);
        return change >= 0 ? $"+{change}%" : $"{change}%";
    }

    private string GetRankTrend(int current, int previous)
    {
        if (previous == 0) return "N/A";
        int diff = previous - current;
        if (diff > 0)
            return $"+{diff} from last month";
        else if (diff < 0)
            return $"{diff} from last month";
        else
            return "No change";
    }
}

