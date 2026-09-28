using CongBarber.Api.Data;
using CongBarber.Api.Dtos;
using CongBarber.Api.Models;
using CongBarber.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongBarber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IVietQrService _vietQrService;

    public OrdersController(AppDbContext context, IVietQrService vietQrService)
    {
        _context = context;
        _vietQrService = vietQrService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders(
        [FromQuery] DateTime? date = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? status = null,
        [FromQuery] string? paymentMethod = null,
        [FromQuery] int limit = 200)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .AsQueryable();

        if (fromDate.HasValue && toDate.HasValue)
        {
            var start = fromDate.Value.Date;
            var end = toDate.Value.Date.AddDays(1);
            query = query.Where(o => o.CreatedAt >= start && o.CreatedAt < end);
        }
        else if (fromDate.HasValue)
        {
            var start = fromDate.Value.Date;
            query = query.Where(o => o.CreatedAt >= start);
        }
        else if (toDate.HasValue)
        {
            var end = toDate.Value.Date.AddDays(1);
            query = query.Where(o => o.CreatedAt < end);
        }
        else if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            var nextDate = targetDate.AddDays(1);
            query = query.Where(o => o.CreatedAt >= targetDate && o.CreatedAt < nextDate);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o => o.PaymentStatus.ToLower() == status.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(paymentMethod))
        {
            query = query.Where(o => o.PaymentMethod.ToLower() == paymentMethod.ToLower());
        }

        return await query.OrderByDescending(o => o.CreatedAt).Take(limit).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        return order;
    }

    [HttpPost]
    public async Task<ActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
        {
            return BadRequest("Đơn hàng phải có ít nhất 1 dịch vụ hoặc sản phẩm.");
        }

        // 1. Tạo mã đơn hàng duy nhất trong ngày theo giờ Việt Nam (UTC+7)
        var today = DateTime.UtcNow.AddHours(7);
        var todayStart = today.Date;
        var todayOrdersCount = await _context.Orders.CountAsync(o => o.CreatedAt >= todayStart);
        string orderCode = $"CB-{today:yyMMdd}-{(todayOrdersCount + 1):D3}";

        // 2. Tính tiền và xử lý tồn kho
        decimal subTotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in dto.Items)
        {
            decimal itemTotal = item.UnitPrice * item.Quantity;
            subTotal += itemTotal;

            if (item.ItemType == "Product")
            {
                var product = await _context.Products.FindAsync(item.ItemId);
                if (product != null)
                {
                    product.StockQuantity = Math.Max(0, product.StockQuantity - item.Quantity);
                }
            }

            orderItems.Add(new OrderItem
            {
                ItemType = item.ItemType,
                ServiceId = item.ItemType == "Service" ? item.ItemId : null,
                ProductId = item.ItemType == "Product" ? item.ItemId : null,
                ItemName = item.ItemName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = itemTotal
            });
        }

        // Tính giảm giá theo %
        double discountPercent = Math.Clamp(dto.DiscountPercent, 0, 100);
        decimal discountAmount = Math.Round(subTotal * (decimal)(discountPercent / 100.0), 0);
        decimal finalAmount = Math.Max(0, subTotal - discountAmount);

        var order = new Order
        {
            OrderCode = orderCode,
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            SubTotal = subTotal,
            DiscountPercent = discountPercent,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            PaymentMethod = dto.PaymentMethod,
            PaymentStatus = "Completed",
            Note = dto.Note,
            CreatedAt = today,
            Items = orderItems
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // 3. Nếu là VietQR, sinh thông tin mã QR
        VietQrResponseDto? vietQr = null;
        if (order.PaymentMethod == "VietQR")
        {
            vietQr = await _vietQrService.GenerateQrAsync(order.FinalAmount, order.OrderCode, order.CustomerName);
        }

        return Ok(new
        {
            Order = order,
            VietQr = vietQr
        });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        if (order.PaymentStatus == "Cancelled") return BadRequest("Đơn hàng này đã bị hủy trước đó.");

        order.PaymentStatus = "Cancelled";

        // Hoàn lại kho sản phẩm
        foreach (var item in order.Items.Where(i => i.ItemType == "Product" && i.ProductId.HasValue))
        {
            var product = await _context.Products.FindAsync(item.ProductId!.Value);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
            }
        }

        await _context.SaveChangesAsync();
        return Ok(order);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        // Nếu chuyển từ Completed sang Cancelled -> hoàn lại kho sản phẩm
        if (order.PaymentStatus != "Cancelled" && dto.PaymentStatus == "Cancelled")
        {
            foreach (var item in order.Items.Where(i => i.ItemType == "Product" && i.ProductId.HasValue))
            {
                var product = await _context.Products.FindAsync(item.ProductId!.Value);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                }
            }
        }
        // Nếu chuyển từ Cancelled sang Completed -> trừ lại kho
        else if (order.PaymentStatus == "Cancelled" && dto.PaymentStatus == "Completed")
        {
            foreach (var item in order.Items.Where(i => i.ItemType == "Product" && i.ProductId.HasValue))
            {
                var product = await _context.Products.FindAsync(item.ProductId!.Value);
                if (product != null)
                {
                    product.StockQuantity = Math.Max(0, product.StockQuantity - item.Quantity);
                }
            }
        }

        order.CustomerName = dto.CustomerName;
        order.CustomerPhone = dto.CustomerPhone;
        order.PaymentMethod = dto.PaymentMethod;
        order.PaymentStatus = dto.PaymentStatus;
        order.Note = dto.Note;

        // Tính lại giảm giá và thực thu nếu có thay đổi % giảm giá
        double discountPercent = Math.Clamp(dto.DiscountPercent, 0, 100);
        order.DiscountPercent = discountPercent;
        order.DiscountAmount = Math.Round(order.SubTotal * (decimal)(discountPercent / 100.0), 0);
        order.FinalAmount = Math.Max(0, order.SubTotal - order.DiscountAmount);

        await _context.SaveChangesAsync();
        return Ok(order);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        // Nếu đơn hàng chưa bị hủy mà bị xóa thì hoàn lại kho cho các sản phẩm
        if (order.PaymentStatus != "Cancelled")
        {
            foreach (var item in order.Items.Where(i => i.ItemType == "Product" && i.ProductId.HasValue))
            {
                var product = await _context.Products.FindAsync(item.ProductId!.Value);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                }
            }
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
