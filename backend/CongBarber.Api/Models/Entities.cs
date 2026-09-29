namespace CongBarber.Api.Models;

public class ServiceItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string Category { get; set; } = "Cắt tóc"; // "Cắt tóc", "Combo", "Hóa chất", "Gội & Chăm sóc"
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ServiceCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#f59e0b"; // Mã màu hex hoặc tên màu đại diện
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ProductItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockAlert { get; set; } = 3;
    public string Category { get; set; } = "Sáp vuốt tóc"; // "Sáp vuốt tóc", "Pomade", "Gôm xịt", "Dưỡng tóc"
    public string? Description { get; set; }
    public bool ShowOnPos { get; set; } = true; // true: Bán tại quầy POS, false: Kho vật tư / dùng nội bộ
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ProductCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#06b6d4"; // Mã màu hex nhận diện
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Order
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = string.Empty; // VD: CB-260922-001
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public decimal SubTotal { get; set; }
    public double DiscountPercent { get; set; } = 0; // % Giảm giá (0-100)
    public decimal DiscountAmount { get; set; } = 0; // Số tiền giảm tương ứng
    public decimal FinalAmount { get; set; } // Tiền thực thu sau giảm
    public string PaymentMethod { get; set; } = "VietQR"; // "VietQR" | "Cash"
    public string PaymentStatus { get; set; } = "Completed"; // "Completed" | "Cancelled"
    public string? Note { get; set; } // Ghi chú đơn hàng
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public string ItemType { get; set; } = "Service"; // "Service" | "Product"
    public int? ServiceId { get; set; }
    public int? ProductId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class ShopSetting
{
    public int Id { get; set; } = 1;
    public string ShopName { get; set; } = "CÔNG BARBER SHOP";
    public string Address { get; set; } = "123 Đường Cắt Tóc, TP. Hồ Chí Minh";
    public string Phone { get; set; } = "0987654321";
    public string Slogan { get; set; } = "Đẳng cấp phái mạnh - Tút lại vẻ đẹp trai";
    
    // VietQR Settings
    public string BankId { get; set; } = "MB"; // MB, VCB, TCB, VPB, TPB, ACB...
    public string BankName { get; set; } = "MBBank (Ngân hàng Quân Đội)";
    public string AccountNo { get; set; } = "0987654321";
    public string AccountName { get; set; } = "NGUYEN THANH CONG";
    public string QrTemplate { get; set; } = "compact2"; // "compact", "compact2", "qr_only"
}

public class Expense
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty; // VD: Tiền điện nước, Mua khăn giấy & dao cạo, Ăn trưa...
    public decimal Amount { get; set; }
    public string Category { get; set; } = "Phụ liệu & Hóa chất"; // "Mặt bằng & Tiện ích", "Phụ liệu & Hóa chất", "Dụng cụ & Máy móc", "Sinh hoạt & Ăn uống", "Khác"
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SystemLicense
{
    public int Id { get; set; } = 1;
    public string ShopCode { get; set; } = "DIRO-POS-01";
    public string ShopName { get; set; } = "DiroPos Store";
    public string LicenseKey { get; set; } = string.Empty;
    public string PlanType { get; set; } = "Trial"; // Trial, Monthly, Yearly, Lifetime
    public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30);
    public string Status { get; set; } = "Active"; // Active, Expired, Suspended
    public string HardwareId { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public DateTime? LastCheckedAt { get; set; }
}

