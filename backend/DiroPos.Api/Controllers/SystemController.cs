using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace DiroPos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    public const string CURRENT_VERSION = "1.0.0";
    public const string APP_NAME = "DiroPos PRO";
    public const string BUILD_DATE = "2026-09-30";

    private const string SUPABASE_URL = "https://lacdvcuztjafnwpitntk.supabase.co";
    private const string SUPABASE_KEY = "sb_publishable_myN8hP6gC-sj8jhX9OAIrQ_Q1_F94So";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SystemController> _logger;

    public SystemController(IHttpClientFactory httpClientFactory, ILogger<SystemController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Lấy thông tin phiên bản hiện tại của máy POS
    /// </summary>
    [HttpGet("version")]
    public IActionResult GetVersion()
    {
        return Ok(new
        {
            appName = APP_NAME,
            version = CURRENT_VERSION,
            buildDate = BUILD_DATE,
            edition = "PRO Commercial",
            os = Environment.OSVersion.ToString(),
            machine = Environment.MachineName
        });
    }

    /// <summary>
    /// Tự động kiểm tra bản cập nhật mới trên Cloud Supabase
    /// </summary>
    [HttpGet("check-update")]
    public async Task<IActionResult> CheckUpdate()
    {
        try
        {
            string supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? SUPABASE_URL;
            string supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY") ?? SUPABASE_KEY;

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("apikey", supabaseKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

            // Lấy phiên bản mới nhất từ bảng app_versions
            string queryUrl = $"{supabaseUrl}/rest/v1/app_versions?order=release_date.desc&limit=1&select=*";
            var response = await client.GetAsync(queryUrl);

            if (!response.IsSuccessStatusCode)
            {
                // Bảng chưa tạo hoặc lỗi mạng: Giữ an toàn, coi như chưa có bản mới
                return Ok(new
                {
                    hasUpdate = false,
                    currentVersion = CURRENT_VERSION,
                    latestVersion = CURRENT_VERSION,
                    message = "Đang chạy phiên bản mới nhất."
                });
            }

            string json = await response.Content.ReadAsStringAsync();
            var versions = JsonSerializer.Deserialize<List<SupabaseAppVersionDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var latest = versions?.FirstOrDefault();
            if (latest == null || string.IsNullOrWhiteSpace(latest.Version))
            {
                return Ok(new
                {
                    hasUpdate = false,
                    currentVersion = CURRENT_VERSION,
                    latestVersion = CURRENT_VERSION,
                    message = "Đang chạy phiên bản mới nhất."
                });
            }

            bool hasNewer = CompareVersions(latest.Version, CURRENT_VERSION) > 0;
            bool isMandatory = latest.IsMandatory;

            if (!string.IsNullOrWhiteSpace(latest.MinVersion) && CompareVersions(latest.MinVersion, CURRENT_VERSION) > 0)
            {
                isMandatory = true;
            }

            return Ok(new
            {
                hasUpdate = hasNewer,
                currentVersion = CURRENT_VERSION,
                latestVersion = latest.Version,
                releaseDate = latest.ReleaseDate,
                changelog = latest.Changelog ?? "Cải tiến hiệu năng và độ ổn định hệ thống.",
                downloadUrl = latest.DownloadUrl ?? string.Empty,
                isMandatory = isMandatory
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lỗi khi kiểm tra phiên bản mới từ Supabase");
            return Ok(new
            {
                hasUpdate = false,
                currentVersion = CURRENT_VERSION,
                latestVersion = CURRENT_VERSION,
                message = "Không thể kết nối máy chủ kiểm tra cập nhật."
            });
        }
    }

    /// <summary>
    /// So sánh 2 chuỗi version ngữ nghĩa (VD: 1.1.0 > 1.0.0)
    /// Trả về: > 0 nếu v1 > v2, < 0 nếu v1 < v2, 0 nếu bằng nhau
    /// </summary>
    private static int CompareVersions(string v1, string v2)
    {
        try
        {
            string clean1 = v1.Trim().TrimStart('v', 'V');
            string clean2 = v2.Trim().TrimStart('v', 'V');

            var parts1 = clean1.Split('.').Select(p => int.TryParse(p, out int n) ? n : 0).ToArray();
            var parts2 = clean2.Split('.').Select(p => int.TryParse(p, out int n) ? n : 0).ToArray();

            int maxLen = Math.Max(parts1.Length, parts2.Length);
            for (int i = 0; i < maxLen; i++)
            {
                int num1 = i < parts1.Length ? parts1[i] : 0;
                int num2 = i < parts2.Length ? parts2[i] : 0;
                if (num1 != num2) return num1.CompareTo(num2);
            }
            return 0;
        }
        catch
        {
            return string.Compare(v1, v2, StringComparison.OrdinalIgnoreCase);
        }
    }
}

public class SupabaseAppVersionDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("release_date")]
    public DateTime? ReleaseDate { get; set; }

    [JsonPropertyName("changelog")]
    public string? Changelog { get; set; }

    [JsonPropertyName("download_url")]
    public string? DownloadUrl { get; set; }

    [JsonPropertyName("is_mandatory")]
    public bool IsMandatory { get; set; }

    [JsonPropertyName("min_version")]
    public string? MinVersion { get; set; }
}
