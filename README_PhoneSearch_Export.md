# Tính năng Xuất Excel cho Tìm kiếm Số điện thoại

## Mô tả
Tính năng này cho phép xuất kết quả tìm kiếm số điện thoại ra file Excel và gửi qua email.

## Cách sử dụng

### 1. Truy cập trang tìm kiếm
- Đăng nhập với quyền Admin
- Vào trang Admin > Tìm kiếm số điện thoại

### 2. Tìm kiếm số điện thoại
- Nhập số điện thoại cần tìm vào ô tìm kiếm
- Nhấn nút "Tìm kiếm" để xem kết quả

### 3. Xuất Excel
Có 2 cách để xuất Excel:

#### Cách 1: Xuất trực tiếp
- Nhấn nút "Xuất Excel" bên cạnh nút "Tìm kiếm"
- Hệ thống sẽ tạo file Excel và gửi qua email

#### Cách 2: Xuất sau khi có kết quả
- Sau khi có kết quả tìm kiếm
- Nhấn nút "Xuất kết quả ra Excel" phía trên bảng kết quả

## Thông tin Email

### Email gửi từ:
- **Địa chỉ:** ciresearch.dn@gmail.com
- **Mật khẩu:** mhip zhvj dhpd zrgo (App password)

### Email nhận:
- Hệ thống sẽ tự động lấy email từ cột Email trong bảng users của user đang đăng nhập

## Nội dung file Excel

File Excel sẽ chứa các thông tin sau:
- STT
- Họ tên
- Số điện thoại
- Email
- Thành phố
- Quận/Huyện
- Phường/Xã
- Địa chỉ
- Tuổi
- Giới tính
- Nghề nghiệp
- Tình trạng hôn nhân
- Dự án
- Năm
- Code

## Thông báo

### Thông báo thành công:
- Hiển thị số lượng bản ghi đã xuất
- Hiển thị email đã gửi

### Thông báo lỗi:
- Lỗi khi không tìm thấy email của user
- Lỗi khi gửi email
- Lỗi khi tạo file Excel

## Lưu ý
- Chỉ user có quyền Admin mới có thể sử dụng tính năng này
- File Excel sẽ được đặt tên theo format: `TimKiemSoDienThoai_YYYYMMDD_HHMMSS.xlsx`
- Email sẽ được gửi với nội dung HTML có thông tin chi tiết về kết quả tìm kiếm

## Cấu hình SMTP
- **Server:** smtp.gmail.com
- **Port:** 587
- **SSL:** Enabled
- **Authentication:** Username/Password

## Dependencies
- ClosedXML (v0.102.2) - Để tạo file Excel
- System.Net.Mail - Để gửi email
- MySql.Data - Để truy vấn database 