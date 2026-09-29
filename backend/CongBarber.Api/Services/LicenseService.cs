using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CongBarber.Api.Data;
using CongBarber.Api.Dtos;
using CongBarber.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CongBarber.Api.Services;

public interface ILicenseService
{
    Task<LicenseStatusDto> GetStatusAsync();
    Task<(bool Success, string Message, LicenseStatusDto Status)> ActivateKeyAsync(string licenseKey);
    Task<(bool Success, string Message, LicenseStatusDto Status)> SyncWithServerAsync();
    Task<bool> IsLicenseValidAsync();
    string GenerateKey(string shopCode, string shopName, string plan, DateTime expiresAt, string? hardwareId = null);
    string GetHardwareId();
    Task ClearLicenseForTestAsync();
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
        bool isActive = license.Status == "Active";
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

        string status = "Active";
        string msg = "Bản quyền đang hoạt động bình thường.";

        if (!keyValid)
        {
            status = "Invalid";
            msg = "Mã bản quyền không hợp lệ hoặc đã bị chỉnh sửa bất hợp pháp.";
        }
        else if (!notExpired)
        {
            status = "Expired";
            msg = "Bản quyền phần mềm DiroPos đã hết hạn sử dụng.";
        }
        else if (!isActive)
        {
            status = "Suspended";
            msg = "Bản quyền tiệm đã bị tạm khóa bởi quản trị viên.";
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
            SupportHotline = "0987.654.321"
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

        // 1. Tìm bản ghi theo hardware_id trên Supabase
        string queryUrl = $"{supabaseUrl}/rest/v1/customers?hardware_id=eq.{Uri.EscapeDataString(currentHw)}&select=*";
        var res = await client.GetAsync(queryUrl);
        if (!res.IsSuccessStatusCode)
        {
            return (false, "Lỗi kết nối Supabase", await BuildStatusFallbackAsync(license, currentHw));
        }

        var json = await res.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<SupabaseCustomerDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        SupabaseCustomerDto? record = list?.FirstOrDefault();

        // 2. Nếu chưa có theo hardware_id, thử tìm theo shop_code
        if (record == null && license != null && !string.IsNullOrEmpty(license.ShopCode))
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

        // 3. Nếu là máy mới hoàn toàn chưa từng có trên Supabase -> Tự động đăng ký máy mới
        if (record == null)
        {
            string newShopCode = license?.ShopCode ?? $"DP-{Random.Shared.Next(1000, 9999)}";
            string newShopName = license?.ShopName ?? "DiroPos Quán Mới";
            var initExpires = now.AddDays(30);

            var newRecord = new
            {
                shop_code = newShopCode,
                shop_name = newShopName,
                owner_name = "Chủ tiệm",
                phone = "",
                business_model = "Barber",
                current_plan = "Trial",
                activated_at = now,
                expires_at = initExpires,
                status = "Active",
                hardware_id = currentHw,
                last_ping_at = DateTime.UtcNow,
                notes = "Máy POS tự động đăng ký lên Supabase"
            };

            var postContent = new StringContent(JsonSerializer.Serialize(newRecord), Encoding.UTF8, "application/json");
            var postRes = await client.PostAsync($"{supabaseUrl}/rest/v1/customers", postContent);
            if (postRes.IsSuccessStatusCode)
            {
                record = new SupabaseCustomerDto
                {
                    ShopCode = newShopCode,
                    ShopName = newShopName,
                    CurrentPlan = "Trial",
                    ActivatedAt = now,
                    ExpiresAt = initExpires,
                    Status = "Active",
                    HardwareId = currentHw
                };
            }
        }
        else
        {
            // Cập nhật last_ping_at và hardware_id lên Supabase
            var patchBody = new Dictionary<string, object?>
            {
                ["last_ping_at"] = DateTime.UtcNow,
                ["hardware_id"] = currentHw
            };
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
            license.ShopName = record.ShopName;
            license.PlanType = record.CurrentPlan;
            license.Status = record.Status;
            license.ExpiresAt = record.ExpiresAt;
            license.LastCheckedAt = now;
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
                SupportHotline = "0987.654.321"
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
}


