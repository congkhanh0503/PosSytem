# TÀI LIỆU TOÀN DIỆN VỀ HỆ SINH THÁI DIRO: DIROPOS & DIROADMIN
> **Dành cho các AI Agents & Lập trình viên tiếp nhận dự án.**  
> *Tài liệu này mô tả 100% kiến trúc, tư duy thiết kế, ngăn xếp công nghệ, luồng nghiệp vụ và mối quan hệ giữa **DiroPos** và **DiroAdmin**.*

---

## 1. TỔNG QUAN HỆ SINH THÁI (HIGH-LEVEL OVERVIEW)

**Diro** là hệ sinh thái phần mềm quản lý bán hàng & tài chính tinh gọn, được thiết kế chuyên biệt cho thị trường **cửa hàng dịch vụ & bán lẻ cá nhân/quy mô vừa và nhỏ tại Việt Nam** (tiệm cắt tóc Barber Shop, Salon tóc, tiệm Nail/Mi, Spa mini, tiệm Rửa xe, Studio làm đẹp...).

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
   - Ứng dụng bán hàng trực tiếp tại từng cửa hàng (chạy cục bộ qua bộ cài đặt Desktop `.exe`, Docker hoặc Web Local).
   - Người dùng: Chủ tiệm, thợ cắt tóc, nhân viên thu ngân.
   - Nhiệm vụ: Chạm bán hàng cực nhanh, hiển thị mã VietQR động NAPAS 247, quản lý dịch vụ/sản phẩm, kiểm soát tồn kho 3 lớp, chốt ca cuối ngày (Z-Report), quản lý chi tiêu (OPEX), tự động tính lợi nhuận ròng.
   - **Triết lý thiết kế:** **100% không cần in bill giấy**, không cần đầu tư máy POS đắt tiền, thao tác tối giản cho tiệm 1-2 người.

2. **DiroAdmin (Platform Management / Master Admin)**:
   - Ứng dụng web quản trị trung tâm dành cho người sáng lập / nhà phát triển nền tảng (Platform Owner / Super Admin).
   - Người dùng: Người bán và vận hành phần mềm Diro (Admin hệ thống).
   - Nhiệm vụ: Quản lý danh sách các cửa hàng (Tenants / Stores), quản lý thời hạn thuê bao / bản quyền (License Keys), khóa/mở bản quyền từ xa, theo dõi doanh thu thuê bao toàn hệ thống, quản lý các bản cập nhật phần mềm (`DiroPos.exe` / `DiroPos_Setup_v1.0.0.exe`).

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
  - Cơ sở dữ liệu: SQLite (`diropos.db`).
  - Tài liệu API: Swagger / OpenAPI (`/swagger`).
- **Hình thức đóng gói & triển khai**:
  - **Bộ cài đặt Desktop 1-Click (`DiroPos_Setup_v1.0.0.exe`):** Đóng gói bằng Inno Setup (dung lượng ~35.7 MB). Cài đặt tự động vào `C:\DiroPos\`, tự tạo Desktop Shortcut với Icon chính thức, tự đăng ký khởi động cùng Windows (Startup) và tự mở giao diện bán hàng khi bật máy.
  - **Bản EXE độc lập (Self-Contained Windows Executable):** Xuất ra file `DiroPos.exe` tích hợp sẵn .NET Runtime và thư mục `wwwroot` chứa frontend Vue 3. Khi mở `.exe`, ứng dụng tự động lắng nghe tại `http://localhost:5012/pos` mà không cần cài đặt Node.js hay .NET SDK.
  - **Bản Docker Compose:** Gồm container backend `.NET` và container frontend `nginx:alpine` làm reverse proxy cổng 80 & 5173.
  - **Sử dụng trên di động/máy tính bảng:** Khi máy tính tiệm mở `DiroPos.exe`, điện thoại/iPad kết nối cùng mạng Wifi có thể truy cập `http://<IP_MÁY_TÍNH>:5012` và dùng như App di động.

