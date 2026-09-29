using System.Text.RegularExpressions;
using System.Web;
using DiroPos.Api.Data;
using DiroPos.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace DiroPos.Api.Services;

public interface IVietQrService
{
    Task<VietQrResponseDto> GenerateQrAsync(decimal amount, string orderCode, string? description = null);
}

public class VietQrService : IVietQrService
{
    private readonly AppDbContext _context;

    private static readonly Dictionary<string, string> BankBinMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "MB", "970422" },
        { "MBBANK", "970422" },
        { "VCB", "970436" },
        { "VIETCOMBANK", "970436" },
        { "TCB", "970407" },
        { "TECHCOMBANK", "970407" },
        { "VPB", "970432" },
        { "VPBANK", "970432" },
        { "TPB", "970423" },
        { "TPBANK", "970423" },
        { "ACB", "970416" },
        { "BIDV", "970418" },
        { "VIB", "970441" },
        { "CTG", "970415" },
        { "ICB", "970415" },
        { "VIETINBANK", "970415" },
        { "STB", "970403" },
        { "SACOMBANK", "970403" },
        { "MSB", "970426" },
        { "OCB", "970448" },
        { "SHB", "970443" },
        { "HDB", "970437" },
        { "HDBANK", "970437" },
        { "LPB", "970449" },
        { "LPBANK", "970449" },
        { "VBA", "970405" },
        { "AGRIBANK", "970405" },
        { "SCB", "970429" },
        { "SEAB", "970440" },
        { "SEABANK", "970440" },
        { "BVB", "970454" },
        { "BVBANK", "970454" },
        { "KLB", "970452" },
        { "KIENLONGBANK", "970452" },
        { "NAB", "970428" },
        { "NAMABANK", "970428" },
        { "PGB", "970430" },
        { "PGBANK", "970430" },
        { "PVB", "970412" },
        { "PVCOMBANK", "970412" },
        { "BAB", "970409" },
        { "BACABANK", "970409" },
        { "BAOVIETBANK", "970438" },
        { "SAIGONBANK", "970400" },
        { "SGB", "970400" }
    };

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

        // Chuẩn URL ảnh VietQR.io QuickLink hiển thị trực tiếp
        string qrUrl = $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png?amount={(long)amount}&addInfo={encodedAddInfo}&accountName={encodedAccountName}";

        // Tra cứu mã BIN ngân hàng
        string bankBin = BankBinMap.TryGetValue(bankId, out var bin) ? bin : (Regex.IsMatch(bankId, "^\\d+$") ? bankId : "970422");

        // Sinh chuỗi payload EMVCo chuẩn NAPAS 247 để quét ngoại tuyến (Offline)
        string qrData = BuildEmvCoPayload(bankBin, accountNo, amount, addInfo);

        return new VietQrResponseDto
        {
            QrImageUrl = qrUrl,
            QrData = qrData,
            BankBin = bankBin,
            BankId = bankId,
            BankName = setting.BankName,
            AccountNo = accountNo,
            AccountName = accountName,
            Amount = amount,
            OrderCode = orderCode,
            Description = addInfo
        };
    }

    private static string FormatTlv(string tag, string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return $"{tag}{value.Length:D2}{value}";
    }

    private static string BuildEmvCoPayload(string bin, string accountNo, decimal amount, string? addInfo)
    {
        // 1. Tag 38: Thông tin người thụ hưởng NAPAS 247
        string cleanAccount = Regex.Replace(accountNo ?? "", "[^a-zA-Z0-9]", "");
        string subBeneficiary = FormatTlv("01", FormatTlv("00", bin) + FormatTlv("01", cleanAccount));
        string subService = FormatTlv("02", "QRIBFTTA");
        string tag38 = FormatTlv("38", FormatTlv("00", "A000000727") + subBeneficiary + subService);

        // 2. Các trường dữ liệu chính
        string payload = FormatTlv("00", "01")
                       + FormatTlv("01", amount > 0 ? "12" : "11")
                       + tag38
                       + FormatTlv("53", "704");

        if (amount > 0)
        {
            payload += FormatTlv("54", ((long)Math.Round(amount)).ToString());
        }

        payload += FormatTlv("58", "VN");

        // 3. Tag 62: Nội dung đơn hàng (không dấu, max 25 ký tự)
        if (!string.IsNullOrWhiteSpace(addInfo))
        {
            string cleanInfo = Regex.Replace(addInfo, "[^a-zA-Z0-9 ]", "").Trim();
            if (cleanInfo.Length > 25) cleanInfo = cleanInfo[..25];
            if (!string.IsNullOrEmpty(cleanInfo))
            {
                payload += FormatTlv("62", FormatTlv("08", cleanInfo));
            }
        }

        // 4. Tag 63: Checksum CRC16-CCITT
        payload += "6304";
        string crc = CalculateCrc16(payload);
        return payload + crc;
    }

    private static string CalculateCrc16(string str)
    {
        ushort crc = 0xFFFF;
        foreach (char c in str)
        {
            crc ^= (ushort)((byte)c << 8);
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 0x8000) != 0)
                {
                    crc = (ushort)((crc << 1) ^ 0x1021);
                }
                else
                {
                    crc = (ushort)(crc << 1);
                }
            }
        }
        return crc.ToString("X4");
    }
}
