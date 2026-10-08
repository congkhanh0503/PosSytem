# TÀI LIỆU TOÀN DIỆN VỀ HỆ SINH THÁI DIRO: DIROPOS & DIROADMIN
> **Dành cho các AI Agents & Lập trình viên tiếp nhận dự án.**  
> *Tài liệu này mô tả 100% kiến trúc, tư duy thiết kế, ngăn xếp công nghệ, luồng nghiệp vụ và mối quan hệ giữa **DiroPos** và **DiroAdmin**.*

---

## 1. TỔNG QUAN HỆ SINH THÁI (HIGH-LEVEL OVERVIEW)

**Diro** là hệ sinh thái phần mềm quản lý bán hàng & tài chính tinh gọn, được thiết kế chuyên biệt cho thị trường **cửa hàng dịch vụ & bán lẻ cá nhân/quy mô nhỏ tại Việt Nam** (tiệm cắt tóc Barber Shop, Salon tóc, tiệm Nail/Mi, Spa mini, tiệm Rửa xe, Studio làm đẹp...).

Hệ sinh thái gồm **2 mảnh ghép cốt lõi**:

```
                              ┌────────────────────────────────────────┐
                              │               DIROADMIN                │
                              │    (Hệ Thống Quản Trị Trung Tâm)       │
                              │   Platform Owner / SaaS Master Admin   │
                              └──────────────────┬─────────────────────┘
                                                 │
                  ┌──────────────────────────────┼──────────────────────────────┐
                  │ Quản lý bản quyền / Thuê bao │ Cấp phép / Setup cửa hàng    │
                  ▼                              ▼                              ▼
     ┌────────────────────────┐    ┌────────────────────────┐    ┌────────────────────────┐
     │      DIROPOS #01       │    │      DIROPOS #02       │    │      DIROPOS #N        │
     │ (Tiệm Barber Shop A)   │    │  (Tiệm Nail & Mi B)    │    │ (Tiệm Cắt Tóc Quê C)   │
     │  Client POS / Desktop  │    │  Client POS / Desktop  │    │  Client POS / Desktop  │
     └────────────────────────┘    └────────────────────────┘    └────────────────────────┘
```

1. **DiroPos (Client Storefront / POS App)**:
   - Ứng dụng bán hàng trực tiếp tại từng cửa hàng (chạy cục bộ qua file `.exe`, Docker hoặc Web Local).
   - Người dùng: Chủ tiệm, thợ cắt tóc, nhân viên thu ngân.
   - Nhiệm vụ: Chạm bán hàng cực nhanh, hiển thị mã VietQR động NAPAS 247, quản lý dịch vụ/sản phẩm, quản lý chi tiêu (OPEX), tự động tính lợi nhuận ròng, sao lưu dữ liệu SQLite.
   - **Triết lý thiết kế:** **100% không cần in bill giấy**, không cần mua máy POS đắt tiền, thao tác tối giản cho tiệm 1-2 người.

2. **DiroAdmin (Platform Management / Master Admin)**:
   - Ứng dụng web quản trị trung tâm dành cho người sáng lập / nhà phát triển nền tảng (Platform Owner / Super Admin).
   - Người dùng: Người bán phần mềm Diro (Admin hệ thống).
   - Nhiệm vụ: Quản lý danh sách các cửa hàng (Tenants / Stores), quản lý thời hạn thuê bao / bản quyền (License Keys), kích hoạt cửa hàng mới (`InitialSetupModal`), theo dõi doanh thu thuê bao toàn hệ thống, quản lý các bản cập nhật phần mềm (`DiroPos.exe`).

---

## 2. CHI TIẾT PHÂN HỆ DIROPOS (STORE POS APPLICATION)