---

### 2.2. Các Phân Hệ Chức Năng Cốt Lõi Trong DiroPos

#### 1. Màn Hình Bán Hàng Cảm Ứng (Fast Touch POS - `POSView.vue`)
- **Tối ưu hóa thao tác chạm 1 lần:** Chạm vào dịch vụ hoặc sản phẩm là tự động thêm vào giỏ hàng.
- **Phân nhóm Tab rõ ràng:**
  - Tab Dịch vụ: Các dịch vụ cắt tóc, gội đầu, uốn, nhuộm, cạo mặt, spa...
  - Tab Sản phẩm: Sáp vuốt tóc, gôm xịt, tinh dầu, mỹ phẩm bán kèm...
- **Bộ lọc theo danh mục (Categories):** Lọc nhanh theo danh mục dịch vụ và danh mục sản phẩm.
- **Giảm giá linh hoạt (%):** Nút chọn nhanh `0%`, `5%`, `10%`, `15%`, `20%` hoặc tự nhập % tùy ý. Tự động tính số tiền giảm (VNĐ) và trừ trực tiếp vào tổng thanh toán.
- **Ghi chú đơn hàng & Khách hàng:** Lưu thông tin khách hàng (Tên, SĐT), ghi chú yêu cầu riêng của khách.
- **Cơ chế thanh toán kép:**
  - **VietQR (NAPAS 247):** Bấm nút $\rightarrow$ Mở popup hiển thị mã QR động chứa đúng số tiền đã giảm giá. Khách quét app ngân hàng bất kỳ là thanh toán tức thì.
  - **Tiền mặt (Cash):** Bàn phím số thông minh, nút tiền chẵn nhanh (50k, 100k, 200k, 500k), tự động tính số tiền thối lại cho khách.
- **Tự động quản lý kho:** Khi đơn hàng hoàn tất, hệ thống tự động trừ số lượng tồn kho của các sản phẩm có trong đơn.

#### 2. Tích Hợp VietQR Chuẩn NAPAS 247 (`VietQrModal.vue`, `VietQrService.cs`)
- Sử dụng chuẩn QuickLink của VietQR.io: `https://img.vietqr.io/image/<BANK>-<ACC>-<TEMPLATE>.png`.
- Tự động điền: Số tài khoản, Ngân hàng thụ hưởng, Tên chủ tiệm, Số tiền chính xác (sau khi trừ giảm giá), Mã đơn hàng và Nội dung chuyển khoản.
- Không cần cổng thanh toán phức tạp, không mất phí giao dịch trung gian.

#### 3. Quản Lý Menu Dịch Vụ (`ServicesView.vue`, `ServicesController.cs`)
- CRUD Dịch vụ: Tên dịch vụ, Đơn giá, Thời lượng làm (phút), Thuộc danh mục nào, Bật/tắt hiển thị.
- Quản lý danh mục dịch vụ (`ServiceCategoriesController.cs`).

#### 4. Quản Lý Sản Phẩm & Kiểm Soát Tồn Kho 3 Lớp (`ProductsView.vue`, `ProductsController.cs`, `OrdersController.cs`)
- CRUD Sản phẩm: Tên sản phẩm, Mã SKU, Đơn giá bán (`SalePrice`), Giá vốn (`CostPrice`), Số lượng tồn kho (`StockQuantity`), Ngưỡng cảnh báo hết hàng (`LowStockAlert`), Hiển thị trên POS (`ShowOnPos`).
- **Cơ chế chặn bán âm kho nghiêm ngặt ở 3 lớp (3-Layer Stock Protection):**
  - **Lớp 1 (Menu UI):** Khi tồn kho `<= 0`, tự động hiển thị nhãn đỏ *"Hết hàng"*, làm mờ thẻ sản phẩm (`opacity-60 cursor-not-allowed`) và vô hiệu hóa nút thêm vào giỏ.
  - **Lớp 2 (Giỏ hàng & Modal):** Hiển thị rõ số lượng còn lại (`Kho: x`), khóa nút tăng số lượng (`+`) khi chạm trần tồn kho, chặn nút thanh toán nếu có món vượt tồn kho.
  - **Lớp 3 (Backend API):** Kiểm tra `StockQuantity < Quantity` và `StockQuantity <= 0` trong transaction trước khi trừ kho và tạo đơn, ngăn ngừa hoàn toàn tình trạng âm kho do thao tác đồng thời.
