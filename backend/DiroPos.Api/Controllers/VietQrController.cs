using DiroPos.Api.Data;
using DiroPos.Api.Dtos;
using DiroPos.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DiroPos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VietQrController : ControllerBase
{
    private readonly IVietQrService _vietQrService;
    private readonly AppDbContext _context;

    public VietQrController(IVietQrService vietQrService, AppDbContext context)
    {
        _vietQrService = vietQrService;
        _context = context;
    }

    [HttpGet("generate")]
    public async Task<ActionResult<VietQrResponseDto>> Generate(
        [FromQuery] decimal amount = 0,
        [FromQuery] string? orderCode = null,
        [FromQuery] string? description = null)
    {
        string safeOrderCode = string.IsNullOrWhiteSpace(orderCode) ? "DIROPOS" : orderCode;
        var result = await _vietQrService.GenerateQrAsync(amount, safeOrderCode, description);
        return Ok(result);
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<VietQrResponseDto>> GenerateForOrder(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return NotFound("Không tìm thấy đơn hàng");
        var result = await _vietQrService.GenerateQrAsync(order.FinalAmount, order.OrderCode, order.CustomerName);
        return Ok(result);
    }

    [HttpGet("popular-banks")]
    public ActionResult GetPopularBanks()
    {
        var banks = new[]
        {
            new { Id = "MB", Name = "MBBank - Ngân hàng TMCP Quân Đội" },
            new { Id = "VCB", Name = "Vietcombank - Ngân hàng TMCP Ngoại Thương Việt Nam" },
            new { Id = "TCB", Name = "Techcombank - Ngân hàng TMCP Kỹ Thương Việt Nam" },
            new { Id = "VPB", Name = "VPBank - Ngân hàng TMCP Việt Nam Thịnh Vượng" },
            new { Id = "TPB", Name = "TPBank - Ngân hàng TMCP Tiên Phong" },
            new { Id = "ACB", Name = "ACB - Ngân hàng TMCP Á Châu" },
            new { Id = "BIDV", Name = "BIDV - Ngân hàng TMCP Đầu Tư và Phát Triển Việt Nam" },
            new { Id = "VIB", Name = "VIB - Ngân hàng TMCP Quốc Tế Việt Nam" },
            new { Id = "CTG", Name = "VietinBank - Ngân hàng TMCP Công Thương Việt Nam" },
            new { Id = "STB", Name = "Sacombank - Ngân hàng TMCP Sài Gòn Thương Tín" },
            new { Id = "MSB", Name = "MSB - Ngân hàng TMCP Hàng Hải Việt Nam" },
            new { Id = "OCB", Name = "OCB - Ngân hàng TMCP Phương Đông" },
            new { Id = "SHB", Name = "SHB - Ngân hàng TMCP Sài Gòn - Hà Nội" }
        };

        return Ok(banks);
    }
}
