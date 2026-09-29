# DiroPos - Hệ Thống Quản Lý Bán Hàng & Thu Ngân Đa Năng 💈☕🛍️

Hệ thống Point-of-Sale (POS) và quản lý tài chính chuyên nghiệp, thông minh dành cho các cửa hàng, tiệm cắt tóc, salon, quán cafe, spa và mô hình kinh doanh bán lẻ. Được thiết kế theo phong cách hiện đại, tinh tế, tối ưu hóa thao tác chạm nhanh và tích hợp công nghệ quản lý bản quyền từ xa.

---

## 🌟 Tính Năng Nổi Bật

- ⚡ **Bán hàng cảm ứng cực nhanh (Touch POS)**: Thao tác 1 chạm thêm dịch vụ / sản phẩm, tùy biến danh mục trực quan.
- 🏷️ **Giảm giá linh hoạt (%)**: Nút chọn nhanh 5%, 10%, 15%, 20% hoặc nhập % tùy ý, tự tính số tiền giảm.
- 📝 **Ghi chú đơn hàng (Note)**: Lưu yêu cầu của khách hàng, ghi chú chăm sóc và theo dõi.
- 📲 **Thanh toán VietQR NAPAS 247**: Tự động sinh mã QR với số tiền chính xác sau giảm giá, khách quét app ngân hàng bất kỳ là thanh toán ngay.
- 💰 **Quản lý Chi tiêu & Lợi nhuận ròng**: Phân loại chi phí và tự động tính lãi ròng `Lợi nhuận = Doanh thu - Chi phí`.
- 📅 **Bộ lọc lịch đa năng & Đối soát tháng**: Xem nhanh hôm nay, hôm qua, 7 ngày, tháng này, và so sánh đối soát % tăng trưởng với tháng trước.
- 💾 **Sao lưu & Khôi phục dữ liệu**: Tải bản sao lưu SQLite `.db` nguyên vẹn và nạp lại an toàn với cơ chế backup dự phòng tự động.
- ☁️ **Đồng bộ Quản trị Tập trung (DiroAdmin)**: Tự động kết nối Cloud Supabase, quản lý danh sách tiệm, giám sát trạng thái online, cấp quyền và gia hạn bản quyền từ xa.
- 🐳 **Triển khai 1 lệnh bằng Docker**: Đóng gói toàn bộ Backend + Frontend + Nginx + Database.

---

## 🛠️ Ngăn Xếp Công Nghệ (Tech Stack)

- **Frontend**: Vue 3 (Composition API), Vite, Tailwind CSS, Lucide Icons, Axios, Pinia.
- **Backend**: .NET 10 Web API, Entity Framework Core, SQLite, Swagger OpenAPI.
- **Thanh toán**: VietQR.io QuickLink chuẩn Napas 247.
- **Triển khai**: Docker & Docker Compose, Nginx Alpine Reverse Proxy, hoặc chạy file `.exe` độc lập.

---

## 🚀 Hướng Dẫn Khởi Chạy Nhanh

### Cách 1: Khởi Chạy Bằng Docker (Khuyến nghị)
Yêu cầu: Đã cài Docker Desktop.
```bash
# Khởi động toàn bộ hệ thống bằng 1 lệnh duy nhất:
docker compose up -d --build
```
Truy cập:
- **Giao diện POS**: `http://localhost/pos` (hoặc `http://localhost:5173/pos`)
- **Tổng quan Dashboard**: `http://localhost/`
- **Swagger API**: `http://localhost:5012/swagger`

Chi tiết xem tại: [DOCKER_GUIDE.md](file:///d:/Pos/PosSytem/DOCKER_GUIDE.md)

---

### Cách 2: Chạy Thủ Công Trên Máy (Local Development)

#### Backend (.NET 10)
```bash
cd backend/DiroPos.Api
dotnet run
# API lắng nghe tại: http://localhost:5012
```

#### Frontend (Vue 3)
```bash
cd frontend
npm install
npm run dev
# Giao diện chạy tại: http://localhost:5173
```
