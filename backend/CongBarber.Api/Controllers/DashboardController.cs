using CongBarber.Api.Data;
using CongBarber.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongBarber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(
        [FromQuery] int? month = null, 
        [FromQuery] int? year = null)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;

        int selectedYear = year ?? now.Year;
        int selectedMonth = month ?? now.Month;

        var monthStart = new DateTime(selectedYear, selectedMonth, 1);
        var monthEnd = monthStart.AddMonths(1);

        // Tháng trước đó (Last Month)
        var lastMonthStart = monthStart.AddMonths(-1);
        var lastMonthEnd = monthStart;

        // Chỉ tính các đơn đã hoàn tất (Completed)
        var completedOrdersQuery = _context.Orders
            .Include(o => o.Items)
            .Where(o => o.PaymentStatus == "Completed");

        // 1. Thống kê hôm nay
        var todayOrders = await completedOrdersQuery
            .Where(o => o.CreatedAt >= todayStart)
            .ToListAsync();

        decimal todayRevenue = todayOrders.Sum(o => o.FinalAmount);
        int todayOrdersCount = todayOrders.Count;
        decimal todayDiscountTotal = todayOrders.Sum(o => o.DiscountAmount);

        // 2. Thống kê Tháng này (Selected Month)
        var monthOrders = await completedOrdersQuery
            .Where(o => o.CreatedAt >= monthStart && o.CreatedAt < monthEnd)
            .ToListAsync();

        decimal monthRevenue = monthOrders.Sum(o => o.FinalAmount);
        int monthOrdersCount = monthOrders.Count;

        // 3. Thống kê Tháng trước (Last Month)
        var lastMonthOrders = await completedOrdersQuery
            .Where(o => o.CreatedAt >= lastMonthStart && o.CreatedAt < lastMonthEnd)
            .ToListAsync();

        decimal lastMonthRevenue = lastMonthOrders.Sum(o => o.FinalAmount);
        int lastMonthOrdersCount = lastMonthOrders.Count;

        // 4. Thống kê Chi tiêu (Expenses)
        var todayExpenses = await _context.Expenses
            .Where(e => e.Date >= todayStart)
            .ToListAsync();
        decimal todayExpenseTotal = todayExpenses.Sum(e => e.Amount);
        decimal todayNetProfit = todayRevenue - todayExpenseTotal;

        // Chi tiêu tháng được chọn
        var monthExpenses = await _context.Expenses
            .Where(e => e.Date >= monthStart && e.Date < monthEnd)
            .ToListAsync();
        decimal monthExpenseTotal = monthExpenses.Sum(e => e.Amount);
        decimal monthNetProfit = monthRevenue - monthExpenseTotal;

        // Chi tiêu tháng trước
        var lastMonthExpenses = await _context.Expenses
            .Where(e => e.Date >= lastMonthStart && e.Date < lastMonthEnd)
            .ToListAsync();
        decimal lastMonthExpenseTotal = lastMonthExpenses.Sum(e => e.Amount);
        decimal lastMonthNetProfit = lastMonthRevenue - lastMonthExpenseTotal;

        // Tính % tăng trưởng
        double revenueGrowth = lastMonthRevenue > 0 
            ? Math.Round((double)((monthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100, 1) 
            : 0;

        double netProfitGrowth = lastMonthNetProfit != 0 
            ? Math.Round((double)((monthNetProfit - lastMonthNetProfit) / Math.Abs(lastMonthNetProfit)) * 100, 1) 
            : 0;

        var expenseCategories = monthExpenses
            .GroupBy(e => e.Category)
            .Select(g => new ExpenseCategoryDto
            {
                Category = g.Key,
                TotalAmount = g.Sum(e => e.Amount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

        // 4. Tỷ trọng Dịch vụ vs Sản phẩm trong tháng
        var allMonthItems = monthOrders.SelectMany(o => o.Items).ToList();
        decimal serviceRevenue = allMonthItems.Where(i => i.ItemType == "Service").Sum(i => i.TotalPrice);
        decimal productRevenue = allMonthItems.Where(i => i.ItemType == "Product").Sum(i => i.TotalPrice);

        // 5. Doanh số 7 ngày gần nhất
        var last7Days = new List<DailySalesDto>();
        for (int i = 6; i >= 0; i--)
        {
            var targetDay = todayStart.AddDays(-i);
            var nextDay = targetDay.AddDays(1);

            var dayOrders = await completedOrdersQuery
                .Where(o => o.CreatedAt >= targetDay && o.CreatedAt < nextDay)
                .ToListAsync();

            last7Days.Add(new DailySalesDto
            {
                Date = targetDay.ToString("dd/MM"),
                Revenue = dayOrders.Sum(o => o.FinalAmount),
                OrdersCount = dayOrders.Count
            });
        }

        // 6. Top 5 Dịch vụ được làm nhiều nhất
        var topServices = allMonthItems
            .Where(i => i.ItemType == "Service")
            .GroupBy(i => i.ItemName)
            .Select(g => new TopItemDto
            {
                Name = g.Key,
                Quantity = g.Sum(x => x.Quantity),
                TotalAmount = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(5)
            .ToList();

        // 7. Top 5 Sản phẩm bán chạy nhất
        var topProducts = allMonthItems
            .Where(i => i.ItemType == "Product")
            .GroupBy(i => i.ItemName)
            .Select(g => new TopItemDto
            {
                Name = g.Key,
                Quantity = g.Sum(x => x.Quantity),
                TotalAmount = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(5)
            .ToList();

        return Ok(new DashboardSummaryDto
        {
            TodayRevenue = todayRevenue,
            TodayOrdersCount = todayOrdersCount,
            TodayDiscountTotal = todayDiscountTotal,
            TodayExpense = todayExpenseTotal,
            TodayNetProfit = todayNetProfit,
            SelectedMonthName = $"Tháng {selectedMonth:D2}/{selectedYear}",
            LastMonthName = $"Tháng {lastMonthStart.Month:D2}/{lastMonthStart.Year}",
            MonthRevenue = monthRevenue,
            MonthOrdersCount = monthOrdersCount,
            MonthExpense = monthExpenseTotal,
            MonthNetProfit = monthNetProfit,
            LastMonthRevenue = lastMonthRevenue,
            LastMonthOrdersCount = lastMonthOrdersCount,
            LastMonthExpense = lastMonthExpenseTotal,
            LastMonthNetProfit = lastMonthNetProfit,
            RevenueGrowthPercent = revenueGrowth,
            NetProfitGrowthPercent = netProfitGrowth,
            ServiceRevenueTotal = serviceRevenue,
            ProductRevenueTotal = productRevenue,
            Last7DaysSales = last7Days,
            TopServices = topServices,
            TopProducts = topProducts,
            ExpenseCategories = expenseCategories
        });
    }
}