- Phân định rõ ràng: Sản phẩm (có trừ kho) và Dịch vụ (không giới hạn kho).

#### 5. Chốt Sổ & Đóng Ca Cuối Ngày (Z-Report - `ShiftCloseModal.vue`, `OrdersController.cs`)
- **Tổng kết dòng tiền trong ca:** Tổng hợp toàn bộ doanh thu phát sinh (tổng doanh thu, tiền mặt đã thu, chuyển khoản VietQR, các khoản chi tiêu trong ca).
- **Đối soát két tiền mặt:** Tự động tính toán số tiền mặt lý thuyết phải có trong ngăn kéo thu ngân:
  $$\text{Tiền mặt trong két} = \text{Tiền mặt thu vào} - \text{Tiền mặt chi ra}$$
- **Bàn giao ca an toàn:** Nhập tên người đóng ca, số tiền lẻ để lại ca sau và ghi chú bàn giao ca.
- **Khóa đơn hàng chống gian lận:** Gọi API `POST /api/orders/close-shift` để gắn cờ `IsLocked = true` và lưu thời điểm `ShiftClosedAt`. Sau khi ca đã đóng, hệ thống **tuyệt đối không cho phép sửa hay hủy bất kỳ đơn hàng nào thuộc ca đó**.

#### 6. Quản Lý Chi Tiêu Tiệm - OPEX (`ExpensesView.vue`, `ExpensesController.cs`)
- Ghi nhận mọi chi phí vận hành cửa hàng theo hình thức tiền mặt hoặc chuyển khoản:
  - *Mặt bằng & Tiện ích* (Tiền thuê mặt bằng, điện, nước, internet, rác...).
  - *Phụ liệu & Hóa chất* (Lưỡi dao lam, bọt cạo râu, thuốc uốn/nhuộm, khăn giấy...).
  - *Dụng cụ & Máy móc* (Kéo, tông đơ, máy sấy, dầu tra máy...).
  - *Sinh hoạt & Ăn uống* (Cơm trưa, cà phê, nước ngọt trong ca làm...).
  - *Marketing & Quảng cáo*.
- Bộ lọc lịch chi tiêu: Hôm nay, Hôm qua, 7 ngày, Tháng này, Tùy chọn ngày.

#### 7. Báo Cáo Doanh Thu, Chi Phí & Lợi Nhuận Ròng (`DashboardView.vue`, `DashboardController.cs`)
- **Tự động tính Lợi Nhuận Ròng (Net Profit):**
  $$\text{Lợi Nhuận Ròng} = \text{Doanh Thu Thuần} - \text{Chi Phí Vận Hành}$$
- **Bộ chuyển đổi tháng nhanh:** Xem tháng này, tháng trước hoặc chọn bất kỳ tháng nào trong quá khứ (`type="month"`).
- **Bảng đối soát tài chính Tháng Này vs Tháng Trước:** So sánh Doanh thu, Chi phí, Lợi nhuận và tính % tăng trưởng tự động.
- **Biến động chi tiêu qua các tháng (Monthly Expense Trend):** Biểu đồ thể hiện chi phí tiệm tăng/giảm qua từng tháng.
- **Bảng kê doanh thu từng ngày (Daily Breakdown):**
  - Liệt kê toàn bộ các ngày trong tháng: Số lượt khách, Tiền dịch vụ, Tiền sản phẩm, Tiền giảm giá, Doanh thu thuần, Chi phí ngày, và Thực lãi ròng của ngày đó.
  - **Modal Chi Tiết Ngày:** Nhấp vào bất kỳ ngày nào để xem toàn bộ danh sách hóa đơn, khách hàng, món đã làm, và chi phí phát sinh của ngày hôm đó.

