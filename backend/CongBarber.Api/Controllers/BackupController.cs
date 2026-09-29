using CongBarber.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CongBarber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BackupController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

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

        // 3. Fallback thư mục app
        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "congbarber.db");
        if (System.IO.File.Exists(fullPath)) return fullPath;
        if (System.IO.File.Exists("congbarber.db")) return "congbarber.db";

        return !string.IsNullOrEmpty(envPath) ? envPath : fullPath;
    }

    public BackupController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
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
            DatabaseFile = "congbarber.db",
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
            return NotFound("Chưa tìm thấy file cơ sở dữ liệu congbarber.db.");
        }

        string tempBackupPath = Path.Combine(Path.GetTempPath(), $"congbarber_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.db");

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
            string downloadFileName = $"congbarber_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";

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
        string preRestoreBakPath = $"congbarber_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak";

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
}
