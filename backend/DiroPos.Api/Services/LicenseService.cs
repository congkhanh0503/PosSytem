using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DiroPos.Api.Data;
using DiroPos.Api.Dtos;
using DiroPos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DiroPos.Api.Services;

public interface ILicenseService
{
    Task<LicenseStatusDto> GetStatusAsync();
    Task<(bool Success, string Message, LicenseStatusDto Status)> ActivateKeyAsync(string licenseKey);
    Task<(bool Success, string Message, LicenseStatusDto Status)> SyncWithServerAsync();
    Task<bool> IsLicenseValidAsync();
    string GenerateKey(string shopCode, string shopName, string plan, DateTime expiresAt, string? hardwareId = null);
    string GetHardwareId();
    Task ClearLicenseForTestAsync();
    Task<(bool Success, string Message, LicenseStatusDto Status)> InitializeShopAsync(InitShopRequestDto dto);
    Task<(bool Success, string Message, LicenseStatusDto Status)> UpdateShopProfileAsync(UpdateShopProfileDto dto);
}

public class LicenseService : ILicenseService
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    // Chìa khóa bí mật chỉ Admin nắm giữ (dùng để ký và xác thực bản quyền)
    private const string MASTER_SECRET = "DiroPos_Master_Secret_Key_@2026_Secure_License_System!";

    public LicenseService(AppDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    public string GetHardwareId()
    {
        try
        {
            string machineName = Environment.MachineName;
            string osVersion = Environment.OSVersion.ToString();
            int cpuCores = Environment.ProcessorCount;
            string raw = $"{machineName}-{osVersion}-{cpuCores}";
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            string hex = Convert.ToHexString(hash);
            return $"HW-{hex[..4]}-{hex[4..8]}-{hex[8..12]}";
        }
        catch
        {
            return "HW-DEFAULT-NODE-01";
        }
    }

    public string GenerateKey(string shopCode, string shopName, string plan, DateTime expiresAt, string? hardwareId = null)
    {
        var payload = new LicensePayload
        {
            ShopCode = shopCode,
            ShopName = shopName,
            PlanType = plan,
            ExpiresAt = expiresAt,
            HardwareId = hardwareId ?? string.Empty,
            IssuedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        string json = JsonSerializer.Serialize(payload);
        string payloadB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(MASTER_SECRET));
        byte[] sigBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadB64));
        string sigB64 = Convert.ToBase64String(sigBytes)
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        return $"{payloadB64}.{sigB64}";
    }

    public bool VerifyKey(string licenseKey, out LicensePayload? payload)
    {
        payload = null;
        if (string.IsNullOrWhiteSpace(licenseKey)) return false;

        string[] parts = licenseKey.Trim().Split('.');
        if (parts.Length != 2) return false;

        string payloadB64 = parts[0];
        string sigB64 = parts[1];

        // 1. Kiểm tra chữ ký HMAC-SHA256
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(MASTER_SECRET));
        byte[] expectedSig = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadB64));
        string expectedSigB64 = Convert.ToBase64String(expectedSig)
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        if (!CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(sigB64), 
            Encoding.UTF8.GetBytes(expectedSigB64)))
        {
            return false;
        }

        // 2. Giải mã Payload
        try
        {
            string padded = payloadB64.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            byte[] jsonBytes = Convert.FromBase64String(padded);
            string json = Encoding.UTF8.GetString(jsonBytes);
            payload = JsonSerializer.Deserialize<LicensePayload>(json);
            return payload != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<LicenseStatusDto> GetStatusAsync()
    {
        var license = await _context.SystemLicenses.FirstOrDefaultAsync();
        string currentHw = GetHardwareId();
        var now = DateTime.UtcNow.AddHours(7);

        // Nếu chưa có bản ghi license nào, thử đồng bộ từ DiroAdmin trước
        if (license == null)
        {
            var expires = now.AddDays(30);
            string trialKey = GenerateKey("DIRO-TRIAL", "Quán Dùng Thử", "Trial", expires, currentHw);

            license = new SystemLicense
            {
                Id = 1,
                ShopCode = "DIRO-TRIAL",
                ShopName = "Quán Dùng Thử",
                LicenseKey = trialKey,
                PlanType = "Trial",
                ActivatedAt = now,
                ExpiresAt = expires,
                Status = "Active",
                HardwareId = currentHw,
                LastCheckedAt = now
            };
            _context.SystemLicenses.Add(license);
            await _context.SaveChangesAsync();

            // Thử đăng ký/đồng bộ máy mới với server DiroAdmin
            _ = Task.Run(async () => {
                try { await SyncWithServerAsync(); } catch { }
            });
        }

        // Kiểm tra tính hợp lệ của LicenseKey hiện có
        bool keyValid = VerifyKey(license.LicenseKey, out var payload);
        bool notExpired = license.ExpiresAt > now;
        bool isActive = string.Equals(license.Status, "Active", StringComparison.OrdinalIgnoreCase);

        // Tự động cấp lại license key hợp lệ nếu thời hạn trong DB còn hợp lệ để tránh chớp thông báo lỗi
        if (!keyValid && notExpired && isActive)
        {
            license.LicenseKey = GenerateKey(license.ShopCode, license.ShopName, license.PlanType, license.ExpiresAt, currentHw);
            await _context.SaveChangesAsync();
            keyValid = true;
        }

        bool isValid = keyValid && notExpired && isActive;

        // Nếu máy đang bị khóa hoặc hết hạn, thử đồng bộ nhanh với server xem Admin vừa gia hạn chưa
        if (!isValid)
        {
            try
            {
                var (synced, _, newStatus) = await SyncWithServerAsync();
                if (synced && newStatus.IsValid)
                {
                    return newStatus;
                }
            }
            catch { }
        }

        int daysRemaining = Math.Max(0, (int)(license.ExpiresAt.Date - now.Date).TotalDays);

        string status = license.Status;
        string msg = "Bản quyền đang hoạt động bình thường.";

        if (!isActive)
        {
            status = "Suspended";
            msg = "Bản quyền tiệm đã bị tạm khóa hoặc ngừng sử dụng bởi quản trị viên.";
        }
        else if (!notExpired)
        {
            status = "Expired";
            msg = "Bản quyền phần mềm DiroPos đã hết hạn sử dụng.";
        }
        else if (!keyValid)
        {
            status = "Invalid";
            msg = "Mã bản quyền không hợp lệ hoặc đã bị chỉnh sửa.";
        }
        else if (daysRemaining <= 5)
        {
            msg = $"Bản quyền sắp hết hạn sau {daysRemaining} ngày nữa.";
        }

        return new LicenseStatusDto
        {
            IsValid = isValid,
            ShopCode = license.ShopCode,
            ShopName = license.ShopName,
            PlanType = license.PlanType,
            Status = status,
            ActivatedAt = license.ActivatedAt,
            ExpiresAt = license.ExpiresAt,
            DaysRemaining = daysRemaining,
            HardwareId = currentHw,
            Message = msg,
            SupportHotline = "0987.654.321",
            IsInitialized = license.IsInitialized,
            BusinessModel = license.BusinessModel,
            ContactPhone = license.ContactPhone,
            OwnerName = license.OwnerName,
            Address = license.Address
        };
    }

    private const string SUPABASE_URL = "https://lacdvcuztjafnwpitntk.supabase.co";
    private const string SUPABASE_KEY = "sb_publishable_myN8hP6gC-sj8jhX9OAIrQ_Q1_F94So";

    public async Task<(bool Success, string Message, LicenseStatusDto Status)> SyncWithServerAsync()
    {
        var license = await _context.SystemLicenses.FirstOrDefaultAsync();
        string currentHw = GetHardwareId();

        // 1. Ưu tiên số 1: Đồng bộ trực tiếp lên Cloud Supabase (0 VNĐ, không cần VPS)
        try
        {
            var (supabaseOk, supabaseMsg, supabaseStatus) = await SyncWithSupabaseAsync(license, currentHw);
            if (supabaseOk)
            {
                return (true, supabaseMsg, supabaseStatus);
            }
        }
        catch { }

        // 2. Fallback nếu không có internet hoặc Supabase lỗi: Giữ nguyên trạng thái local
        var curStatus = await BuildStatusFallbackAsync(license, currentHw);
        return (false, "Không thể kết nối đến máy chủ Cloud Supabase để đồng bộ.", curStatus);
    }

    private async Task<(bool Success, string Message, LicenseStatusDto Status)> SyncWithSupabaseAsync(SystemLicense? license, string currentHw)
    {
        string supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? SUPABASE_URL;
        string supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY") ?? SUPABASE_KEY;

        using var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(5);
        client.DefaultRequestHeaders.Add("apikey", supabaseKey);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

        // 1. Tìm bản ghi theo hardware_id trên Supabase (chỉ tìm bản ghi chưa bị xóa)
        string queryUrl = $"{supabaseUrl}/rest/v1/customers?hardware_id=eq.{Uri.EscapeDataString(currentHw)}&status=neq.Deleted&select=*";
        var res = await client.GetAsync(queryUrl);
        if (!res.IsSuccessStatusCode)
        {
            return (false, "Lỗi kết nối Supabase", await BuildStatusFallbackAsync(license, currentHw));
        }

        var json = await res.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<SupabaseCustomerDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        SupabaseCustomerDto? record = list?.FirstOrDefault();

        // 2. Nếu chưa có theo hardware_id, thử tìm theo shop_code
        if (record == null && license != null && !string.IsNullOrEmpty(license.ShopCode) && license.ShopCode != "DIRO-TRIAL")
        {
            string queryShop = $"{supabaseUrl}/rest/v1/customers?shop_code=eq.{Uri.EscapeDataString(license.ShopCode)}&select=*";
            var resShop = await client.GetAsync(queryShop);
            if (resShop.IsSuccessStatusCode)
            {
                var listShop = JsonSerializer.Deserialize<List<SupabaseCustomerDto>>(await resShop.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                record = listShop?.FirstOrDefault();
            }
        }

        var now = DateTime.UtcNow.AddHours(7);

        // 3. Nếu là máy mới hoàn toàn chưa từng có trên Supabase
        if (record == null)
        {
            // Nếu quán này trước đó ĐÃ ĐƯỢC KHỞI TẠO nhưng trên Supabase không còn
            // => Có nghĩa là Quản trị viên (Admin) đã XÓA QUÁN hoặc hủy cấp phép!
            if (license != null && (license.IsInitialized || (!string.IsNullOrWhiteSpace(license.ShopCode) && license.ShopCode != "DIRO-TRIAL")))
            {
                license.Status = "Suspended";
                license.ExpiresAt = now.AddMinutes(-10);
                license.LastCheckedAt = now;
                await _context.SaveChangesAsync();

                return (false, "Quán này đã bị gỡ bỏ hoặc ngừng sử dụng trên hệ thống DiroAdmin.", new LicenseStatusDto
                {
                    IsValid = false,
                    ShopCode = license.ShopCode,
                    ShopName = license.ShopName,
                    PlanType = license.PlanType,
                    Status = "Suspended",
                    ActivatedAt = license.ActivatedAt,
                    ExpiresAt = license.ExpiresAt,
                    DaysRemaining = 0,
                    HardwareId = currentHw,
                    Message = "Quán của bạn đã bị xóa hoặc ngưng cung cấp trên hệ thống DiroAdmin. Vui lòng liên hệ Admin nếu cần gia hạn/kích hoạt lại.",
                    SupportHotline = "0987.654.321",
                    IsInitialized = true,
                    BusinessModel = license.BusinessModel,
                    ContactPhone = license.ContactPhone,
                    OwnerName = license.OwnerName,
                    Address = license.Address
                });
            }

            // Nếu máy mới tinh chưa cấu hình thông tin tiệm, không tự tạo rác trên Supabase
            // Khách sẽ điền thông tin thật qua màn hình khởi tạo InitialSetupModal
            return (true, "Máy chưa thiết lập thông tin tiệm.", await BuildStatusFallbackAsync(license, currentHw));
        }
        else if (string.Equals(record.Status, "Deleted", StringComparison.OrdinalIgnoreCase))
        {
            // Quán đã bị Admin đưa vào danh sách xóa (Deleted)
            if (license != null)
            {
                license.Status = "Suspended";
                license.ExpiresAt = now.AddMinutes(-10);
                license.LastCheckedAt = now;
                await _context.SaveChangesAsync();
            }

            return (false, "Quán này đã bị gỡ bỏ hoặc ngừng sử dụng trên hệ thống DiroAdmin.", new LicenseStatusDto
            {
                IsValid = false,
                ShopCode = record.ShopCode,
                ShopName = record.ShopName,
                Status = "Suspended",
                HardwareId = currentHw,
                Message = "Quán của bạn đã bị xóa khỏi hệ thống DiroAdmin. Vui lòng liên hệ Admin nếu cần hỗ trợ.",
                SupportHotline = "0987.654.321"
            });
        }
        else
        {
            // Cập nhật last_ping_at, hardware_id và thông tin tiệm lên Supabase
            var patchBody = new Dictionary<string, object?>
            {
                ["last_ping_at"] = DateTime.UtcNow,
                ["hardware_id"] = currentHw
            };
            if (license != null)
            {
                if (!string.IsNullOrWhiteSpace(license.ShopName) && license.ShopName != "DiroPos Store")
                {
                    patchBody["shop_name"] = license.ShopName;
                }
                if (!string.IsNullOrWhiteSpace(license.ContactPhone))
                {
                    patchBody["phone"] = license.ContactPhone;
                }
                if (!string.IsNullOrWhiteSpace(license.BusinessModel))
                {
                    patchBody["business_model"] = license.BusinessModel;
                }
                if (!string.IsNullOrWhiteSpace(license.OwnerName))
                {
                    patchBody["owner_name"] = license.OwnerName;
                }
                if (!string.IsNullOrWhiteSpace(license.Address))
                {
                    patchBody["address"] = license.Address;
                }
            }

            var patchContent = new StringContent(JsonSerializer.Serialize(patchBody), Encoding.UTF8, "application/json");
            try
            {
                await client.PatchAsync($"{supabaseUrl}/rest/v1/customers?id=eq.{record.Id}", patchContent);
            }
            catch { }
        }

        if (record != null)
        {
            if (license == null)
            {
                license = new SystemLicense { Id = 1, HardwareId = currentHw };
                _context.SystemLicenses.Add(license);
            }

            license.ShopCode = record.ShopCode;
            if (!string.IsNullOrWhiteSpace(record.ShopName) && record.ShopName != "DiroPos Quán Mới")
            {
                license.ShopName = record.ShopName;
            }
            if (!string.IsNullOrWhiteSpace(record.Phone))
            {
                license.ContactPhone = record.Phone;
            }
            if (!string.IsNullOrWhiteSpace(record.BusinessModel))
            {
                license.BusinessModel = record.BusinessModel;
            }
            if (!string.IsNullOrWhiteSpace(record.OwnerName))
            {
                license.OwnerName = record.OwnerName;
            }
            if (!string.IsNullOrWhiteSpace(record.Address))
            {
                license.Address = record.Address;
            }

            license.PlanType = record.CurrentPlan;
            license.Status = record.Status;
            license.ExpiresAt = record.ExpiresAt;
            license.LastCheckedAt = now;
            // Luôn đồng bộ mã LicenseKey cục bộ khớp với thông tin hạn từ Supabase
            license.LicenseKey = GenerateKey(license.ShopCode, license.ShopName, license.PlanType, license.ExpiresAt, currentHw);
            await _context.SaveChangesAsync();

            bool notExpired = license.ExpiresAt > now;
            bool isActive = string.Equals(license.Status, "Active", StringComparison.OrdinalIgnoreCase);
            bool isValid = notExpired && isActive;
            int daysRemaining = Math.Max(0, (int)(license.ExpiresAt.Date - now.Date).TotalDays);

            var st = new LicenseStatusDto
            {
                IsValid = isValid,
                ShopCode = license.ShopCode,
                ShopName = license.ShopName,
                PlanType = license.PlanType,
                Status = license.Status,
                ActivatedAt = license.ActivatedAt,
                ExpiresAt = license.ExpiresAt,
                DaysRemaining = daysRemaining,
                HardwareId = currentHw,
                Message = isValid 
                    ? $"Bản quyền Cloud Supabase hợp lệ (Gói: {license.PlanType}, hạn đến {license.ExpiresAt:dd/MM/yyyy})."
                    : (string.Equals(license.Status, "Suspended", StringComparison.OrdinalIgnoreCase) 
                        ? "Bản quyền quán đã bị khóa từ xa bởi Admin." 
                        : "Bản quyền phần mềm đã hết hạn sử dụng."),
                SupportHotline = "0987.654.321",
                IsInitialized = license.IsInitialized,
                BusinessModel = license.BusinessModel,
                ContactPhone = license.ContactPhone,
                OwnerName = license.OwnerName,
                Address = license.Address
            };

            return (true, $"Đồng bộ Supabase Cloud thành công: {st.Message}", st);
        }

        return (false, "Không tìm thấy dữ liệu trên Supabase", await BuildStatusFallbackAsync(license, currentHw));
    }

    private Task<LicenseStatusDto> BuildStatusFallbackAsync(SystemLicense? license, string currentHw)
    {
        var now = DateTime.UtcNow.AddHours(7);
        if (license == null)
        {
            return Task.FromResult(new LicenseStatusDto
            {
                IsValid = false,
                ShopCode = "UNREGISTERED",
                ShopName = "Chưa kích hoạt",
                PlanType = "None",
                Status = "Expired",
                HardwareId = currentHw,
                Message = "Chưa có bản quyền.",
                SupportHotline = "0987.654.321"
            });
        }

        bool notExpired = license.ExpiresAt > now;
        bool isActive = license.Status == "Active";
        bool isValid = notExpired && isActive;
        int daysRemaining = Math.Max(0, (int)(license.ExpiresAt.Date - now.Date).TotalDays);

        return Task.FromResult(new LicenseStatusDto
        {
            IsValid = isValid,
            ShopCode = license.ShopCode,
            ShopName = license.ShopName,
            PlanType = license.PlanType,
            Status = license.Status,
            ActivatedAt = license.ActivatedAt,
            ExpiresAt = license.ExpiresAt,
            DaysRemaining = daysRemaining,
            HardwareId = currentHw,
            Message = isValid ? "Bản quyền hợp lệ." : "Bản quyền đã hết hạn hoặc bị tạm khóa.",
            SupportHotline = "0987.654.321"
        });
    }

    public async Task<bool> IsLicenseValidAsync()
    {
        var status = await GetStatusAsync();
        return status.IsValid;
    }

    public async Task<(bool Success, string Message, LicenseStatusDto Status)> ActivateKeyAsync(string licenseKey)
    {
        string currentHw = GetHardwareId();

        if (!VerifyKey(licenseKey, out var payload) || payload == null)
        {
            var curStatus = await GetStatusAsync();
            return (false, "Mã bản quyền không hợp lệ hoặc chữ ký bảo mật sai.", curStatus);
        }

        // Kiểm tra hardwareId nếu key có ràng buộc
        if (!string.IsNullOrEmpty(payload.HardwareId) && 
            !string.Equals(payload.HardwareId, currentHw, StringComparison.OrdinalIgnoreCase))
        {
            var curStatus = await GetStatusAsync();
            return (false, $"Khóa bản quyền này chỉ dành cho máy {payload.HardwareId}, không khớp với máy hiện tại ({currentHw}).", curStatus);
        }

        var now = DateTime.UtcNow.AddHours(7);
        if (payload.ExpiresAt <= now)
        {
            var curStatus = await GetStatusAsync();
            return (false, "Khóa bản quyền này đã hết hạn sử dụng.", curStatus);
        }

        var license = await _context.SystemLicenses.FirstOrDefaultAsync();
        if (license == null)
        {
            license = new SystemLicense { Id = 1 };
            _context.SystemLicenses.Add(license);
        }

        license.ShopCode = payload.ShopCode;
        license.ShopName = payload.ShopName;
        license.LicenseKey = licenseKey.Trim();
        license.PlanType = payload.PlanType;
        license.ActivatedAt = now;
        license.ExpiresAt = payload.ExpiresAt;
        license.Status = "Active";
        license.HardwareId = currentHw;
        license.LastCheckedAt = now;

        await _context.SaveChangesAsync();

        var newStatus = await GetStatusAsync();
        return (true, $"Kích hoạt thành công gói {payload.PlanType}! Hạn dùng đến {payload.ExpiresAt:dd/MM/yyyy}.", newStatus);
    }

    public async Task ClearLicenseForTestAsync()
    {
        var license = await _context.SystemLicenses.FirstOrDefaultAsync();
        if (license == null)
        {
            license = new SystemLicense { Id = 1 };
            _context.SystemLicenses.Add(license);
        }

        license.LicenseKey = "EXPIRED_TEST_KEY_DELETED";
        license.Status = "Expired";
        license.ExpiresAt = DateTime.UtcNow.AddDays(-2); // Đã hết hạn 2 ngày trước
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string Message, LicenseStatusDto Status)> InitializeShopAsync(InitShopRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ShopName))
        {
            return (false, "Vui lòng nhập tên tiệm / quán.", await GetStatusAsync());
        }
        if (string.IsNullOrWhiteSpace(dto.Phone))
        {
            return (false, "Vui lòng nhập số điện thoại liên hệ.", await GetStatusAsync());
        }

        string currentHw = GetHardwareId();
        var license = await _context.SystemLicenses.FirstOrDefaultAsync();
        var now = DateTime.UtcNow.AddHours(7);

        string targetShopCode = (license != null && !string.IsNullOrWhiteSpace(license.ShopCode) && license.ShopCode != "DIRO-TRIAL")
            ? license.ShopCode
            : $"DP-{Random.Shared.Next(1000, 9999)}";
            
        // Bảo vệ ngày hết hạn: Nếu máy đã có hạn dùng hợp lệ trong tương lai (ví dụ 6 tháng, 1 năm),
        // GIỮ NGUYÊN ngày đó, TUYỆT ĐỐI KHÔNG ép về 30 ngày!
        var expires = (license != null && license.IsInitialized && license.ExpiresAt > now)
            ? license.ExpiresAt
            : now.AddDays(30);

        if (license == null)
        {
            license = new SystemLicense
            {
                Id = 1,
                ShopCode = targetShopCode,
                ShopName = dto.ShopName.Trim(),
                ContactPhone = dto.Phone.Trim(),
                BusinessModel = dto.BusinessModel?.Trim() ?? "Barber",
                OwnerName = dto.OwnerName?.Trim() ?? "Chủ tiệm",
                Address = dto.Address?.Trim() ?? "",
                IsInitialized = true,
                ActivatedAt = now,
                ExpiresAt = expires,
                PlanType = "Trial",
                Status = "Active",
                HardwareId = currentHw,
                LastCheckedAt = now
            };
            license.LicenseKey = GenerateKey(license.ShopCode, license.ShopName, license.PlanType, license.ExpiresAt, currentHw);
            _context.SystemLicenses.Add(license);
        }
        else
        {
            license.ShopCode = targetShopCode;
            license.ShopName = dto.ShopName.Trim();
            license.ContactPhone = dto.Phone.Trim();
            license.BusinessModel = dto.BusinessModel?.Trim() ?? "Barber";
            license.OwnerName = dto.OwnerName?.Trim() ?? "Chủ tiệm";
            license.Address = dto.Address?.Trim() ?? "";
            license.IsInitialized = true;
            license.HardwareId = currentHw;
            license.Status = "Active";
            license.ExpiresAt = expires;
            license.PlanType = string.IsNullOrWhiteSpace(license.PlanType) ? "Trial" : license.PlanType;
            license.LastCheckedAt = now;
            license.LicenseKey = GenerateKey(license.ShopCode, license.ShopName, license.PlanType, license.ExpiresAt, currentHw);
        }

        // Cập nhật cấu hình ShopSetting hiển thị trên hóa đơn / POS
        var setting = await _context.ShopSettings.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new ShopSetting
            {
                ShopName = dto.ShopName.Trim(),
                Phone = dto.Phone.Trim(),
                Address = dto.Address?.Trim() ?? "Chưa cập nhật địa chỉ"
            };
            _context.ShopSettings.Add(setting);
        }
        else
        {
            setting.ShopName = dto.ShopName.Trim();
            setting.Phone = dto.Phone.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Address))
            {
                setting.Address = dto.Address.Trim();
            }
        }

        await _context.SaveChangesAsync();

        // Đồng bộ ngay lập tức lên Supabase Cloud để DiroAdmin nhận thông tin tiệm thật
        try
        {
            string supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? SUPABASE_URL;
            string supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY") ?? SUPABASE_KEY;

            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.Add("apikey", supabaseKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

            // Tìm xem tiệm này đã có record ACTIVE trên Supabase chưa (bỏ qua record Deleted)
            string queryUrl = $"{supabaseUrl}/rest/v1/customers?hardware_id=eq.{Uri.EscapeDataString(currentHw)}&status=neq.Deleted&select=id";
            var checkRes = await client.GetAsync(queryUrl);
            long? existingId = null;
            if (checkRes.IsSuccessStatusCode)
            {
                var json = await checkRes.Content.ReadAsStringAsync();
                var list = JsonSerializer.Deserialize<List<SupabaseCustomerDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                existingId = list?.FirstOrDefault()?.Id;
            }

            if (!existingId.HasValue && !string.IsNullOrEmpty(license.ShopCode) && license.ShopCode != "DIRO-TRIAL")
            {
                string queryShop = $"{supabaseUrl}/rest/v1/customers?shop_code=eq.{Uri.EscapeDataString(license.ShopCode)}&status=neq.Deleted&select=id";
                var resShop = await client.GetAsync(queryShop);
                if (resShop.IsSuccessStatusCode)
                {
                    var listShop = JsonSerializer.Deserialize<List<SupabaseCustomerDto>>(await resShop.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    existingId = listShop?.FirstOrDefault()?.Id;
                }
            }

            if (existingId.HasValue && existingId.Value > 0)
            {
                var patchBody = new Dictionary<string, object?>
                {
                    ["shop_name"] = dto.ShopName.Trim(),
                    ["shop_code"] = license.ShopCode,
                    ["phone"] = dto.Phone.Trim(),
                    ["business_model"] = dto.BusinessModel ?? "Barber",
                    ["owner_name"] = string.IsNullOrWhiteSpace(dto.OwnerName) ? "Chủ tiệm" : dto.OwnerName.Trim(),
                    ["address"] = dto.Address?.Trim() ?? "",
                    ["status"] = "Active",
                    ["current_plan"] = license.PlanType,
                    ["expires_at"] = license.ExpiresAt,
                    ["last_ping_at"] = DateTime.UtcNow,
                    ["hardware_id"] = currentHw
                };
                var patchContent = new StringContent(JsonSerializer.Serialize(patchBody), Encoding.UTF8, "application/json");
                await client.PatchAsync($"{supabaseUrl}/rest/v1/customers?id=eq.{existingId.Value}", patchContent);
            }
            else
            {
                var newRecord = new
                {
                    shop_code = license.ShopCode,
                    shop_name = dto.ShopName.Trim(),
                    owner_name = string.IsNullOrWhiteSpace(dto.OwnerName) ? "Chủ tiệm" : dto.OwnerName.Trim(),
                    phone = dto.Phone.Trim(),
                    address = dto.Address?.Trim() ?? "",
                    business_model = dto.BusinessModel ?? "Barber",
                    current_plan = license.PlanType,
                    activated_at = license.ActivatedAt,
                    expires_at = license.ExpiresAt,
                    status = "Active",
                    hardware_id = currentHw,
                    last_ping_at = DateTime.UtcNow,
                    notes = "Khách hàng tự thiết lập thông tin ban đầu khi cài đặt DiroPos"
                };
                var postContent = new StringContent(JsonSerializer.Serialize(newRecord), Encoding.UTF8, "application/json");
                await client.PostAsync($"{supabaseUrl}/rest/v1/customers", postContent);
            }
        }
        catch { }

        var status = await GetStatusAsync();
        return (true, "Thiết lập thông tin tiệm thành công!", status);
    }

    public async Task<(bool Success, string Message, LicenseStatusDto Status)> UpdateShopProfileAsync(UpdateShopProfileDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ShopName))
        {
            return (false, "Vui lòng nhập tên tiệm / quán.", await GetStatusAsync());
        }

        string currentHw = GetHardwareId();
        var license = await _context.SystemLicenses.FirstOrDefaultAsync();
        var now = DateTime.UtcNow.AddHours(7);

        if (license != null)
        {
            license.ShopName = dto.ShopName.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Phone)) license.ContactPhone = dto.Phone.Trim();
            if (!string.IsNullOrWhiteSpace(dto.BusinessModel)) license.BusinessModel = dto.BusinessModel.Trim();
            if (!string.IsNullOrWhiteSpace(dto.OwnerName)) license.OwnerName = dto.OwnerName.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Address)) license.Address = dto.Address.Trim();
            license.LastCheckedAt = now;
            // Tái tạo key bảo mật tương ứng với tên mới, nhưng GIỮ NGUYÊN ExpiresAt và PlanType
            license.LicenseKey = GenerateKey(license.ShopCode, license.ShopName, license.PlanType, license.ExpiresAt, currentHw);
            await _context.SaveChangesAsync();

            // Cập nhật lên Supabase Cloud (chỉ cập nhật thông tin tiệm, TUYỆT ĐỐI KHÔNG SỬA expires_at hoặc current_plan)
            try
            {
                string supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? SUPABASE_URL;
                string supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY") ?? SUPABASE_KEY;

                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                client.DefaultRequestHeaders.Add("apikey", supabaseKey);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

                var patchBody = new Dictionary<string, object?>
                {
                    ["shop_name"] = license.ShopName,
                    ["phone"] = license.ContactPhone,
                    ["address"] = license.Address,
                    ["last_ping_at"] = DateTime.UtcNow
                };
                if (!string.IsNullOrWhiteSpace(license.OwnerName)) patchBody["owner_name"] = license.OwnerName;
                if (!string.IsNullOrWhiteSpace(license.BusinessModel)) patchBody["business_model"] = license.BusinessModel;

                var patchContent = new StringContent(JsonSerializer.Serialize(patchBody), Encoding.UTF8, "application/json");

                // Update theo hardware_id
                await client.PatchAsync($"{supabaseUrl}/rest/v1/customers?hardware_id=eq.{Uri.EscapeDataString(currentHw)}&status=neq.Deleted", patchContent);
                // Update theo shop_code
                if (!string.IsNullOrEmpty(license.ShopCode) && license.ShopCode != "DIRO-TRIAL")
                {
                    await client.PatchAsync($"{supabaseUrl}/rest/v1/customers?shop_code=eq.{Uri.EscapeDataString(license.ShopCode)}&status=neq.Deleted", patchContent);
                }
            }
            catch { }
        }

        var status = await GetStatusAsync();
        return (true, "Cập nhật thông tin tiệm thành công!", status);
    }
}

public class RemoteLicenseCheckDto
{
    public bool IsAllowed { get; set; }
    public string? Status { get; set; }
    public string? ShopCode { get; set; }
    public string? ShopName { get; set; }
    public string? PlanType { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? LicenseKey { get; set; }
    public string? Message { get; set; }
}

public class SupabaseCustomerDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("shop_code")]
    public string ShopCode { get; set; } = string.Empty;

    [JsonPropertyName("shop_name")]
    public string ShopName { get; set; } = string.Empty;

    [JsonPropertyName("owner_name")]
    public string? OwnerName { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("business_model")]
    public string? BusinessModel { get; set; }

    [JsonPropertyName("current_plan")]
    public string CurrentPlan { get; set; } = "Trial";

    [JsonPropertyName("activated_at")]
    public DateTime ActivatedAt { get; set; }

    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Active";

    [JsonPropertyName("hardware_id")]
    public string? HardwareId { get; set; }

    [JsonPropertyName("active_license_key")]
    public string? ActiveLicenseKey { get; set; }

    [JsonPropertyName("last_ping_at")]
    public DateTime? LastPingAt { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }
}


