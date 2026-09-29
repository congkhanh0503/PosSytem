using CongBarber.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CongBarber.Api.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.EnsureCreated();

        // Đảm bảo cấu trúc các bảng tồn tại trong SQLite và bật WAL mode
        context.Database.ExecuteSqlRaw(@"
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS ""Expenses"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Expenses"" PRIMARY KEY AUTOINCREMENT,
                ""Title"" TEXT NOT NULL,
                ""Amount"" TEXT NOT NULL,
                ""Category"" TEXT NOT NULL,
                ""Date"" TEXT NOT NULL,
                ""Note"" TEXT NULL,
                ""CreatedAt"" TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS ""ServiceCategories"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_ServiceCategories"" PRIMARY KEY AUTOINCREMENT,
                ""Name"" TEXT NOT NULL,
                ""Color"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS ""ProductCategories"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_ProductCategories"" PRIMARY KEY AUTOINCREMENT,
                ""Name"" TEXT NOT NULL,
                ""Color"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS ""SystemLicenses"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_SystemLicenses"" PRIMARY KEY AUTOINCREMENT,
                ""ShopCode"" TEXT NOT NULL,
                ""ShopName"" TEXT NOT NULL,
                ""LicenseKey"" TEXT NOT NULL,
                ""PlanType"" TEXT NOT NULL,
                ""ActivatedAt"" TEXT NOT NULL,
                ""ExpiresAt"" TEXT NOT NULL,
                ""Status"" TEXT NOT NULL,
                ""HardwareId"" TEXT NOT NULL,
                ""ContactPhone"" TEXT NULL,
                ""LastCheckedAt"" TEXT NULL
            );
        ");

        // Đảm bảo tương thích ngược: Kiểm tra cột ShowOnPos trước để tránh sinh log fail
        try
        {
            var conn = context.Database.GetDbConnection();
            bool wasOpen = conn.State == System.Data.ConnectionState.Open;
            if (!wasOpen) conn.Open();

            bool columnExists = false;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"PRAGMA table_info(""Products"");";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string colName = reader.GetString(1);
                    if (string.Equals(colName, "ShowOnPos", StringComparison.OrdinalIgnoreCase))
                    {
                        columnExists = true;
                        break;
                    }
                }
            }

            if (!columnExists)
            {
                using var alterCmd = conn.CreateCommand();
                alterCmd.CommandText = @"ALTER TABLE ""Products"" ADD COLUMN ""ShowOnPos"" INTEGER NOT NULL DEFAULT 1;";
                alterCmd.ExecuteNonQuery();
            }

            if (!wasOpen) conn.Close();
        }
        catch
        {
            // Bỏ qua nếu có ngoại lệ bất ngờ
        }

        // 1. Seed ServiceCategories mặc định (Màu nhận diện chuẩn)
        if (!context.ServiceCategories.Any())
        {
            context.ServiceCategories.AddRange(
                new ServiceCategory { Name = "Cắt tóc", Color = "#f59e0b" },       // Vàng kim
                new ServiceCategory { Name = "Combo", Color = "#8b5cf6" },         // Tím thời thượng
                new ServiceCategory { Name = "Hóa chất", Color = "#ec4899" },      // Hồng rực rỡ
                new ServiceCategory { Name = "Gội & Chăm sóc", Color = "#10b981" },// Xanh ngọc
                new ServiceCategory { Name = "Cạo & Rái tai", Color = "#06b6d4" }  // Xanh Cyan
            );
            context.SaveChanges();
        }

        // 2. Seed ProductCategories mặc định (Màu nhận diện chuẩn)
        if (!context.ProductCategories.Any())
        {
            context.ProductCategories.AddRange(
                new ProductCategory { Name = "Sáp vuốt tóc", Color = "#06b6d4" },  // Xanh Cyan
                new ProductCategory { Name = "Pomade", Color = "#f59e0b" },        // Vàng kim
                new ProductCategory { Name = "Gôm xịt", Color = "#8b5cf6" },        // Tím
                new ProductCategory { Name = "Dưỡng tóc", Color = "#10b981" },      // Xanh lục
                new ProductCategory { Name = "Dầu gội/xả", Color = "#3b82f6" }     // Xanh dương
            );
            context.SaveChanges();
        }

        // 3. Seed ShopSetting cơ bản cho tiệm (Cấu hình VietQR và thông tin quán)
        if (!context.ShopSettings.Any())
        {
            context.ShopSettings.Add(new ShopSetting
            {
                Id = 1,
                ShopName = "DiroPos Store",
                Address = "128 Đường Nguyễn Văn Cừ, Quận 5, TP. Hồ Chí Minh",
                Phone = "0987.654.321",
                Slogan = "Hệ thống quản lý bán hàng thông minh - Tối ưu vận hành",
                BankId = "MB",
                BankName = "MBBank",
                AccountNo = "0987654321",
                AccountName = "DIRO POS",
                QrTemplate = "compact2"
            });
            context.SaveChanges();
        }
    }
}
