# DiroPos - Hệ Thống Quản Lý Bán Hàng & Thu Ngân Đa Năng 💈☕🛍️

> **DiroPos** là giải pháp Point-of-Sale (POS) và quản trị doanh thu - kho hàng chuyên nghiệp, hiện đại, mượt mà dành cho các cửa hàng bán lẻ, tiệm cắt tóc, salon, quán cafe, spa và chuỗi dịch vụ. Hệ thống được tối ưu hóa cho màn hình cảm ứng, tích hợp thanh toán VietQR động, kiểm soát tồn kho nghiêm ngặt và đồng bộ quản trị bản quyền từ xa qua Cloud.

---

## 🌟 Tính Năng Nổi Bật

### 1. ⚡ Bán Hàng Cảm Ứng Cực Nhanh (Touch POS)
- **Thao tác 1 chạm**: Thêm nhanh sản phẩm/dịch vụ vào giỏ hàng, giao diện phân loại danh mục trực quan, chuyển đổi mượt mà.
- **Tìm kiếm thông minh**: Tìm kiếm theo tên, mã sản phẩm hoặc quét mã vạch tức thì.
- **Chiết khấu & Giảm giá linh hoạt**: Nút chọn nhanh (5%, 10%, 15%, 20%) hoặc nhập % tùy ý, hệ thống tự tính toán số tiền giảm.
- **Ghi chú đơn hàng & Khách hàng**: Lưu thông tin khách hàng (Tên, SĐT), ghi chú yêu cầu riêng cho từng đơn.

### 2. 📦 Kiểm Soát Tồn Kho Nghiêm Ngặt (Stock Management)
- **Chặn bán âm kho ở 3 lớp**:
  - **Giao diện Menu**: Tự động đánh dấu nhãn *"Hết hàng"*, làm mờ thẻ sản phẩm và vô hiệu hóa nút thêm khi tồn kho `= 0`.
  - **Giỏ hàng & Modal xác nhận**: Hiển thị rõ số lượng còn trong kho (`Kho: x`), khóa nút tăng số lượng khi chạm trần tồn kho, chặn nút thanh toán nếu có món lỗi tồn kho.
  - **Backend API**: Kiểm tra tồn kho trước khi trừ kho và tạo đơn, ngăn chặn triệt để tình trạng âm kho do thao tác đồng thời.
- **Phân loại rạch ròi**: Quản lý song song Sản phẩm (có trừ kho) và Dịch vụ (không giới hạn kho).

### 3. 💳 Đa Dạng Phương Thức Thanh Toán
- **Tiền mặt (Cash)**: Tích hợp bàn phím tính tiền thông minh, các nút tiền chẵn nhanh (50k, 100k, 200k, 500k), tự động tính số tiền thối lại cho khách.
- **Thanh toán VietQR Động (NAPAS 247)**: Tự động tạo mã QR chứa chính xác số tiền cần thu và nội dung đơn hàng. Khách hàng quét bằng bất kỳ ứng dụng ngân hàng nào là thanh toán chuẩn xác 100%.

### 4. ⏰ Quản Lý Ca Làm Việc Chặt Chẽ (Shift Management)
- **Mở ca & Khai báo tiền đầu ca**: Ghi nhận số tiền ban đầu tại ngăn kéo thu ngân khi nhân viên bắt đầu ca.
- **Tổng kết & Đóng ca**: Đối soát doanh thu tiền mặt, chuyển khoản, thu chi phát sinh trong ca và tính toán chênh lệch tiền mặt thực tế.
- **Bảo toàn dữ liệu**: Tự động khóa tính năng chỉnh sửa/hủy đơn hàng của các ca đã đóng nhằm chống gian lận thu ngân.

### 5. 📊 Quản Lý Thu Chi & Báo Cáo Lợi Nhuận Ròng
- **Ghi nhận chi phí linh hoạt**: Quản lý các khoản chi phát sinh (nhập hàng, điện nước, mặt bằng, tiếp khách...).
- **Tính toán lãi ròng tự động**: `Lợi nhuận ròng = Doanh thu - Tổng chi phí`.
- **Bộ lọc đa mốc thời gian**: Xem nhanh hôm nay, hôm qua, 7 ngày qua, tháng này, tháng trước và đối soát % tăng trưởng doanh thu trực quan.

