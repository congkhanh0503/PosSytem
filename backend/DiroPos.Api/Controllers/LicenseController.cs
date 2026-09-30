using DiroPos.Api.Dtos;
using DiroPos.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DiroPos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LicenseController : ControllerBase
{
    private readonly ILicenseService _licenseService;

    public LicenseController(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    [HttpGet("status")]
    public async Task<ActionResult<LicenseStatusDto>> GetStatus()
    {
        var status = await _licenseService.GetStatusAsync();
        return Ok(status);
    }

    [HttpPost("activate")]
    public async Task<ActionResult> Activate([FromBody] ActivateLicenseRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.LicenseKey))
        {
            return BadRequest(new { Message = "Vui lòng nhập mã bản quyền (License Key)." });
        }

        var (success, message, status) = await _licenseService.ActivateKeyAsync(request.LicenseKey);
        if (!success)
        {
            return BadRequest(new { Message = message, Status = status });
        }

        return Ok(new { Message = message, Status = status });
    }

    [HttpPost("sync")]
    public async Task<ActionResult> Sync()
    {
        var (success, message, status) = await _licenseService.SyncWithServerAsync();
        return Ok(new { Success = success, Message = message, Status = status });
    }

    [HttpPost("init-shop")]
    public async Task<ActionResult> InitShop([FromBody] InitShopRequestDto request)
    {
        var (success, message, status) = await _licenseService.InitializeShopAsync(request);
        if (!success)
        {
            return BadRequest(new { Success = false, Message = message, Status = status });
        }
        return Ok(new { Success = true, Message = message, Status = status });
    }

    [HttpPost("update-profile")]
    public async Task<ActionResult> UpdateProfile([FromBody] UpdateShopProfileDto request)
    {
        var (success, message, status) = await _licenseService.UpdateShopProfileAsync(request);
        if (!success)
        {
            return BadRequest(new { Success = false, Message = message, Status = status });
        }
        return Ok(new { Success = true, Message = message, Status = status });
    }

    // Endpoint test: Xóa key bản quyền đưa về trạng thái hết hạn
    [HttpPost("test-clear")]
    public async Task<ActionResult> TestClear()
    {
        await _licenseService.ClearLicenseForTestAsync();
        var status = await _licenseService.GetStatusAsync();
        return Ok(new 
        { 
            Message = "Đã xóa key bản quyền và đặt trạng thái hết hạn thành công!", 
            Status = status 
        });
    }

    // Endpoint tiện ích cho Admin tạo nhanh key bàn giao khách hàng
    [HttpPost("generate-key")]
    public ActionResult GenerateKey([FromBody] GenerateKeyRequest request)
    {
        // Yêu cầu Master PIN của bạn để tránh nhân viên tiệm tự sinh key
        if (request.MasterPin != "88889999")
        {
            return Unauthorized(new { Message = "Mã xác thực Admin không hợp lệ." });
        }

        DateTime expires = request.Months > 0 
            ? DateTime.UtcNow.AddHours(7).AddMonths(request.Months)
            : DateTime.UtcNow.AddHours(7).AddYears(1);

        string key = _licenseService.GenerateKey(
            request.ShopCode ?? "TIEM-01",
            request.ShopName ?? "DiroPos Shop",
            request.PlanType ?? "Yearly",
            expires,
            request.HardwareId
        );

        return Ok(new
        {
            LicenseKey = key,
            ShopCode = request.ShopCode,
            ShopName = request.ShopName,
            PlanType = request.PlanType,
            ExpiresAt = expires,
            HardwareId = request.HardwareId
        });
    }
}

public class GenerateKeyRequest
{
    public string MasterPin { get; set; } = string.Empty;
    public string? ShopCode { get; set; }
    public string? ShopName { get; set; }
    public string? PlanType { get; set; } // Monthly, Yearly, Lifetime
    public int Months { get; set; } = 12;
    public string? HardwareId { get; set; }
}
