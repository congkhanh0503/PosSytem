# Hướng Dẫn Sử Dụng Bản Chạy Nhanh (.EXE) - CongBarber POS 💈

Bản đóng gói **`CongBarberPOS.exe`** là phiên bản độc lập (Self-Contained), tích hợp trọn gói cả Backend .NET 10, Frontend Vue 3, Cơ sở dữ liệu SQLite và mã VietQR NAPAS 247.

> [!IMPORTANT]
> **Ưu điểm vượt trội:**
> - **KHÔNG CẦN CÀI ĐẶT GÌ THÊM:** Không cần cài Docker, không cần cài .NET SDK, không cần Node.js.
> - **Chỉ cần click đúp** là phần mềm tự chạy và tự động mở trình duyệt web.
> - **Có thể copy vào USB** mang sang máy tính của tiệm cắt tóc chạy ngay trong 5 giây!

---

## 1. Vị Trí Thư Mục Sau Khi Đóng Gói

Toàn bộ gói ứng dụng nằm tại thư mục:
```
D:\Project\CongBaber\publish\CongBarberPOS\
├── CongBarberPOS.exe          <-- File chạy chính của phần mềm (Click đúp vào đây)
├── Chay_Phan_Mem.bat          <-- File chạy nhanh dự phòng
├── congbarber.db              <-- File cơ sở dữ liệu của tiệm (Chứa hóa đơn, menu, chi tiêu)
├── appsettings.json           <-- Cấu hình hệ thống
└── wwwroot/                   <-- Giao diện web Vue 3
```

---

## 2. Cách Sử Dụng Cho Tiệm Cắt Tóc

### Cách 1: Chạy trực tiếp trên máy tính tiệm
1. Copy toàn bộ thư mục **`CongBarberPOS`** vào máy tính của tiệm (ví dụ để ở ổ `D:\CongBarberPOS` hoặc ngoài màn hình `Desktop`).
2. Nhấp đúp chuột vào file **`CongBarberPOS.exe`** (hoặc `Chay_Phan_Mem.bat`).
3. Phần mềm sẽ khởi động và **tự động mở trình duyệt web** đưa bạn vào thẳng màn hình Bán Hàng POS:
   - **`http://localhost:5012`**
   - Hoặc: **`http://localhost:5012/pos`**

### Cách 2: Dùng điện thoại (iPhone / Android) trong quán để bấm tính tiền
Khi máy tính của tiệm đang bật file `CongBarberPOS.exe`, bất kỳ điện thoại hoặc máy tính bảng nào kết nối cùng mạng Wifi của quán đều có thể vào dùng:
1. Xem địa chỉ IP của máy tính tiệm (ví dụ: `192.168.1.15`).
2. Mở trình duyệt Safari (trên iPhone) hoặc Chrome (trên Android) gõ:
   ```
   http://192.168.1.15:5012
   ```
3. Bấm **"Thêm vào Màn hình chính" (Add to Home Screen)** trên điện thoại $\rightarrow$ Nó sẽ biến thành 1 ứng dụng icon như một App thật trên điện thoại của thợ!

---

## 3. Cách Tạo Shortcut Ngoài Màn Hình Desktop Cho Chủ Tiệm

Để chủ tiệm mở dễ dàng nhất:
1. Chuột phải vào file `CongBarberPOS.exe`.
2. Chọn **"Send to"** $\rightarrow$ **"Desktop (create shortcut)"**.
3. Đổi tên shortcut ngoài màn hình thành **"Phần Mềm Cắt Tóc CongBarber"**.
4. Mỗi ngày mở máy tính lên, chủ tiệm chỉ cần click đúp vào biểu tượng này là bán hàng ngay!

---

## 4. Sao Lưu Dữ Liệu An Toàn

- Toàn bộ dữ liệu của tiệm được lưu trong file `congbarber.db` ngay trong thư mục.
- Bạn có thể vào mục **"Cài đặt & Sao lưu"** trên giao diện để bấm nút **"Tải File Sao Lưu (.db)"** cất vào Google Drive hoặc USB bất cứ lúc nào.
