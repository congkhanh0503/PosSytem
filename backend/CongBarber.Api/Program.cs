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

// Đường dẫn file SQLite luôn nằm cạnh file thực thi
string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "congbarber.db");

// 1. Cấu hình Database SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? $"Data Source={dbPath}"));

// 2. Đăng ký Services
builder.Services.AddScoped<IVietQrService, VietQrService>();

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "CongBarber POS API",
        Version = "v1",
        Description = "API hệ thống POS tiệm cắt tóc 1 thợ với tích hợp VietQR"
    });
});

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CongBarber POS API v1");
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

// Tự động mở trình duyệt web khi ứng dụng bắt đầu chạy
app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        // Chờ 500ms để server sẵn sàng
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
});

app.Run();
