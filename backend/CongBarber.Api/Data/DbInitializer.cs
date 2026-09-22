using CongBarber.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CongBarber.Api.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.EnsureCreated();

        // Đảm bảo bảng Expenses tồn tại nếu cơ sở dữ liệu đã tạo từ trước
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""Expenses"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Expenses"" PRIMARY KEY AUTOINCREMENT,
                ""Title"" TEXT NOT NULL,
                ""Amount"" TEXT NOT NULL,
                ""Category"" TEXT NOT NULL,
                ""Date"" TEXT NOT NULL,
                ""Note"" TEXT NULL,
                ""CreatedAt"" TEXT NOT NULL
            );
        ");

        // 1. Seed ShopSetting
        if (!context.ShopSettings.Any())
        {
            context.ShopSettings.Add(new ShopSetting
            {
                Id = 1,
                ShopName = "CÔNG BARBER SHOP",
                Address = "128 Đường Nguyễn Văn Cừ, Quận 5, TP. Hồ Chí Minh",
                Phone = "0987.654.321",
                Slogan = "Phong độ - Bản lĩnh - Chuẩn men",
                BankId = "MB", // Ngân hàng Quân Đội
                BankName = "MBBank",
                AccountNo = "0987654321",
                AccountName = "NGUYEN THANH CONG",
                QrTemplate = "compact2"
            });
            context.SaveChanges();
        }

        // 2. Seed Services
        if (!context.Services.Any())
        {
            context.Services.AddRange(
                new ServiceItem
                {
                    Name = "Cắt tóc Barber Classic",
                    Price = 100000,
                    DurationMinutes = 30,
                    Category = "Cắt tóc",
                    Description = "Tư vấn kiểu tóc hợp khuôn mặt, cắt Fade sắc nét, cạo viền và sấy tạo kiểu"
                },
                new ServiceItem
                {
                    Name = "Combo Đế Vương VIP",
                    Price = 200000,
                    DurationMinutes = 60,
                    Category = "Combo",
                    Description = "Cắt tóc + Cạo mặt rái tai + Gội đầu dưỡng sinh + Massage cổ vai gáy & đắp mặt nạ"
                },
                new ServiceItem
                {
                    Name = "Cạo mặt & Rái tai êm ái",
                    Price = 50000,
                    DurationMinutes = 20,
                    Category = "Cạo & Rái tai",
                    Description = "Cạo râu sạch sẽ, dưỡng ẩm khăn nóng và lấy ráy tai chuyên nghiệp"
                },
                new ServiceItem
                {
                    Name = "Gội đầu dưỡng sinh thư giãn",
                    Price = 80000,
                    DurationMinutes = 35,
                    Category = "Gội & Chăm sóc",
                    Description = "Gội thảo mộc, bấm huyệt lưu thông khí huyết, massage đầu vai gáy"
                },
                new ServiceItem
                {
                    Name = "Uốn tóc Texture / Premlock / Sidepart",
                    Price = 350000,
                    DurationMinutes = 90,
                    Category = "Hóa chất",
                    Description = "Uốn phồng chân tóc, tạo sóng bồng bềnh chuẩn soái ca, giữ nếp 3-4 tháng"
                },
                new ServiceItem
                {
                    Name = "Nhuộm tóc thời trang / Tẩy tóc",
                    Price = 300000,
                    DurationMinutes = 75,
                    Category = "Hóa chất",
                    Description = "Nhuộm màu tôn da (Nâu khói, Xám khói, Nâu lạnh...) bằng thuốc cao cấp không rát da đầu"
                },
                new ServiceItem
                {
                    Name = "Phục hồi tóc hư tổn Keratin",
                    Price = 150000,
                    DurationMinutes = 40,
                    Category = "Gội & Chăm sóc",
                    Description = "Ủ tinh chất Keratin tái tạo sợi tóc bóng mượt, chống xơ rối"
                }
            );
            context.SaveChanges();
        }

        // 3. Seed Products
        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new ProductItem
                {
                    Name = "Sáp Volcanic Clay V5 nắp nhôm",
                    Sku = "SAP-VOLCANIC-01",
                    CostPrice = 190000,
                    SalePrice = 280000,
                    StockQuantity = 24,
                    LowStockAlert = 3,
                    Category = "Sáp vuốt tóc",
                    Description = "Độ giữ nếp Extreme Hold siêu đỉnh, hoàn thiện mờ tự nhiên, hút dầu cực tốt"
                },
                new ProductItem
                {
                    Name = "Sáp Morris Motley Matte Balm",
                    Sku = "SAP-MORRIS-02",
                    CostPrice = 490000,
                    SalePrice = 650000,
                    StockQuantity = 12,
                    LowStockAlert = 2,
                    Category = "Sáp vuốt tóc",
                    Description = "Dòng sáp vuốt tóc cao cấp từ Úc, giàu dưỡng chất, giữ nếp tự nhiên không bóng"
                },
                new ProductItem
                {
                    Name = "Pomade gốc nước Schmiere Water Based",
                    Sku = "POM-SCHMIERE-01",
                    CostPrice = 250000,
                    SalePrice = 360000,
                    StockQuantity = 15,
                    LowStockAlert = 3,
                    Category = "Pomade",
                    Description = "Dành riêng cho phong cách Pompadour, Slick Back cổ điển, dễ gội rửa"
                },
                new ProductItem
                {
                    Name = "Gôm xịt tạo kiểu Silhouette Đức",
                    Sku = "GOM-SILHOUETTE-01",
                    CostPrice = 85000,
                    SalePrice = 150000,
                    StockQuantity = 30,
                    LowStockAlert = 5,
                    Category = "Gôm xịt",
                    Description = "Khóa nếp tóc suốt 24h, mùi thơm nhẹ dịu không nồng hắc"
                },
                new ProductItem
                {
                    Name = "Tinh dầu dưỡng tóc Barber Argan Oil 60ml",
                    Sku = "DAU-ARGAN-01",
                    CostPrice = 130000,
                    SalePrice = 220000,
                    StockQuantity = 18,
                    LowStockAlert = 3,
                    Category = "Dưỡng tóc",
                    Description = "Dưỡng mềm mượt, bảo vệ tóc trước nhiệt độ máy sấy"
                }
            );
            context.SaveChanges();
        }

        // 4. Seed 1-2 Đơn mẫu để có dữ liệu Dashboard ban đầu
        if (!context.Orders.Any())
        {
            var classicCut = context.Services.FirstOrDefault(s => s.Name.Contains("Classic"));
            var comboVip = context.Services.FirstOrDefault(s => s.Name.Contains("Đế Vương"));
            var volcanic = context.Products.FirstOrDefault(p => p.Name.Contains("Volcanic"));

            var today = DateTime.UtcNow;

            var order1 = new Order
            {
                OrderCode = $"CB-{today:yyMMdd}-001",
                CustomerName = "Anh Tuấn",
                CustomerPhone = "0901234567",
                SubTotal = 200000,
                DiscountPercent = 10,
                DiscountAmount = 20000,
                FinalAmount = 180000,
                PaymentMethod = "VietQR",
                PaymentStatus = "Completed",
                Note = "Khách quen khu vực gần tiệm, thích vuốt tóc sidepart",
                CreatedAt = today.AddHours(-3),
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ItemType = "Service",
                        ServiceId = comboVip?.Id,
                        ItemName = comboVip?.Name ?? "Combo Đế Vương VIP",
                        Quantity = 1,
                        UnitPrice = 200000,
                        TotalPrice = 200000
                    }
                }
            };

            var order2 = new Order
            {
                OrderCode = $"CB-{today:yyMMdd}-002",
                CustomerName = "Em Hoàng (Sinh viên)",
                CustomerPhone = "0912345678",
                SubTotal = 380000,
                DiscountPercent = 0,
                DiscountAmount = 0,
                FinalAmount = 380000,
                PaymentMethod = "VietQR",
                PaymentStatus = "Completed",
                Note = "Cắt Fade chân trắng + lấy thêm 1 hộp sáp Volcanic Clay",
                CreatedAt = today.AddHours(-1),
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ItemType = "Service",
                        ServiceId = classicCut?.Id,
                        ItemName = classicCut?.Name ?? "Cắt tóc Barber Classic",
                        Quantity = 1,
                        UnitPrice = 100000,
                        TotalPrice = 100000
                    },
                    new OrderItem
                    {
                        ItemType = "Product",
                        ProductId = volcanic?.Id,
                        ItemName = volcanic?.Name ?? "Sáp Volcanic Clay V5 nắp nhôm",
                        Quantity = 1,
                        UnitPrice = 280000,
                        TotalPrice = 280000
                    }
                }
            };

            context.Orders.AddRange(order1, order2);
            context.SaveChanges();
        }

        // 5. Seed Chi Tiêu mẫu
        if (!context.Expenses.Any())
        {
            var today = DateTime.UtcNow;
            context.Expenses.AddRange(
                new Expense
                {
                    Title = "Mua lưỡi dao cạo Dorco & Bọt cạo râu",
                    Amount = 180000,
                    Category = "Phụ liệu & Hóa chất",
                    Date = today.AddHours(-5),
                    Note = "Mua 2 hộp lưỡi lam + 1 chai bọt cạo",
                    CreatedAt = today.AddHours(-5)
                },
                new Expense
                {
                    Title = "Tiền điện & nước tiệm tháng này",
                    Amount = 650000,
                    Category = "Mặt bằng & Tiện ích",
                    Date = today.AddDays(-2),
                    Note = "Thanh toán qua app điện lực",
                    CreatedAt = today.AddDays(-2)
                },
                new Expense
                {
                    Title = "Mua khăn quấn cổ & Dầu tra tông đơ",
                    Amount = 95000,
                    Category = "Dụng cụ & Máy móc",
                    Date = today.AddDays(-1),
                    Note = "Phụ kiện vệ sinh kéo & tông đơ",
                    CreatedAt = today.AddDays(-1)
                },
                new Expense
                {
                    Title = "Cơm trưa & Cà phê ca cắt tóc",
                    Amount = 60000,
                    Category = "Sinh hoạt & Ăn uống",
                    Date = today.AddHours(-2),
                    Note = "Ăn trưa tại tiệm",
                    CreatedAt = today.AddHours(-2)
                }
            );
            context.SaveChanges();
        }
    }
}
