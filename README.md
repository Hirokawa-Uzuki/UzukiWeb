# Uzuki Manga Reader - Hệ thống đọc truyện trực tuyến

Dự án cuối khóa môn **Thiết kế & Lập trình Back-End** - Đại học Đại Nam.

## 📖 Giới thiệu
Uzuki là một nền tảng web đọc truyện trực tuyến được xây dựng trên mô hình **Freemium**, cho phép người dùng đọc miễn phí các chương cơ bản và thanh toán để truy cập các chương cao cấp.

## 🚀 Công nghệ sử dụng
- **Back-end:** ASP.NET Core 10.0 (MVC)
- **Database:** SQL Server
- **Authentication:** ASP.NET Core Identity
- **Thanh toán:** Tích hợp ví điện tử/mô hình Freemium

## ✨ Tính năng chính (Lưu ý đây là mô tả khái quát còn nhiều chức năng khác lên tự khám phá)
- [x] Đọc truyện trực tuyến (giao diện responsive).
- [x] Quản lý truyện, chương cho Admin.
- [x] Hệ thống tài khoản người dùng.
- [x] Tích hợp mô hình thanh toán chương VIP.
 

## 🛠 Hướng dẫn cài đặt
1. Clone dự án: `git clone https://github.com/Hirokawa-Uzuki/UzukiWeb.git`
2. Cập nhật chuỗi kết nối Database trong `appsettings.json`.
3. Chạy lệnh `Update-Database` trong Package Manager Console.
4. Nhấn `F5` để chạy dự án.
