# Tank Travel - Ứng Dụng quản lý dành cho TANK-Travel website

## 🚌 Giới Thiệu

Tank Travel là ứng dụng desktop quản lý và đặt vé xe khách/tàu hỏa, cung cấp giải pháp hiệu quả cho việc quản lý và đặt vé trực tuyến.

## 🛠 Công Nghệ

### Môi Trường Phát Triển
- **Ngôn Ngữ**: C# 💻  
- **Framework**: .NET Framework 4.7.2 hoặc cao hơn ⚙️  
- **Giao Diện**: WPF (Windows Presentation Foundation) 🖼️  

### Thư Viện & Công Cụ
- **Giao Diện**: LiveCharts 📊, Material Design 🎨  
- **HTTP Client**: HttpClient 🌐  
- **JSON Parsing**: Newtonsoft.Json 📝  
- **Xác thực**: JWT 🔒  
- **Quản lý kết nối**: Connection Pooling 🔗, Timeout configuration ⏳  

## 🔑 Chức Năng Chính

### Quản Trị Viên 👑
- Toàn quyền quản lý hệ thống ⚡  
- Quản lý tài khoản người dùng 👤  
- Kiểm duyệt đăng ký doanh nghiệp ✅  
- Quản lý thông báo hệ thống 📢  
- Xem báo cáo thống kê chi tiết 📈  

### Doanh Nghiệp 🏢
- Quản lý thông tin tài khoản ℹ️  
- Quản lý phương tiện (thêm, sửa, xóa, tra cứu) 🚍  
- Quản lý tuyến (thêm, sửa, xóa, tra cứu) 🛤️  
- Xem biểu đồ thống kê doanh thu 💰  
- Nhận thông báo riêng 🔔  

### Đặc Điểm Kết Nối API 🌐
- Sử dụng `HttpClient` với cấu hình tối ưu ⚙️  
- Hỗ trợ các phương thức HTTP: GET 📥, POST 📤, PUT ✏️, DELETE 🗑️  
- Xử lý nén dữ liệu (GZip, Deflate) 📦  
- Quản lý kết nối với connection pooling 🔗  
- Cấu hình timeout cho các request ⏱️  
- Xử lý lỗi và ngoại lệ chi tiết 🚨  

### Tính Năng Chính của API Service 🔧
- Gửi request không header 📡  
- Gửi request với header 📋  
- Gửi request với body 📦  
- Gửi request với header và body 📋📦  
- Hỗ trợ upload ảnh đơn và nhiều ảnh 🖼️📸  
- Tự động xử lý các loại ngoại lệ ⚠️  

## 🌐 Server Repository

**Link GitHub Server**: [Tank Travel Server Repository](https://github.com/ZombieGenZ/tank-travel-website) 🔗  

## 👥 Nhóm Phát Triển

| Tên                        | Vai Trò                  |
|----------------------------|------------------------- |
| Nguyễn Đặng Thành Thái     | 👨‍💼 Quản Lý Dự Án         |
| Ngô Gia Bảo                | 🧪 Chuyên viên Testing   |
| Nguyễn Đình Nam            | 💻 Lập Trình Frontend    |
| Nguyễn Đức Anh             | 💻 Lập Trình Frontend    |
| Bùi Đăng Khoa              | 💻 Lập Trình Backend    |