### 2.1. Ngăn Xếp Công Nghệ (Tech Stack)
- **Frontend**:
  - Framework: Vue 3 (Composition API với `<script setup>`).
  - Build tool: Vite.
  - Styling: Tailwind CSS (Tông màu chủ đạo: Diro Indigo `#4f46e5`, nền sáng hiện đại, kết hợp các điểm nhấn Emerald cho doanh thu/lợi nhuận và Rose cho chi phí).
  - Icons: `lucide-vue-next`.
  - HTTP Client: Axios (cấu hình tại `src/services/api.js`).
  - Quản lý trạng thái: Pinia (`src/stores/posStore.js`).
  - Biểu đồ: Chart.js & `vue-chartjs` (biểu đồ doanh thu 7 ngày, biến động chi tiêu qua các tháng).
- **Backend**:
  - Nền tảng: .NET 10 Web API (C#).
  - ORM: Entity Framework Core 10.
  - Cơ sở dữ liệu: SQLite (`congbarber.db` / `diropos.db`).
  - Tài liệu API: Swagger / OpenAPI (`/swagger`).
- **Hình thức đóng gói & triển khai**:
  - **Bản EXE độc lập (Self-Contained Windows Executable):** Xuất ra file `DiroPos.exe` (~113 MB) tích hợp sẵn .NET Runtime và thư mục `wwwroot` chứa frontend Vue 3. Khi nhấp đúp vào file `.exe`, ứng dụng tự động mở trình duyệt web `http://localhost:5012/pos` mà không cần cài đặt Docker, Node.js hay .NET SDK.
  - **Bản Docker Compose:** Gồm container backend `.NET` và container frontend `nginx:alpine` làm reverse proxy cổng 80 & 5173.
  - **Sử dụng trên di động:** Khi máy tính tiệm mở `DiroPos.exe`, điện thoại/iPad kết nối cùng Wifi có thể truy cập `http://<IP_MÁY_TÍNH>:5012` và dùng như App di động.

---

### 2.2. Các Phân Hệ Chức Năng Cốt Lõi Trong DiroPos

#### 1. Màn Hình Bán Hàng Cảm Ứng (Fast Touch POS - `POSView.vue`)
- **Tối ưu hóa thao tác chạm 1 lần:** Chạm vào dịch vụ hoặc sản phẩm là tự động thêm vào giỏ hàng.
- **Phân nhóm Tab rõ ràng:**
  - Tab Dịch vụ: Các dịch vụ cắt tóc, gội đầu, uốn, nhuộm, cạo mặt...
  - Tab Sản phẩm: Sáp vuốt tóc, gôm xịt, tinh dầu, mỹ phẩm bán thêm...
- **Bộ lọc theo danh mục (Categories):** Lọc nhanh danh mục dịch vụ và danh mục sản phẩm.
- **Giảm giá linh hoạt (%):** Nút chọn nhanh `0%`, `5%`, `10%`, `15%`, `20%` hoặc tự nhập % tùy ý. Tự động tính số tiền giảm (VNĐ) và trừ trực tiếp vào tổng thanh toán.
- **Ghi chú đơn hàng (Note):** Cho phép ghi chú yêu cầu riêng của khách (ví dụ: *"Khách quen anh Nam - uốn tóc tuần sau"*, lưu ý bảo hành...).
- **Cơ chế thanh toán kép:**
  - **VietQR (NAPAS 247):** Bấm nút $\rightarrow$ Mở popup hiển thị mã QR động chứa đúng số tiền đã giảm giá. Khách quét app ngân hàng bất kỳ là thanh toán tức thì.
  - **Tiền mặt (Cash):** Bấm thanh toán là hoàn tất đơn ngay lập tức.
- **Tự động quản lý kho:** Khi đơn hàng hoàn tất, hệ thống tự động trừ số lượng tồn kho của các sản phẩm có trong đơn.

#### 2. Tích Hợp VietQR Chuẩn NAPAS 247 (`VietQrModal.vue`, `VietQrService.cs`)
- Sử dụng chuẩn QuickLink của VietQR.io: `https://img.vietqr.io/image/<BANK>-<ACC>-<TEMPLATE>.png`.
- Tự động điền: Số tài khoản, Ngân hàng thụ hưởng, Tên chủ tiệm, Số tiền chính xác (sau khi trừ giảm giá), Mã đơn hàng và Nội dung chuyển khoản.
- Không cần cổng thanh toán phức tạp, không mất phí trung gian cổng thanh toán.

#### 3. Quản Lý Menu Dịch Vụ (`ServicesView.vue`, `ServicesController.cs`)
- CRUD Dịch vụ: Tên dịch vụ, Đơn giá, Thời lượng làm (phút), Thuộc danh mục nào, Bật/tắt hiển thị.
- Quản lý danh mục dịch vụ (`ServiceCategoriesController.cs`).

#### 4. Quản Lý Sản Phẩm & Kiểm Soát Tồn Kho 3 Lớp (`ProductsView.vue`, `ProductsController.cs`)
- CRUD Sản phẩm: Tên sản phẩm, Đơn giá bán, Giá vốn (nhập), Số lượng tồn kho, Ngưỡng cảnh báo hết hàng.
- **Cơ chế chặn bán âm kho nghiêm ngặt ở 3 lớp (3-Layer Stock Protection):**
  - **Lớp 1 (Menu UI):** Khi tồn kho `= 0`, tự động hiển thị nhãn *"Hết hàng"*, làm mờ thẻ sản phẩm và vô hiệu hóa nút thêm vào giỏ.
  - **Lớp 2 (Giỏ hàng & Modal):** Hiển thị rõ số lượng còn lại (`Kho: x`), khóa nút tăng số lượng khi chạm trần tồn kho, chặn nút thanh toán nếu có món vượt tồn kho.
  - **Lớp 3 (Backend API):** Kiểm tra `Product.Stock >= Quantity` trong transaction trước khi trừ kho và tạo đơn, ngăn ngừa hoàn toàn tình trạng âm kho do bấm nhanh hoặc thao tác đồng thời.
- Nút tăng/giảm kho nhanh (+1 / -1) giúp kiểm kho tại quầy cực kỳ nhanh gọn.
- Phân định rõ ràng: Sản phẩm (có trừ kho) và Dịch vụ (không giới hạn kho).

#### 5. Quản Lý Ca Làm Việc Chặt Chẽ (Shift Management)
- **Mở ca (Open Shift):** Khai báo số tiền mặt ban đầu tại ngăn kéo thu ngân khi nhân viên bắt đầu ca làm việc.
- **Vận hành trong ca:** Theo dõi toàn bộ dòng tiền phát sinh theo ca (tiền mặt thu vào, chuyển khoản VietQR, tiền chi ra cho sinh hoạt/phụ liệu).
- **Tổng kết & Đóng ca (Close Shift):** Khai báo số tiền mặt thực tế khi kết thúc ca, đối soát doanh thu lý thuyết vs thực tế, tính toán chênh lệch (thừa/thiếu tiền).
- **Chống gian lận thu ngân:** Tự động khóa tính năng chỉnh sửa/hủy đơn hàng của các ca đã đóng.

#### 6. Quản Lý Chi Tiêu Tiệm - OPEX (`ExpensesView.vue`, `ExpensesController.cs`)
- Ghi nhận mọi chi phí vận hành cửa hàng:
  - *Mặt bằng & Tiện ích* (Tiền thuê mặt bằng, điện, nước, internet, rác...).
  - *Phụ liệu & Hóa chất* (Lưỡi dao lam, bọt cạo râu, thuốc uốn/nhuộm, khăn giấy...).
  - *Dụng cụ & Máy móc* (Kéo, tông đơ, máy sấy, dầu tra máy...).
  - *Sinh hoạt & Ăn uống* (Cơm trưa, cà phê, nước ngọt trong ca làm...).
  - *Marketing & Quảng cáo*.
- Bộ lọc lịch chi tiêu: Hôm nay, Hôm qua, 7 ngày, Tháng này, Tùy chọn ngày.
- Thống kê tổng chi hôm nay, tổng chi tháng này.

#### 7. Báo Cáo Doanh Thu, Chi Phí & Lợi Nhuận Ròng (`DashboardView.vue`, `DashboardController.cs`)
- **Tự động tính Lợi Nhuận Ròng (Net Profit):**
  $$\text{Lợi Nhuận Ròng} = \text{Doanh Thu Thuần} - \text{Chi Phí Vận Hành}$$
- **Bộ chuyển đổi tháng nhanh:** Xem tháng này, tháng trước hoặc chọn bất kỳ tháng nào trong quá khứ (`type="month"`).
- **Bảng đối soát tài chính Tháng Này vs Tháng Trước:** So sánh Doanh thu, Chi phí, Lợi nhuận và tính % tăng trưởng tự động.
- **Biến động chi tiêu qua các tháng (Monthly Expense Trend):** Biểu đồ thể hiện chi phí tiệm tăng/giảm qua từng tháng.
- **BẢNG KÊ DOANH THU TỪNG NGÀY (DAILY BREAKDOWN):**
  - Liệt kê toàn bộ các ngày trong tháng với đầy đủ: Số lượt khách, Tiền dịch vụ, Tiền sản phẩm, Tiền giảm giá, Doanh thu thuần, Chi phí ngày, và Thực lãi ròng của ngày đó.
  - Bộ lọc: "Chỉ ngày có đơn" hoặc "Tất cả các ngày".
  - **Modal Chi Tiết Ngày:** Nhấp vào bất kỳ ngày nào để xem toàn bộ danh sách hóa đơn, khách hàng, món đã làm, và chi phí phát sinh của ngày hôm đó.

#### 7. Lịch Sử Đơn Hàng & Đối Soát (`OrdersView.vue`, `OrdersController.cs`)
- Bộ lọc ngày nhanh: Hôm nay, Hôm qua, 7 ngày, Tháng này, Tất cả, Tùy chọn từ ngày - đến ngày.
- Thanh tổng kết tài chính theo khoảng thời gian được lọc (Số đơn, Tổng giảm giá, Doanh thu thực).
- Hỗ trợ xem chi tiết đơn hàng và hủy đơn (tự động hoàn trả số lượng vào tồn kho).

#### 8. Sao Lưu & Khôi Phục Dữ Liệu (`SettingsView.vue`, `BackupController.cs`)
- **Tải Bản Sao Lưu (`.db`):** Sử dụng API `SqliteConnection.BackupDatabase` tạo snapshot nhị phân tức thì không làm khóa database, tải về máy tính với định dạng ngày giờ chuẩn (`diropos_backup_YYYYMMDD_HHmmss.db`).
- **Khôi Phục Dữ Liệu (Restore):** Nhận file `.db`, xác thực chữ ký SQLite (`SQLite format 3`), tự động tạo file dự phòng `.bak` trước khi ghi đè để an toàn tuyệt đối.

---

## 3. CHI TIẾT PHÂN HỆ DIROADMIN (PLATFORM MASTER ADMIN)

### 3.1. Mục Tiêu Của DiroAdmin
DiroAdmin là cổng vận hành trung tâm dành cho người quản lý nền tảng Diro nhằm giải quyết bài toán:
- **Làm thế nào để quản lý hàng chục / hàng trăm tiệm đang sử dụng DiroPos?**
- **Làm thế nào để thu tiền thuê bao (50k/tháng hoặc 500k/năm) và khóa/mở bản quyền khi khách hết hạn?**
- **Làm thế nào để cấp mới một cửa hàng nhanh chóng?**

---

### 3.2. Các Chức Năng Cốt Lõi Của DiroAdmin

```
┌────────────────────────────────────────────────────────────────────────┐
│                        DIROADMIN DASHBOARD                             │
├──────────────────┬──────────────────────┬──────────────────────────────┤
│ 🏬 QUẢN LÝ TIỆM   │ 🔑 BẢN QUYỀN / GÓI   │ 📊 THỐNG KÊ DOANH THU THUÊ BAO│
│ - Danh sách tiệm │ - Gói Dùng thử       │ - Tổng thuê bao active       │
│ - Thêm tiệm mới  │ - Gói 6 tháng (300k) │ - Tiệm sắp hết hạn (7 ngày)  │
│ - Kích hoạt/Khóa │ - Gói 1 năm (500k)   │ - Doanh thu nền tảng         │
│ - Reset dữ liệu  │ - Gói Trọn đời       │                              │
└──────────────────┴──────────────────────┴──────────────────────────────┘
```

1. **Quản Lý Cửa Hàng (Store & Tenant Management)**:
   - Danh sách tất cả các cửa hàng trong hệ sinh thái:
     - Tên cửa hàng (Shop Name).
     - Chủ sở hữu (Owner Name) & Số điện thoại / Zalo.
     - Địa chỉ / Khu vực (Huyện, Tỉnh).
     - Loại hình: Barber Shop, Salon Tóc, Tiệm Nail, Rửa xe...
     - Trạng thái: `Active` (Đang hoạt động), `Expired` (Hết hạn), `Pending` (Chờ kích hoạt), `Suspended` (Tạm khóa).
   - Form tạo cửa hàng mới hoặc liên kết với cửa hàng vừa cài đặt (`InitialSetupModal.vue`).

2. **Quản Lý Bản Quyền & Thuê Bao (License & Subscription Management)**:
   - Cơ chế cấp License Key hoặc quản lý hạn dùng theo ngày (`ExpiryDate`).
   - Các gói dịch vụ:
     - **Gói Trial:** Dùng thử miễn phí 7 – 15 ngày.
     - **Gói 6 Tháng:** 300.000 ₫.
     - **Gói 1 Năm:** 500.000 ₫ / năm (~41.000 ₫/tháng).
     - **Gói Trọn Đời (Lifetime):** 2.500.000 ₫ – 3.500.000 ₫ (không thu phí duy trì).
   - Tự động cảnh báo: Danh sách các cửa hàng sắp hết hạn trong 7 ngày tới để Admin chủ động nhắn tin Zalo nhắc gia hạn.
   - Nút Gia Hạn Nhanh (+1 tháng, +6 tháng, +1 năm).

3. **Thống Kê Doanh Thu Nền Tảng (SaaS Business Metrics)**:
   - Tổng số cửa hàng đang hoạt động (Active Tenants).
   - Tỷ lệ gia hạn thuê bao (Renewal Rate).
   - Dòng tiền định kỳ (Monthly Recurring Revenue - MRR và Annual Recurring Revenue - ARR).

4. **Quản Lý Cập Nhật Phần Mềm (Release Management)**:
   - Quản lý phiên bản ứng dụng DiroPos (`v1.0`, `v2.0`...).
   - Đường dẫn tải bộ cài đặt mới nhất (`DiroPos.exe` hoặc file zip).
   - Gửi thông báo cập nhật tính năng mới đến các cửa hàng.

---

## 4. MỐI QUAN HỆ & LUỒNG TƯƠNG TÁC GIỮA DIROPOS VÀ DIROADMIN

```
 [Khách Hàng / Tiệm Mới]                   [Admin / DiroAdmin]
           │                                        │
           │  1. Đăng ký / Mua phần mềm             │
           │───────────────────────────────────────>│
           │                                        │
           │                                        │ 2. Tạo Store trên DiroAdmin
           │                                        │    Cấp License Key / Hạn dùng
           │  3. Nhận bản cài DiroPos.exe + Key     │
           │<───────────────────────────────────────│
           │                                        │
           │ 4. Mở DiroPos.exe                      │
           │    Nhập thông tin tiệm & VietQR        │
           │    (InitialSetupModal)                 │
           │                                        │
           │ 5. Kiểm tra bản quyền (Online / Token) │
           │───────────────────────────────────────>│
           │<───────────────────────────────────────│
           │                                        │
           │ 6. Vận hành bán hàng hàng ngày         │
           │    (Lưu trữ SQLite cục bộ tại tiệm)    │
           │                                        │
           │ 7. Nhắc gia hạn khi sắp hết hạn        │
           │<───────────────────────────────────────│
```

### Kiến Trúc Dữ Liệu Hybrid (Cục Bộ + Đám Mây):
- **Tại sao DiroPos dùng SQLite cục bộ?**
  - Giúp cửa hàng bán hàng **ngay cả khi mất mạng Internet**.
  - Tốc độ tải dữ liệu tức thì (0ms latency).
  - Dữ liệu tài chính thuộc sở hữu 100% của chủ tiệm, bảo mật tuyệt đối.
- **DiroAdmin đóng vai trò gì?**
  - Là trung tâm xác thực bản quyền (License Server), quản lý thông tin khách hàng, chăm sóc khách hàng và thu tiền thuê bao định kỳ.

---

## 5. CƠ SỞ DỮ LIỆU CỐT LÕI (DATABASE SCHEMA - DIROPOS)

| Bảng (Table) | Mục đích | Các trường quan trọng |
| :--- | :--- | :--- |
| **`ShopSettings`** | Thông tin tiệm & Cấu hình VietQR | `ShopName`, `Address`, `Phone`, `BankId`, `AccountNo`, `AccountName`, `QrTemplate` |
| **`ServiceCategories`** | Phân loại danh mục dịch vụ | `Id`, `Name`, `DisplayOrder`, `IsActive` |
| **`Services`** | Dịch vụ cắt/uốn/nhuộm... | `Id`, `CategoryId`, `Name`, `Price`, `DurationMinutes`, `IsActive` |
| **`ProductCategories`** | Phân loại danh mục sản phẩm | `Id`, `Name`, `DisplayOrder`, `IsActive` |
| **`Products`** | Sản phẩm sáp/gôm bán lẻ | `Id`, `CategoryId`, `Name`, `Price`, `CostPrice`, `Stock`, `LowStockThreshold`, `IsActive` |
| **`Orders`** | Hóa đơn bán hàng | `Id`, `OrderCode`, `CustomerName`, `CustomerPhone`, `SubTotal`, `DiscountPercent`, `DiscountAmount`, `FinalAmount`, `PaymentMethod` (VietQR/Cash), `PaymentStatus`, `Note`, `CreatedAt` |
| **`OrderItems`** | Chi tiết từng món trong hóa đơn | `Id`, `OrderId`, `ItemType` (Service/Product), `ServiceId`, `ProductId`, `ItemName`, `Quantity`, `UnitPrice`, `TotalPrice` |
| **`Expenses`** | Chi phí vận hành cửa hàng | `Id`, `Title`, `Amount`, `Category`, `Date`, `Note`, `CreatedAt` |

---

## 6. HƯỚNG DẪN DÀNH CHO CÁC AI AGENTS KHI PHÁT TRIỂN TIẾP

Khi làm việc trên dự án này, mọi AI cần tuân thủ các quy tắc sau:

1. **Ngôn ngữ:** Toàn bộ giao diện người dùng, thông báo lỗi, toast message và tài liệu **bắt buộc dùng 100% tiếng Việt**.
2. **Triết lý sản phẩm:**
   - **Không in bill giấy:** Mọi tính năng phải xoay quanh hóa đơn điện tử và thanh toán mã VietQR.
   - **Tối giản thao tác:** Càng ít click càng tốt. Chủ tiệm là người vừa cắt tóc vừa bấm máy, giao diện phải to rõ, nhạy cảm ứng.
   - **Màu sắc:** Tuân thủ hệ màu Diro Brand:
     - Màu thương hiệu chính: Diro Indigo (`#4f46e5`, `bg-indigo-600`).
     - Doanh thu / Lợi nhuận dương: Emerald (`#10b981`, `text-emerald-400`).
     - Chi phí / Lợi nhuận âm: Rose (`#f43f5e`, `text-rose-400`).
     - Điểm nhấn tiền tệ / POS: Amber / Gold (`#f59e0b`).
3. **Cơ chế đóng gói Windows:**
   - Mã nguồn backend .NET có thể publish ra `DiroPos.exe` chạy độc lập với `wwwroot` nhúng kèm.
   - Đường dẫn database luôn dùng `Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "congbarber.db")` để tránh lỗi sai thư mục khi chạy shortcut ngoài màn hình Desktop.
4. **Phân tách trách nhiệm:**
   - **DiroPos:** Tập trung trải nghiệm thu ngân, tính tiền, kiểm kho, chi tiêu của 1 cửa hàng cụ thể.
   - **DiroAdmin:** Tập trung quản trị hệ thống, cấp bản quyền, quản lý danh sách tiệm và thu tiền SaaS.
