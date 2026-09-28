# Hướng Dẫn Sử Dụng Bản Chạy Nhanh (.EXE) - DiroPos 🚀

Bản đóng gói **`DiroPos.exe`** là phiên bản ứng dụng chạy độc lập (Self-Contained), tích hợp trọn gói cả Backend .NET 10, Frontend Vue 3 tông màu sáng sủa hiện đại, Cơ sở dữ liệu SQLite cục bộ và cổng thanh toán mã VietQR NAPAS 247.

> [!IMPORTANT]
> **Ưu điểm vượt trội khi giao cho khách hàng:**
> - **KHÔNG CẦN CÀI ĐẶT MÔI TRƯỜNG PHỨC TẠP:** Máy khách hàng không cần cài Docker, không cần cài .NET SDK, không cần Node.js.
> - **Chỉ cần nhấp đúp chuột** vào file `DiroPos.exe` (hoặc `Chay_Phan_Mem.bat`) là phần mềm tự chạy và tự động mở trình duyệt web.
> - **Chạy trực tiếp từ USB:** Khách hàng chỉ cần giải nén thư mục là dùng ngay trong 5 giây!

---

## 1. Vị Trí Gói Phần Mềm Sau Khi Đóng Gói

Gói phần mềm hoàn chỉnh đã được xuất tại:
```
D:\Pos\publish\
├── DiroPos v2.0.zip               <-- File nén trọn gói gửi cho khách hàng (~53 MB)
└── DiroPos\                       <-- Thư mục chạy trực tiếp
    ├── DiroPos.exe                <-- File chạy chính của phần mềm (Click đúp)
    ├── CongBarberPOS.exe          <-- File chạy dự phòng (Tương thích máy cũ)
    ├── Chay_Phan_Mem.bat          <-- File chạy nhanh bằng Batch script
    ├── appsettings.json           <-- Cấu hình hệ thống
    └── wwwroot/                   <-- Toàn bộ giao diện web Vue 3 mới nhất
```

---

## 2. Hướng Dẫn Bàn Giao Cho Khách Hàng

### Cách 1: Chạy trực tiếp trên máy tính thu ngân / máy chủ quán
1. Gửi file **`DiroPos v2.0.zip`** cho khách hàng (hoặc chép qua USB).
2. Khách hàng giải nén ra thư mục bất kỳ (ví dụ: `C:\DiroPos` hoặc `D:\DiroPos` hoặc `Desktop`).
3. Nhấp đúp chuột vào file **`DiroPos.exe`** (hoặc `Chay_Phan_Mem.bat`).
4. Phần mềm khởi động và tự động mở trình duyệt web:
   - **`http://localhost:5012`**

### Cách 2: Dùng điện thoại (iPhone / Android) hoặc iPad để bán hàng từ xa
Khi máy tính thu ngân đang mở `DiroPos.exe`, bất kỳ điện thoại hay iPad nào cùng kết nối mạng Wifi đều truy cập được:
1. Xem địa chỉ IP của máy tính (ví dụ: `192.168.1.15`).
2. Mở trình duyệt trên điện thoại gõ:
   ```
   http://192.168.1.15:5012
   ```
3. Bấm **"Thêm vào Màn hình chính" (Add to Home Screen)** để sử dụng như một App di động thực thụ!

---

## 3. Tạo Shortcut Tiện Lợi Ngoài Màn Hình Desktop
1. Chuột phải vào file `DiroPos.exe`.
2. Chọn **"Send to"** $\rightarrow$ **"Desktop (create shortcut)"**.
3. Đổi tên shortcut ngoài màn hình thành **"Phần Mềm Bán Hàng DiroPos"**.
4. Hàng ngày chỉ cần bấm vào icon này là hệ thống sẵn sàng phục vụ.

---

## 4. Sao Lưu & Bảo Vệ Dữ Liệu
- Toàn bộ cơ sở dữ liệu được lưu tự động trong file SQLite tại thư mục phần mềm.
- Khách hàng có thể vào tab **"Cài đặt & VietQR"** bấm **"Tải File Sao Lưu (.db)"** cất vào Google Drive định kỳ để an toàn tuyệt đối.
