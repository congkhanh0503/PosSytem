using System.Web;
using CongBarber.Api.Data;
using CongBarber.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CongBarber.Api.Services;

public interface IVietQrService
{
    Task<VietQrResponseDto> GenerateQrAsync(decimal amount, string orderCode, string? description = null);
}

public class VietQrService : IVietQrService
{
    private readonly AppDbContext _context;

    public VietQrService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<VietQrResponseDto> GenerateQrAsync(decimal amount, string orderCode, string? description = null)
    {
        var setting = await _context.ShopSettings.FirstOrDefaultAsync() ?? new Models.ShopSetting();

        string bankId = string.IsNullOrWhiteSpace(setting.BankId) ? "MB" : setting.BankId;
        string accountNo = string.IsNullOrWhiteSpace(setting.AccountNo) ? "0987654321" : setting.AccountNo;
        string template = string.IsNullOrWhiteSpace(setting.QrTemplate) ? "compact2" : setting.QrTemplate;
        string accountName = string.IsNullOrWhiteSpace(setting.AccountName) ? "NGUYEN THANH CONG" : setting.AccountName;

        string addInfo = string.IsNullOrWhiteSpace(description) ? orderCode : $"{orderCode} {description}";
        if (addInfo.Length > 25)
        {
            addInfo = addInfo[..25];
        }

        string encodedAccountName = HttpUtility.UrlEncode(accountName);
        string encodedAddInfo = HttpUtility.UrlEncode(addInfo);

        // Chuẩn URL ảnh VietQR.io QuickLink cực kỳ ổn định và hiển thị trực tiếp
        string qrUrl = $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png?amount={(long)amount}&addInfo={encodedAddInfo}&accountName={encodedAccountName}";

        return new VietQrResponseDto
        {
            QrImageUrl = qrUrl,
            BankId = bankId,
            BankName = setting.BankName,
            AccountNo = accountNo,
            AccountName = accountName,
            Amount = amount,
            OrderCode = orderCode,
            Description = addInfo
        };
    }
}
