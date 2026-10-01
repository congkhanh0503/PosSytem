using DiroPos.Api.Data;
using DiroPos.Api.Models;
using DiroPos.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiroPos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BackupController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILicenseService _licenseService;
    private readonly IHttpClientFactory _httpClientFactory;

    private const string SUPABASE_URL = "https://lacdvcuztjafnwpitntk.supabase.co";
    private static readonly string SUPABASE_KEY = Environment.GetEnvironmentVariable("SUPABASE_KEY") 
        ?? System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("c2Jfc2VjcmV0X1psdVVDQ0RGeVRMaFlZYlBubGxOblFfU2s3WllzLUc="));

    private string GetDbPath()
    {
        // 1. Lấy trực tiếp từ DataSource của kết nối đang hoạt động
        try
        {
            var conn = _context.Database.GetDbConnection();
            string ds = conn.DataSource;
            if (!string.IsNullOrEmpty(ds) && System.IO.File.Exists(ds)) return ds;
            if (!string.IsNullOrEmpty(ds) && System.IO.File.Exists(Path.GetFullPath(ds))) return Path.GetFullPath(ds);
        }
        catch { }

        // 2. Lấy từ biến môi trường DB_PATH
        string? envPath = Environment.GetEnvironmentVariable("DB_PATH");
        if (!string.IsNullOrEmpty(envPath) && System.IO.File.Exists(envPath)) return envPath;

        // 3. Fallback thư mục app (ưu tiên diropos.db, sau đó congbarber.db)
        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "diropos.db");
        if (System.IO.File.Exists(fullPath)) return fullPath;
        if (System.IO.File.Exists("diropos.db")) return "diropos.db";

        string oldFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "congbarber.db");
        if (System.IO.File.Exists(oldFullPath)) return oldFullPath;
        if (System.IO.File.Exists("congbarber.db")) return "congbarber.db";

        return !string.IsNullOrEmpty(envPath) ? envPath : fullPath;
    }

    public BackupController(
        AppDbContext context, 
        IWebHostEnvironment env,
        ILicenseService licenseService,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _env = env;
        _licenseService = licenseService;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("info")]
    public async Task<ActionResult> GetBackupInfo()
    {
        string dbPath = GetDbPath();
        long fileSizeBytes = 0;
        DateTime lastModified = DateTime.UtcNow;

        if (System.IO.File.Exists(dbPath))
        {
            var fileInfo = new FileInfo(dbPath);
            fileSizeBytes = fileInfo.Length;
            lastModified = fileInfo.LastWriteTime;
        }

        int ordersCount = await _context.Orders.CountAsync();
        int expensesCount = await _context.Expenses.CountAsync();
        int servicesCount = await _context.Services.CountAsync();
        int productsCount = await _context.Products.CountAsync();

        return Ok(new
        {
            DatabaseFile = "diropos.db",
            FileSizeBytes = fileSizeBytes,
            FileSizeFormatted = $"{Math.Round((double)fileSizeBytes / 1024, 1)} KB",
            LastModified = lastModified,
            TotalOrders = ordersCount,
            TotalExpenses = expensesCount,
            TotalServices = servicesCount,
            TotalProducts = productsCount
        });
    }

    [HttpGet("download")]
    public async Task<IActionResult> DownloadBackup()
    {
        string dbPath = GetDbPath();
        if (!System.IO.File.Exists(dbPath))
        {
            return NotFound("Chưa tìm thấy file cơ sở dữ liệu diropos.db.");
        }

        string tempBackupPath = Path.Combine(Path.GetTempPath(), $"diropos_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.db");

        try
        {
            // Sử dụng SQLite Backup API để tạo snapshot nhất quán mà không khóa cơ sở dữ liệu
            using (var source = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly;Pooling=False"))
            using (var destination = new SqliteConnection($"Data Source={tempBackupPath};Pooling=False"))
            {
                await source.OpenAsync();
                await destination.OpenAsync();
                source.BackupDatabase(destination);
                destination.Close();
                source.Close();
            }

            SqliteConnection.ClearAllPools();

            byte[] bytes = await System.IO.File.ReadAllBytesAsync(tempBackupPath);
            string downloadFileName = $"diropos_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";

            return File(bytes, "application/octet-stream", downloadFileName);
        }
        finally
        {
            if (System.IO.File.Exists(tempBackupPath))
            {
                try { System.IO.File.Delete(tempBackupPath); } catch { }
            }
        }
    }

    [HttpPost("restore")]
    public async Task<IActionResult> RestoreBackup([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Vui lòng chọn file sao lưu (.db) hợp lệ.");
        }

        if (!file.FileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("File sao lưu phải có định dạng .db của SQLite.");
        }

        // Kiểm tra chữ ký SQLite header: "SQLite format 3\0"
        using (var stream = file.OpenReadStream())
        {
            byte[] header = new byte[16];
            int read = await stream.ReadAsync(header, 0, 16);
            string signature = Encoding.ASCII.GetString(header, 0, 15);
            if (signature != "SQLite format 3")
            {
                return BadRequest("File được chọn không phải là định dạng cơ sở dữ liệu SQLite hợp lệ.");
            }
        }

        string dbPath = GetDbPath();
        string dbDir = Path.GetDirectoryName(Path.GetFullPath(dbPath)) ?? AppDomain.CurrentDomain.BaseDirectory;
        string preRestoreBakPath = Path.Combine(dbDir, $"diropos_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak");

        // 0. Lưu lại thông tin bản quyền và cấu hình định danh tiệm hiện tại trước khi khôi phục
        SystemLicense? currentLicense = null;
        try
        {
            currentLicense = await _context.SystemLicenses.AsNoTracking().FirstOrDefaultAsync();
        }
        catch { }

        try
        {
            // 1. Sao lưu file hiện tại phòng ngừa
            if (System.IO.File.Exists(dbPath))
            {
                System.IO.File.Copy(dbPath, preRestoreBakPath, true);
            }

            // 2. Xóa connection pool để giải phóng lock
            SqliteConnection.ClearAllPools();

            // 3. Ghi đè file DB mới
            using (var fileStream = new FileStream(dbPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(fileStream);
            }

            // 4. Bảo toàn thông tin bản quyền và đồng bộ hạn dùng mới nhất từ Cloud Supabase
            await PreserveLicenseAfterRestoreAsync(currentLicense);

            return Ok(new
            {
                Message = "Khôi phục dữ liệu thành công!",
                BackupCreatedBeforeRestore = preRestoreBakPath
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi khi khôi phục dữ liệu: {ex.Message}");
        }
    }

    private async Task<(byte[] ZipBytes, string FileName, long OriginalSizeBytes)> CreateZipBackupAsync()
    {
        string dbPath = GetDbPath();
        if (!System.IO.File.Exists(dbPath))
        {
            throw new FileNotFoundException("Chưa tìm thấy file cơ sở dữ liệu diropos.db.");
        }

        long origSize = new FileInfo(dbPath).Length;
        string tempDbPath = Path.Combine(Path.GetTempPath(), $"snapshot_{Guid.NewGuid():N}.db");
        string tempZipPath = Path.Combine(Path.GetTempPath(), $"backup_{Guid.NewGuid():N}.zip");

        try
        {
            // 1. Snapshot an toàn bằng SQLite Backup API mà không làm khóa DB
            using (var source = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly;Pooling=False"))
            using (var destination = new SqliteConnection($"Data Source={tempDbPath};Pooling=False"))
            {
                await source.OpenAsync();
                await destination.OpenAsync();
                source.BackupDatabase(destination);
                destination.Close();
                source.Close();
            }

            SqliteConnection.ClearAllPools();

            // 2. Nén thành file zip với CompressionLevel.Fastest để tối ưu tốc độ tối đa
            using (var zip = ZipFile.Open(tempZipPath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(tempDbPath, "diropos.db", CompressionLevel.Fastest);
            }

            byte[] zipBytes = await System.IO.File.ReadAllBytesAsync(tempZipPath);
            string fileName = $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";

            return (zipBytes, fileName, origSize);
        }
        finally
        {
            if (System.IO.File.Exists(tempDbPath))
            {
                try { System.IO.File.Delete(tempDbPath); } catch { }
            }
            if (System.IO.File.Exists(tempZipPath))
            {
                try { System.IO.File.Delete(tempZipPath); } catch { }
            }
        }
    }

    [HttpPost("cloud-upload")]
    public async Task<IActionResult> CloudUploadBackup()
    {
        var license = await _licenseService.GetStatusAsync();
        string shopCode = !string.IsNullOrWhiteSpace(license.ShopCode) ? license.ShopCode.Trim().ToUpper() : "DP-DEFAULT";

        try
        {
            var (zipBytes, fileName, origSize) = await CreateZipBackupAsync();

            string supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? SUPABASE_URL;
            string supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY") ?? SUPABASE_KEY;

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("apikey", supabaseKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

            // 1. Upload file zip vào bucket: pos-backups/{shopCode}/{fileName}
            using var content = new ByteArrayContent(zipBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
            content.Headers.Add("x-upsert", "true");

            string uploadUrl = $"{supabaseUrl}/storage/v1/object/pos-backups/{shopCode}/{fileName}";
            var uploadRes = await client.PostAsync(uploadUrl, content);

            if (!uploadRes.IsSuccessStatusCode)
            {
                string err = await uploadRes.Content.ReadAsStringAsync();
                return StatusCode((int)uploadRes.StatusCode, $"Lỗi upload Supabase Storage: {err}");
            }

            // 2. Chạy ngầm dọn dẹp FIFO để duy trì tối đa 3 file (không chặn luồng trả lời người dùng)
            _ = Task.Run(async () =>
            {
                try
                {
                    using var cleanupClient = _httpClientFactory.CreateClient();
                    cleanupClient.DefaultRequestHeaders.Add("apikey", supabaseKey);
                    cleanupClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

                    var listReq = new StringContent(
                        JsonSerializer.Serialize(new { prefix = shopCode, limit = 50, sortBy = new { column = "name", order = "asc" } }),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var listRes = await cleanupClient.PostAsync($"{supabaseUrl}/storage/v1/object/list/pos-backups", listReq);
                    if (listRes.IsSuccessStatusCode)
                    {
                        string listJson = await listRes.Content.ReadAsStringAsync();
                        var items = JsonSerializer.Deserialize<List<SupabaseStorageItem>>(listJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (items != null && items.Count > 3)
                        {
                            var filesToDelete = items
                                .Where(i => !string.IsNullOrEmpty(i.Name))
                                .OrderBy(i => i.CreatedAt ?? DateTime.MinValue)
                                .Take(items.Count - 3)
                                .Select(i => $"{shopCode}/{i.Name}")
                                .ToList();

                            if (filesToDelete.Count > 0)
                            {
                                var delReq = new HttpRequestMessage(HttpMethod.Delete, $"{supabaseUrl}/storage/v1/object/pos-backups")
                                {
                                    Content = new StringContent(JsonSerializer.Serialize(new { prefixes = filesToDelete }), Encoding.UTF8, "application/json")
                                };
                                await cleanupClient.SendAsync(delReq);
                            }
                        }
                    }
                }
                catch
                {
                    // Lỗi dọn dẹp ngầm không ảnh hưởng tới tiến trình
                }
            });

            string formattedZipSize = $"{Math.Round((double)zipBytes.Length / 1024, 1)} KB";
            string formattedOrigSize = $"{Math.Round((double)origSize / 1024, 1)} KB";

            return Ok(new
            {
                Success = true,
                Message = $"Đã sao lưu lên Cloud thành công! File: {fileName} ({formattedZipSize})",
                FileName = fileName,
                ShopCode = shopCode,
                ZipSizeBytes = zipBytes.Length,
                ZipSizeFormatted = formattedZipSize,
                OriginalSizeFormatted = formattedOrigSize,
                UploadedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi tạo và tải backup lên Cloud: {ex.Message}");
        }
    }

    [HttpGet("cloud-list")]
    public async Task<IActionResult> GetCloudBackupList()
    {
        var license = await _licenseService.GetStatusAsync();
        string shopCode = !string.IsNullOrWhiteSpace(license.ShopCode) ? license.ShopCode.Trim().ToUpper() : "DP-DEFAULT";

        string supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? SUPABASE_URL;
        string supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY") ?? SUPABASE_KEY;

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("apikey", supabaseKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

            var listReq = new StringContent(
                JsonSerializer.Serialize(new { prefix = shopCode, limit = 20, sortBy = new { column = "created_at", order = "desc" } }),
                Encoding.UTF8,
                "application/json"
            );

            var listRes = await client.PostAsync($"{supabaseUrl}/storage/v1/object/list/pos-backups", listReq);
            if (!listRes.IsSuccessStatusCode)
            {
                return Ok(new List<object>());
            }

            string listJson = await listRes.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<List<SupabaseStorageItem>>(listJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<SupabaseStorageItem>();

            var result = items
                .Where(i => !string.IsNullOrEmpty(i.Name) && i.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(i => i.CreatedAt ?? DateTime.MinValue)
                .Take(3)
                .Select(i => new
                {
                    FileName = i.Name,
                    ShopCode = shopCode,
                    FileSizeBytes = i.Metadata?.Size ?? 0,
                    FileSizeFormatted = $"{Math.Round((double)(i.Metadata?.Size ?? 0) / 1024, 1)} KB",
                    CreatedAt = i.CreatedAt ?? DateTime.UtcNow,
                    DownloadUrl = $"{supabaseUrl}/storage/v1/object/public/pos-backups/{shopCode}/{i.Name}"
                });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi lấy danh sách backup: {ex.Message}");
        }
    }

    [HttpPost("cloud-restore")]
    public async Task<IActionResult> CloudRestoreBackup([FromBody] CloudRestoreRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FileName))
        {
            return BadRequest("Vui lòng chỉ định tên file backup cần khôi phục.");
        }

        var license = await _licenseService.GetStatusAsync();
        string shopCode = !string.IsNullOrWhiteSpace(dto.ShopCode) 
            ? dto.ShopCode.Trim().ToUpper() 
            : (!string.IsNullOrWhiteSpace(license.ShopCode) ? license.ShopCode.Trim().ToUpper() : "DP-DEFAULT");

        string supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? SUPABASE_URL;
        string downloadUrl = $"{supabaseUrl}/storage/v1/object/public/pos-backups/{shopCode}/{dto.FileName}";

        string tempZipPath = Path.Combine(Path.GetTempPath(), $"restore_{Guid.NewGuid():N}.zip");
        string tempExtractedDbPath = Path.Combine(Path.GetTempPath(), $"extracted_{Guid.NewGuid():N}.db");
        string dbPath = GetDbPath();
        string dbDir = Path.GetDirectoryName(Path.GetFullPath(dbPath)) ?? AppDomain.CurrentDomain.BaseDirectory;
        string preRestoreBakPath = Path.Combine(dbDir, $"diropos_pre_restore_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak");

        // 0. Lưu lại thông tin bản quyền và cấu hình định danh tiệm hiện tại trước khi khôi phục
        SystemLicense? currentLicense = null;
        try
        {
            currentLicense = await _context.SystemLicenses.AsNoTracking().FirstOrDefaultAsync();
        }
        catch { }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var downloadRes = await client.GetAsync(downloadUrl);
            if (!downloadRes.IsSuccessStatusCode)
            {
                return NotFound($"Không tìm thấy file backup '{dto.FileName}' trên Cloud.");
            }

            byte[] zipBytes = await downloadRes.Content.ReadAsByteArrayAsync();
            await System.IO.File.WriteAllBytesAsync(tempZipPath, zipBytes);

            // Giải nén file zip
            using (var zip = ZipFile.OpenRead(tempZipPath))
            {
                var entry = zip.GetEntry("diropos.db") ?? zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".db", StringComparison.OrdinalIgnoreCase));
                if (entry == null)
                {
                    return BadRequest("File sao lưu zip không chứa cơ sở dữ liệu SQLite (.db) hợp lệ.");
                }
                entry.ExtractToFile(tempExtractedDbPath, true);
            }

            // Kiểm tra chữ ký SQLite header
            using (var fs = new FileStream(tempExtractedDbPath, FileMode.Open, FileAccess.Read))
            {
                byte[] header = new byte[16];
                int read = await fs.ReadAsync(header, 0, 16);
                string signature = Encoding.ASCII.GetString(header, 0, 15);
                if (signature != "SQLite format 3")
                {
                    return BadRequest("File giải nén không đúng định dạng cơ sở dữ liệu SQLite.");
                }
            }

            // 1. Sao lưu file hiện tại phòng ngừa
            if (System.IO.File.Exists(dbPath))
            {
                System.IO.File.Copy(dbPath, preRestoreBakPath, true);
            }

            // 2. Xóa connection pool để giải phóng lock
            SqliteConnection.ClearAllPools();

            // 3. Ghi đè file DB mới
            using (var src = new FileStream(tempExtractedDbPath, FileMode.Open, FileAccess.Read))
            using (var dest = new FileStream(dbPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await src.CopyToAsync(dest);
            }

            // 4. Bảo toàn thông tin bản quyền và đồng bộ hạn dùng mới nhất từ Cloud Supabase
            await PreserveLicenseAfterRestoreAsync(currentLicense);

            return Ok(new
            {
                Success = true,
                Message = $"Khôi phục thành công dữ liệu từ bản sao lưu '{dto.FileName}'!",
                BackupCreatedBeforeRestore = preRestoreBakPath
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi khi khôi phục từ Cloud: {ex.Message}");
        }
        finally
        {
            if (System.IO.File.Exists(tempZipPath))
            {
                try { System.IO.File.Delete(tempZipPath); } catch { }
            }
            if (System.IO.File.Exists(tempExtractedDbPath))
            {
                try { System.IO.File.Delete(tempExtractedDbPath); } catch { }
            }
        }
    }

    private async Task PreserveLicenseAfterRestoreAsync(SystemLicense? currentLicense)
    {
        if (currentLicense == null) return;

        try
        {
            // Mở scope dịch vụ mới để kết nối tới file database vừa được khôi phục
            using var scope = HttpContext.RequestServices.CreateScope();
            var newContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var newLicenseSvc = scope.ServiceProvider.GetRequiredService<ILicenseService>();

            var targetLicense = await newContext.SystemLicenses.FirstOrDefaultAsync();
            if (targetLicense == null)
            {
                targetLicense = new SystemLicense { Id = 1 };
                newContext.SystemLicenses.Add(targetLicense);
            }

            // Bảo toàn toàn bộ thông tin bản quyền và định danh hiện tại của cửa hàng
            targetLicense.ShopCode = currentLicense.ShopCode;
            targetLicense.ShopName = currentLicense.ShopName;
            targetLicense.LicenseKey = currentLicense.LicenseKey;
            targetLicense.PlanType = currentLicense.PlanType;
            targetLicense.ActivatedAt = currentLicense.ActivatedAt;
            targetLicense.ExpiresAt = currentLicense.ExpiresAt;
            targetLicense.Status = currentLicense.Status;
            targetLicense.HardwareId = currentLicense.HardwareId;
            targetLicense.ContactPhone = currentLicense.ContactPhone;
            targetLicense.LastCheckedAt = currentLicense.LastCheckedAt;
            targetLicense.IsInitialized = currentLicense.IsInitialized;
            targetLicense.BusinessModel = currentLicense.BusinessModel;
            targetLicense.OwnerName = currentLicense.OwnerName;
            targetLicense.Address = currentLicense.Address;

            await newContext.SaveChangesAsync();

            // Đồng bộ lại với máy chủ Supabase để cập nhật số ngày bản quyền chuẩn nhất
            try
            {
                await newLicenseSvc.SyncWithServerAsync();
            }
            catch { }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RestoreBackup] Lỗi bảo toàn bản quyền: {ex.Message}");
        }
    }
}

public class CloudRestoreRequestDto
{
    public string FileName { get; set; } = string.Empty;
    public string? ShopCode { get; set; }
}

public class SupabaseStorageItem
{
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    public SupabaseStorageMetadata? Metadata { get; set; }
}

public class SupabaseStorageMetadata
{
    public long Size { get; set; }
    public string? Mimetype { get; set; }
}

