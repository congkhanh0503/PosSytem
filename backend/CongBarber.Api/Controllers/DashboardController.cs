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
        var now = DateTime.UtcNow.AddHours(7);
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

        // 8. Bảng kê doanh thu chi tiết từng ngày trong tháng được chọn
        var dailyBreakdown = new List<DailyReportItemDto>();
        DateTime endDay = (selectedYear == now.Year && selectedMonth == now.Month)
            ? now.Date
            : monthEnd.AddDays(-1).Date;

        for (var d = endDay; d >= monthStart.Date; d = d.AddDays(-1))
        {
            var nextD = d.AddDays(1);
            var dayOrders = monthOrders
                .Where(o => o.CreatedAt >= d && o.CreatedAt < nextD)
                .ToList();

            var dayExpenses = monthExpenses
                .Where(e => e.Date >= d && e.Date < nextD)
                .ToList();

            decimal dayRev = dayOrders.Sum(o => o.FinalAmount);
            decimal dayExp = dayExpenses.Sum(e => e.Amount);
            decimal dayDisc = dayOrders.Sum(o => o.DiscountAmount);

            var dayItems = dayOrders.SelectMany(o => o.Items).ToList();
            decimal daySvc = dayItems.Where(i => i.ItemType == "Service").Sum(i => i.TotalPrice);
            decimal dayProd = dayItems.Where(i => i.ItemType == "Product").Sum(i => i.TotalPrice);

            string dayOfWeekVi = d.DayOfWeek switch
            {
                DayOfWeek.Sunday => "Chủ Nhật",
                DayOfWeek.Monday => "Thứ Hai",
                DayOfWeek.Tuesday => "Thứ Ba",
                DayOfWeek.Wednesday => "Thứ Tư",
                DayOfWeek.Thursday => "Thứ Năm",
                DayOfWeek.Friday => "Thứ Sáu",
                DayOfWeek.Saturday => "Thứ Bảy",
                _ => ""
            };

            dailyBreakdown.Add(new DailyReportItemDto
            {
                Date = d.ToString("yyyy-MM-dd"),
                DateFormatted = d.ToString("dd/MM/yyyy"),
                DayOfWeek = dayOfWeekVi,
                IsToday = (d == now.Date),
                OrdersCount = dayOrders.Count,
                ServiceRevenue = daySvc,
                ProductRevenue = dayProd,
                DiscountTotal = dayDisc,
                Revenue = dayRev,
                Expense = dayExp,
                NetProfit = dayRev - dayExp
            });
        }

        // 9. Biến động chi tiêu qua 6 tháng gần nhất tính đến tháng được chọn
        var trendStart = monthStart.AddMonths(-5);
        var trendEnd = monthEnd;

        var allTrendExpenses = await _context.Expenses
            .Where(e => e.Date >= trendStart && e.Date < trendEnd)
            .ToListAsync();

        var allTrendOrders = await completedOrdersQuery
            .Where(o => o.CreatedAt >= trendStart && o.CreatedAt < trendEnd)
            .ToListAsync();

        var monthlyExpenseTrend = new List<MonthlyExpenseTrendDto>();
        for (int i = 5; i >= 0; i--)
        {
            var mStart = monthStart.AddMonths(-i);
            var mEnd = mStart.AddMonths(1);

            var mExpList = allTrendExpenses.Where(e => e.Date >= mStart && e.Date < mEnd).ToList();
            var mOrdList = allTrendOrders.Where(o => o.CreatedAt >= mStart && o.CreatedAt < mEnd).ToList();

            decimal mExpTotal = mExpList.Sum(e => e.Amount);
            decimal mRevTotal = mOrdList.Sum(o => o.FinalAmount);

            monthlyExpenseTrend.Add(new MonthlyExpenseTrendDto
            {
                MonthKey = mStart.ToString("yyyy-MM"),
                MonthName = $"T{mStart.Month:D2}/{mStart:yy}",
                TotalExpense = mExpTotal,
                ExpensesCount = mExpList.Count,
                TotalRevenue = mRevTotal,
                NetProfit = mRevTotal - mExpTotal
            });
        }

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
            ExpenseCategories = expenseCategories,
            MonthlyExpenseTrend = monthlyExpenseTrend,
            DailyBreakdown = dailyBreakdown
        });
    }

    [HttpGet("day-detail")]
    public async Task<ActionResult<DayDetailDto>> GetDayDetail([FromQuery] string? date = null)
    {
        var now = DateTime.UtcNow.AddHours(7);
        DateTime parsedDate = now.Date;

        if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out DateTime dt))
        {
            parsedDate = dt.Date;
        }

        var start = parsedDate;
        var end = start.AddDays(1);

        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CreatedAt >= start && o.CreatedAt < end && o.PaymentStatus == "Completed")
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var expenses = await _context.Expenses
            .Where(e => e.Date >= start && e.Date < end)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        decimal revenue = orders.Sum(o => o.FinalAmount);
        decimal expense = expenses.Sum(e => e.Amount);
        decimal vietQr = orders.Where(o => o.PaymentMethod == "VietQR").Sum(o => o.FinalAmount);
        decimal cash = orders.Where(o => o.PaymentMethod == "Cash").Sum(o => o.FinalAmount);

        string dayOfWeekVi = start.DayOfWeek switch
        {
            DayOfWeek.Sunday => "Chủ Nhật",
            DayOfWeek.Monday => "Thứ Hai",
            DayOfWeek.Tuesday => "Thứ Ba",
            DayOfWeek.Wednesday => "Thứ Tư",
            DayOfWeek.Thursday => "Thứ Năm",
            DayOfWeek.Friday => "Thứ Sáu",
            DayOfWeek.Saturday => "Thứ Bảy",
            _ => ""
        };

        return Ok(new DayDetailDto
        {
            Date = start.ToString("yyyy-MM-dd"),
            DateFormatted = start.ToString("dd/MM/yyyy"),
            DayOfWeek = dayOfWeekVi,
            OrdersCount = orders.Count,
            Revenue = revenue,
            Expense = expense,
            NetProfit = revenue - expense,
            VietQrTotal = vietQr,
            CashTotal = cash,
            Orders = orders,
            Expenses = expenses
        });
    }
}