#### 8. Lịch Sử Đơn Hàng & In Hóa Đơn (`HistoryView.vue`, `OrdersController.cs`)
- Bộ lọc ngày nhanh: Hôm nay, Hôm qua, 7 ngày, Tháng này, Tất cả, Tùy chọn từ ngày - đến ngày.
- Thanh tổng kết tài chính theo khoảng thời gian được lọc (Số đơn, Tổng giảm giá, Doanh thu thực).
- Hỗ trợ xem chi tiết đơn hàng, in hóa đơn (nếu khách yêu cầu) và hủy đơn (chỉ với đơn chưa bị khóa chốt ca; tự động hoàn trả số lượng vào tồn kho).

#### 9. Cài Đặt Hệ Thống & Bản Quyền (`SettingsView.vue`, `LicenseController.cs`)
- Cấu hình tài khoản nhận tiền VietQR (Ngân hàng, Số tài khoản, Tên chủ thẻ, Mẫu hiển thị QR).
- Cấu hình thông tin cửa hàng (Tên tiệm, Hotline, Địa chỉ, Slogan).
- Xem trước mã VietQR động trực quan ngay tại màn hình cài đặt.
- **Quản lý Giấy phép bản quyền (License):** Hiển thị mã tiệm (`ShopCode`), gói cước (`PlanType`), thời hạn sử dụng (`ExpiresAt`), trạng thái kích hoạt và ô nhập License Key để gia hạn.
- **Lưu ý về Sao lưu:** Để đảm bảo tính toàn vẹn dữ liệu thương mại và bản quyền, tính năng tự tải/restore file `.db` thủ công trên giao diện POS đã được lược bỏ. Dữ liệu được bảo vệ an toàn và điều phối sao lưu tập trung qua DiroAdmin hoặc công cụ quản trị.

---

## 3. CHI TIẾT PHÂN HỆ DIROADMIN (PLATFORM MASTER ADMIN)

### 3.1. Mục Tiêu Của DiroAdmin
DiroAdmin là cổng vận hành trung tâm dành cho người quản lý nền tảng Diro nhằm giải quyết bài toán:
- **Quản lý tập trung hàng trăm tiệm đang sử dụng DiroPos.**
- **Thu phí thuê bao định kỳ (ví dụ: 50k/tháng hoặc 500k/năm) và khóa/mở bản quyền tức thì khi khách hết hạn.**
- **Cấp mã kích hoạt (License Key) nhanh chóng cho khách hàng mới.**

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
│ - Giám sát Online│ - Gói Trọn đời       │                              │
└──────────────────┴──────────────────────┴──────────────────────────────┘
```

1. **Quản Lý Cửa Hàng (Store & Tenant Management)**:
   - Danh sách tất cả các cửa hàng trong hệ sinh thái:
     - Tên cửa hàng (Shop Name), Mã tiệm (Shop Code).
     - Chủ sở hữu (Owner Name) & Số điện thoại / Zalo.
     - Địa chỉ / Khu vực kinh doanh.
     - Trạng thái: `Active` (Đang hoạt động), `Expired` (Hết hạn), `Suspended` (Tạm khóa).
   - Nút **Khóa / Mở Khóa 🔒** ngay trên danh sách: Khi bị khóa, máy POS của tiệm sẽ lập tức bị phong tỏa màn hình, chặn tạo đơn hàng.

2. **Quản Lý Bản Quyền & Thuê Bao (License & Subscription Management)**:
   - Cơ chế cấp License Key tự sinh duy nhất cho từng tiệm theo gói:
     - **Gói Trial:** Dùng thử 7 – 15 ngày.
     - **Gói 6 Tháng:** 300.000 ₫.
     - **Gói 1 Năm:** 500.000 ₫ / năm.
     - **Gói Trọn Đời (Lifetime):** Không thu phí duy trì.
   - Cảnh báo tiệm sắp hết hạn trong vòng 7 ngày để Admin chủ động liên hệ chăm sóc và nhắc gia hạn.
   - Nút gia hạn nhanh (+1 tháng, +6 tháng, +1 năm).

3. **Cổng Kết Nối & Vận Hành**:
   - DiroAdmin Web UI: Lắng nghe tại cổng `http://localhost:8080`.
   - DiroAdmin API Backend: Lắng nghe tại cổng `http://localhost:5020/swagger`.

