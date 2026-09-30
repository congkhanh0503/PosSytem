using DiroPos.Api.Data;
using DiroPos.Api.Dtos;
using DiroPos.Api.Models;
using DiroPos.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiroPos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IVietQrService _vietQrService;
    private readonly ILicenseService _licenseService;

    public OrdersController(AppDbContext context, IVietQrService vietQrService, ILicenseService licenseService)
    {
        _context = context;
        _vietQrService = vietQrService;
        _licenseService = licenseService;
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
        // 0. Kiểm tra bản quyền sử dụng phần mềm
        if (!await _licenseService.IsLicenseValidAsync())
        {
            return StatusCode(StatusCodes.Status403Forbidden, 
                "Bản quyền phần mềm DiroPos đã hết hạn hoặc bị tạm khóa. Vui lòng liên hệ Admin để gia hạn tiếp tục bán hàng.");
        }

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
    public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderRequestDto? dto = null)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });
        if (order.PaymentStatus == "Cancelled") return BadRequest(new { message = "Đơn hàng này đã ở trạng thái HỦY trước đó." });

        order.PaymentStatus = "Cancelled";

        string cancelReason = !string.IsNullOrWhiteSpace(dto?.Reason) ? dto.Reason.Trim() : "Khách yêu cầu / nhân viên hủy";
        string timestamp = DateTime.UtcNow.AddHours(7).ToString("HH:mm dd/MM/yyyy");
        order.Note = string.IsNullOrWhiteSpace(order.Note)
            ? $"[ĐÃ HỦY ĐƠN ({timestamp}) - Lý do: {cancelReason}]"
            : $"{order.Note} | [ĐÃ HỦY ({timestamp}) - {cancelReason}]";

        // Hoàn lại kho sản phẩm nếu đơn có bán sản phẩm
        foreach (var item in order.Items.Where(i => i.ItemType == "Product" && i.ProductId.HasValue))
        {
            var product = await _context.Products.FindAsync(item.ProductId!.Value);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Hủy đơn hàng thành công!", order });
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
    public IActionResult DeleteOrder(int id)
    {
        // Chống gian lận tài chính & thất thoát doanh thu: Tuyệt đối không cho phép xóa vật lý đơn hàng khỏi cơ sở dữ liệu
        return BadRequest(new { 
            message = "Hệ thống bảo vệ tài chính DiroPos không cho phép xóa vĩnh viễn đơn hàng để tránh thất thoát và gian lận. Vui lòng sử dụng tính năng 'HỦY ĐƠN HÀNG'." 
        });
    }
}
