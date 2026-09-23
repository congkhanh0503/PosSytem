using CongBarber.Api.Models;

namespace CongBarber.Api.Dtos;

public class OrderItemRequestDto
{
    public string ItemType { get; set; } = "Service"; // "Service" | "Product"
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}

public class CreateOrderDto
{
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public double DiscountPercent { get; set; } = 0; // % giảm giá: 0, 5, 10, 20...
    public string PaymentMethod { get; set; } = "VietQR"; // "VietQR" | "Cash"
    public string? Note { get; set; }
    public List<OrderItemRequestDto> Items { get; set; } = new();
}

public class VietQrResponseDto
{
    public string QrImageUrl { get; set; } = string.Empty;
    public string BankId { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNo { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class DailySalesDto
{
    public string Date { get; set; } = string.Empty; // "22/09"
    public decimal Revenue { get; set; }
    public int OrdersCount { get; set; }
}

public class TopItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
}

public class ExpenseCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class DashboardSummaryDto
{
    public decimal TodayRevenue { get; set; }
    public int TodayOrdersCount { get; set; }
    public decimal TodayDiscountTotal { get; set; }
    public decimal TodayExpense { get; set; }
    public decimal TodayNetProfit { get; set; }

    public decimal MonthRevenue { get; set; }
    public int MonthOrdersCount { get; set; }
    public decimal MonthExpense { get; set; }
    public decimal MonthNetProfit { get; set; }

    // Thống kê Tháng trước (Last Month) để so sánh & xem chi tiết
    public string SelectedMonthName { get; set; } = string.Empty;
    public string LastMonthName { get; set; } = string.Empty;
    public decimal LastMonthRevenue { get; set; }
    public int LastMonthOrdersCount { get; set; }
    public decimal LastMonthExpense { get; set; }
    public decimal LastMonthNetProfit { get; set; }
    public double RevenueGrowthPercent { get; set; }
    public double NetProfitGrowthPercent { get; set; }

    public decimal ServiceRevenueTotal { get; set; }
    public decimal ProductRevenueTotal { get; set; }

    public List<DailySalesDto> Last7DaysSales { get; set; } = new();
    public List<TopItemDto> TopServices { get; set; } = new();
    public List<TopItemDto> TopProducts { get; set; } = new();
    public List<ExpenseCategoryDto> ExpenseCategories { get; set; } = new();

    // Danh sách doanh thu & lợi nhuận chi tiết từng ngày trong tháng
    public List<DailyReportItemDto> DailyBreakdown { get; set; } = new();
}

public class DailyReportItemDto
{
    public string Date { get; set; } = string.Empty; // "2026-09-23"
    public string DateFormatted { get; set; } = string.Empty; // "23/09/2026"
    public string DayOfWeek { get; set; } = string.Empty; // "Thứ Tư"
    public bool IsToday { get; set; }
    public int OrdersCount { get; set; }
    public decimal ServiceRevenue { get; set; }
    public decimal ProductRevenue { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal Revenue { get; set; }
    public decimal Expense { get; set; }
    public decimal NetProfit { get; set; }
}

public class DayDetailDto
{
    public string Date { get; set; } = string.Empty;
    public string DateFormatted { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal Expense { get; set; }
    public decimal NetProfit { get; set; }
    public decimal VietQrTotal { get; set; }
    public decimal CashTotal { get; set; }
    public List<Order> Orders { get; set; } = new();
    public List<Expense> Expenses { get; set; } = new();
}