---

## 4. MỐI QUAN HỆ & LUỒNG TƯƠNG TÁC GIỮA DIROPOS VÀ DIROADMIN

```
 [Khách Hàng / Tiệm Mới]                   [Admin / DiroAdmin]
           │                                        │
           │  1. Đăng ký mua / dùng thử             │
           │───────────────────────────────────────>│
           │                                        │
           │                                        │ 2. Tạo Store trên DiroAdmin
           │                                        │    Cấp License Key / Hạn dùng
           │  3. Nhận bản cài DiroPos_Setup + Key   │
           │<───────────────────────────────────────│
           │                                        │
           │ 4. Chạy cài đặt 1-click & mở POS       │
           │    Vào Cài đặt -> Nhập License Key     │
           │                                        │
           │ 5. Xác thực bản quyền (License API)    │
           │───────────────────────────────────────>│
           │<───────────────────────────────────────│
           │                                        │
           │ 6. Vận hành bán hàng hàng ngày         │
           │    (Lưu trữ SQLite diropos.db cục bộ)  │
           │                                        │
           │ 7. Khóa / Mở khóa từ xa khi cần        │
           │<───────────────────────────────────────│
```

### Kiến Trúc Dữ Liệu Hybrid (Cục Bộ + Đám Mây):
- **Tại sao DiroPos dùng SQLite cục bộ (`diropos.db`)?**
  - Giúp cửa hàng bán hàng bình thường **ngay cả khi mất mạng Internet**.
  - Tốc độ tải dữ liệu tức thì (0ms latency), không phụ thuộc đường truyền mạng.
  - Dữ liệu hóa đơn và tài chính thuộc quyền sở hữu riêng tư của chủ tiệm.
- **DiroAdmin đóng vai trò gì?**
  - Là trung tâm xác thực bản quyền (License Server), quản lý thông tin khách hàng, chăm sóc khách hàng và thu tiền thuê bao định kỳ.

---

## 5. CƠ SỞ DỮ LIỆU CỐT LÕI (DATABASE SCHEMA - DIROPOS)

Cơ sở dữ liệu SQLite chính thức: **`diropos.db`** (Định nghĩa tại `AppDbContext.cs` & `Entities.cs`):

