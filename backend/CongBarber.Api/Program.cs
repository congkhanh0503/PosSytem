using CongBarber.Api.Data;
using CongBarber.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Thiết lập cổng lắng nghe mặc định http://0.0.0.0:5012 nếu chưa cấu hình
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls("http://0.0.0.0:5012");
}

// Đường dẫn file SQLite (ưu tiên biến môi trường DB_PATH nếu chạy trong Docker container có volume mount)
string? envDbPath = Environment.GetEnvironmentVariable("DB_PATH");
string dbPath = !string.IsNullOrEmpty(envDbPath)
    ? envDbPath
    : (builder.Configuration.GetConnectionString("DefaultConnection")?.Replace("Data Source=", "")
       ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "congbarber.db"));

string? dbDir = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
{
    Directory.CreateDirectory(dbDir);
}

// Tự động di chuyển DB từ thư mục app cũ sang volume data mới nếu cần để tránh mất dữ liệu
if (!File.Exists(dbPath))
{
    string oldDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "congbarber.db");
    if (File.Exists(oldDbPath))
    {
        try { File.Copy(oldDbPath, dbPath, true); } catch { }
    }
    else if (File.Exists("congbarber.db"))
    {
        try { File.Copy("congbarber.db", dbPath, true); } catch { }
    }
}

// 1. Cấu hình Database SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// 2. Đăng ký Services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IVietQrService, VietQrService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();

// 3. Cấu hình Controllers & JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 4. Cấu hình CORS cho phép Frontend Vue gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 5. Cấu hình Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 6. Tự động khởi tạo Database và Seed dữ liệu mẫu
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbInitializer.Initialize(context);
}

// 7. Pipeline HTTP request
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DiroPos API v1");
        c.RoutePrefix = "swagger";
    });
}

// Phục vụ giao diện Web SPA Vue 3
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// SPA fallback cho Vue Router
app.MapFallbackToFile("index.html");

// Tự động mở trình duyệt web & gửi heartbeat ping ngầm lên Cloud khi ứng dụng bắt đầu chạy
app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        // Chờ 500ms để server sẵn sàng rồi mở trình duyệt
        Task.Delay(500).ContinueWith(_ =>
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "http://localhost:5012",
                    UseShellExecute = true
                });
            }
            catch { }
        });
    }
    catch { }

    // Heartbeat: Tự động gửi ping ngầm lên Cloud Supabase mỗi 60s để DiroAdmin luôn nhận diện Online
    _ = Task.Run(async () =>
    {
        await Task.Delay(1500);
        while (true)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var licenseSvc = scope.ServiceProvider.GetRequiredService<ILicenseService>();
                await licenseSvc.SyncWithServerAsync();
            }
            catch { }

            await Task.Delay(TimeSpan.FromSeconds(60));
        }
    });
});

app.Run();
