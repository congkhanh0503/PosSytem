# Hướng Dẫn Triển Khai Docker - DiroPos 🚀

Dự án **DiroPos** đã được đóng gói hoàn chỉnh bằng **Docker** & **Docker Compose**, giúp bạn triển khai ứng dụng chỉ với **1 câu lệnh duy nhất** trên bất kỳ máy chủ hoặc máy tính nào (Windows, macOS, Linux).

---

## 1. Cấu Trúc Đóng Gói Docker

```
PosSytem/
├── docker-compose.yml              # Quản lý và liên kết các container
├── backend/
│   └── DiroPos.Api/
│       ├── Dockerfile              # Build .NET 10 Web API & SQLite
│       └── .dockerignore           # Loại trừ các file rác khi build
└── frontend/
    ├── Dockerfile                  # Build Vue 3 & đóng gói Nginx Alpine
    ├── nginx.conf                  # Cấu hình Nginx Reverse Proxy & nén Gzip
    └── .dockerignore               # Loại trừ node_modules khi build
```

---

## 2. Các Dịch Vụ Trong Hệ Thống Container

| Dịch vụ | Công nghệ | Cổng Container | Cổng Host | Chức năng |
| :--- | :--- | :--- | :--- | :--- |
| **`diropos-frontend`** | Nginx Alpine + Vue 3 SPA | `80` | **`80`** & **`5173`** | Giao diện bán hàng POS, Dashboard, tự động nén Gzip và chuyển tiếp request `/api/` |
| **`diropos-backend`** | .NET 10 Web API + SQLite | `5012` | **`5012`** | Xử lý đơn hàng, chi tiêu, xuất mã VietQR NAPAS 247 và API backup |

> [!TIP]
> **Bảo toàn dữ liệu bán hàng:** File cơ sở dữ liệu `diropos.db` được mount trực tiếp qua Docker Named Volume `diropos-data`. Khi bạn tắt, restart hoặc cập nhật container mới, toàn bộ dữ liệu đơn hàng và chi tiêu **hoàn toàn không bị mất**.

---

## 3. Các Lệnh Thao Tác Docker Thường Dùng

### Khởi Động Toàn Bộ Hệ Thống (Lần đầu hoặc sau khi sửa code)
```bash
docker compose up -d --build
```
> Tham số `-d` chạy ngầm (detached mode), `--build` tự động biên dịch lại nếu có thay đổi mã nguồn.

### Kiểm Tra Trạng Thái Các Container Đang Chạy
```bash
docker compose ps
```

### Xem Log Trực Tiếp Của Hệ Thống
```bash
# Xem log toàn bộ:
docker compose logs -f

# Xem riêng log backend:
docker compose logs -f backend

# Xem riêng log frontend:
docker compose logs -f frontend
```

### Dừng Hệ Thống
```bash
docker compose down
```

---

## 4. Địa Chỉ Truy Cập Mặc Định

- **Giao diện Bán Hàng & Thu Ngân**: [http://localhost/pos](http://localhost/pos) (hoặc [http://localhost:5173/pos](http://localhost:5173/pos))
- **Tổng quan Doanh Thu (Dashboard)**: [http://localhost/](http://localhost/)
- **Cài đặt & Quản lý VietQR**: [http://localhost/settings](http://localhost/settings)
- **Tài liệu Swagger API**: [http://localhost:5012/swagger](http://localhost:5012/swagger)