| Bảng (Table) | Mục đích | Các trường quan trọng |
| :--- | :--- | :--- |
| **`ShopSettings`** | Thông tin tiệm & Cấu hình VietQR | `Id`, `ShopName`, `Address`, `Phone`, `Slogan`, `BankId`, `BankName`, `AccountNo`, `AccountName`, `QrTemplate` |
| **`ServiceCategories`** | Danh mục dịch vụ | `Id`, `Name`, `Color`, `CreatedAt` |
| **`Services`** | Chi tiết dịch vụ cắt/uốn/spa... | `Id`, `Name`, `Price`, `DurationMinutes`, `Category`, `Description`, `IsActive`, `CreatedAt` |
| **`ProductCategories`** | Danh mục sản phẩm | `Id`, `Name`, `Color`, `CreatedAt` |
| **`Products`** | Sản phẩm sáp/gôm bán lẻ | `Id`, `Name`, `Sku`, `CostPrice`, `SalePrice`, `StockQuantity`, `LowStockAlert`, `Category`, `ShowOnPos`, `IsActive`, `CreatedAt` |
| **`Orders`** | Hóa đơn bán hàng | `Id`, `OrderCode`, `CustomerName`, `CustomerPhone`, `SubTotal`, `DiscountPercent`, `DiscountAmount`, `FinalAmount`, `PaymentMethod` (VietQR/Cash), `PaymentStatus`, `Note`, **`IsLocked`** (Khóa chốt ca), **`ShiftClosedAt`**, `CreatedAt` |
| **`OrderItems`** | Chi tiết từng món trong hóa đơn | `Id`, `OrderId`, `ItemType` (Service/Product), `ServiceId`, `ProductId`, `ItemName`, `Quantity`, `UnitPrice`, `TotalPrice` |
| **`Expenses`** | Chi phí vận hành cửa hàng (OPEX) | `Id`, `Title`, `Amount`, `Category`, `PaymentMethod` (Cash/Transfer), `Date`, `Note`, `CreatedAt` |
| **`SystemLicenses`** | Thông tin bản quyền & Kích hoạt | `Id`, `ShopCode`, `ShopName`, `LicenseKey`, `PlanType`, `ActivatedAt`, `ExpiresAt`, `Status` (Active/Expired/Suspended), `HardwareId`, `ContactPhone`, `LastCheckedAt`, `IsInitialized` |

---

## 6. HƯỚNG DẪN DÀNH CHO CÁC AI AGENTS KHI PHÁT TRIỂN TIẾP

Khi làm việc trên dự án này, mọi AI cần tuân thủ nghiêm ngặt các quy tắc sau:

1. **Ngôn ngữ:** Toàn bộ giao diện người dùng, thông báo lỗi, toast message và tài liệu **bắt buộc dùng 100% tiếng Việt**.
2. **Triết lý sản phẩm:**
   - **Không in bill giấy:** Mọi tính năng phải xoay quanh hóa đơn điện tử và thanh toán mã VietQR.
   - **Tối giản thao tác:** Càng ít click càng tốt. Giao diện to rõ, nhạy cảm ứng.
   - **Màu sắc:** Tuân thủ hệ màu Diro Brand:
     - Màu thương hiệu chính: Diro Indigo (`#4f46e5`, `bg-indigo-600`).
     - Doanh thu / Lợi nhuận dương: Emerald (`#10b981`, `text-emerald-400`).
     - Chi phí / Lợi nhuận âm: Rose (`#f43f5e`, `text-rose-400`).
     - Điểm nhấn tiền tệ / POS: Amber / Gold (`#f59e0b`).
3. **Cơ chế đóng gói & Đường dẫn Database:**
   - Database chuẩn duy nhất là **`diropos.db`**. Luôn đảm bảo resolve đường dẫn thông qua `AppDomain.CurrentDomain.BaseDirectory` hoặc biến môi trường `DB_PATH`.
   - Bộ cài đặt chuẩn được đóng gói bằng lệnh: `powershell -ExecutionPolicy Bypass -File .\build_installer.ps1` ra file `DiroPos_Setup_v1.0.0.exe`.
4. **Kiểm soát tính toàn vẹn dữ liệu:**
   - **Kho hàng:** Tuyệt đối không cho phép tạo đơn hoặc tăng số lượng vượt quá `StockQuantity`. Chặn ở cả UI, Store và Transaction Backend.
   - **Chốt ca:** Các đơn hàng đã có cờ `IsLocked = true` tuyệt đối không được phép chỉnh sửa hoặc hủy.
5. **Phân tách trách nhiệm:**
   - **DiroPos:** Tập trung trải nghiệm thu ngân, tính tiền, kiểm kho, chốt ca, chi tiêu của 1 cửa hàng cụ thể.
   - **DiroAdmin:** Tập trung quản trị hệ thống, cấp bản quyền, quản lý danh sách tiệm và thu tiền SaaS.