### 6. ☁️ Quản Trị Tập Trung & Bản Quyền Cloud (DiroAdmin)
- **Đồng bộ đám mây (Supabase)**: Tự động gửi tín hiệu Heartbeat giám sát trạng thái Online/Offline của các điểm bán.
- **Kích hoạt & Gia hạn bản quyền từ xa**: Khóa/mở bản quyền máy POS từ xa thông qua cổng [DiroAdmin](file:///d:/Project/pos/DiroAdmin).
- **An toàn dữ liệu thương mại**: Cơ chế bảo vệ dữ liệu nội bộ, quản trị và sao lưu định kỳ tập trung an toàn.

---

## 🛠️ Ngăn Xếp Công Nghệ (Tech Stack)

| Thành Phần | Công Nghệ Sử Dụng | Mô Tả |
| :--- | :--- | :--- |
| **Frontend** | Vue 3 (Composition API), Vite | Giao diện Single Page Application hiện đại, tốc độ phản hồi cực nhanh |
| **Styling** | Tailwind CSS, Lucide Icons | Thiết kế tối ưu hiển thị máy POS, tablet, màn hình cảm ứng |
| **State Management** | Pinia | Quản lý trạng thái giỏ hàng, ca làm việc, danh mục và đơn hàng tập trung |
| **Backend** | .NET 10 Web API, C# | Hiệu năng cao, kiến trúc RESTful chuẩn mực |
| **Database** | SQLite + Entity Framework Core | Cơ sở dữ liệu nhúng gọn nhẹ, không cần cài server SQL cồng kềnh |
| **Đóng gói Desktop** | Inno Setup, .NET Single-File | Xuất bản file cài đặt 1-Click `.exe` độc lập, không yêu cầu cài môi trường |
| **Container** | Docker & Docker Compose | Đóng gói Backend + Frontend + Nginx sẵn sàng cho môi trường Server |

---

## 🚀 Hướng Dẫn Cài Đặt & Triển Khai

### Cách 1: Cài Đặt 1-Click Bằng File Setup (Dành cho Người Dùng Cuối / Máy POS)
Dành cho các cửa hàng triển khai thực tế trên máy tính Windows (Windows 10 / 11):

1. Tải bộ cài đặt mới nhất: `DiroPos_Setup_v1.0.0.exe` (trong thư mục `installer_output/`).
2. Nhấp đúp để cài đặt:
   - Quá trình cài đặt diễn ra hoàn toàn tự động chỉ trong vài giây.
   - Không yêu cầu cài đặt trước .NET Runtime hay Node.js.
   - Tự động tạo biểu tượng ngoài màn hình Desktop với icon DiroPos chính thức.
   - Tự động đăng ký khởi động cùng Windows: Bật máy tính là hệ thống tự khởi chạy ngầm và mở ngay màn hình bán hàng.

---

### Cách 2: Triển Khai Bằng Docker (Dành cho Quản Trị Viên / Server)
Yêu cầu: Máy đã cài đặt Docker & Docker Compose.

```bash
# Khởi động toàn bộ hệ thống bằng 1 lệnh duy nhất:
docker compose up -d --build
```

Sau khi khởi chạy thành công:
- **Giao diện bán hàng (POS)**: `http://localhost/pos`
- **Dashboard quản trị**: `http://localhost/`
- **Tài liệu Swagger API**: `http://localhost:5012/swagger`

---

### Cách 3: Chạy Môi Trường Phát Triển (Local Development)

#### 1. Yêu cầu hệ thống:
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Node.js (v18+)](https://nodejs.org/) & npm

#### 2. Khởi động Backend (.NET 10 API):
```bash
cd backend/DiroPos.Api
dotnet restore
dotnet run
# API lắng nghe tại: http://localhost:5012
```

#### 3. Khởi động Frontend (Vue 3 + Vite):
```bash
cd frontend
npm install
npm run dev
# Giao diện chạy tại: http://localhost:5173
```

---

## 📦 Quy Trình Đóng Gói Bộ Cài Đặt (Build Installer)

Để tự động biên dịch toàn bộ Frontend, Backend thành 1 file EXE độc lập và đóng gói bộ cài đặt:

```powershell
# Chạy script đóng gói tự động tại thư mục gốc PosSytem:
powershell -ExecutionPolicy Bypass -File .\build_installer.ps1
```

Script sẽ tự động thực hiện 4 bước:
1. Dọn dẹp thư mục build cũ.
2. Build Vue.js Frontend (Minified, loại bỏ console log) và sao chép vào `wwwroot` của Backend.
3. Xuất bản Backend .NET 10 thành Single-File EXE độc lập (`win-x64`).
4. Sử dụng Inno Setup biên dịch ra file bộ cài đặt: `installer_output/DiroPos_Setup_v1.0.0.exe`.

---

## 📁 Cấu Trúc Thư Mục Dự Án

```text
PosSytem/
├── backend/
│   └── DiroPos.Api/             # Nguồn Backend ASP.NET Core Web API
│       ├── Controllers/         # API Controllers (Orders, Products, Shifts, Expenses...)
│       ├── Data/                # EF Core AppDbContext & Migrations
│       ├── Models/              # Entity Models dữ liệu
│       └── Services/            # Business Services & Cloud Sync Service
├── frontend/                    # Nguồn Vue 3 Single Page Application
│   ├── src/
│   │   ├── components/          # Các Component giao diện (Modal thanh toán, Header, ...)
│   │   ├── stores/              # Pinia Stores (posStore, shiftStore, productStore...)
│   │   ├── views/               # Màn hình chính (POSView, HistoryView, ExpenseView...)
│   │   └── router/              # Vue Router cấu hình điều hướng
├── installer_output/            # Chứa file cài đặt DiroPos_Setup_v1.0.0.exe
├── build_installer.ps1          # Script tự động hóa đóng gói bộ cài đặt
├── DiroPos_Setup.iss            # Cấu hình kịch bản đóng gói Inno Setup
├── docker-compose.yml           # Cấu hình triển khai Docker container
└── README.md                    # Tài liệu hướng dẫn sử dụng và phát triển
```

---

## 🔒 Bản Quyền & Giấy Phép
Dự án được phát triển và vận hành độc quyền bởi **DiroPos Team**. Mọi quyền được bảo lưu.
