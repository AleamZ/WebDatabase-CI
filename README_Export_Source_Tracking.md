# Tính năng Tracking Nguồn Xuất Excel

## Mô tả
Tính năng này cho phép theo dõi xuất Excel từ trang nào trong hệ thống CIResearch.

## Các nguồn xuất Excel được hỗ trợ

### 1. **DN (Doanh nghiệp)**
- **Controller:** `DNController`
- **Source:** "DN"
- **Mô tả:** Xuất dữ liệu doanh nghiệp từ trang DN
- **Trạng thái:** Chờ duyệt → Admin duyệt/từ chối

### 2. **Bacsi (Bác sĩ)**
- **Controller:** `BacsiController`
- **Source:** "Bacsi"
- **Mô tả:** Xuất dữ liệu bác sĩ từ trang Bacsi
- **Trạng thái:** Chờ duyệt → Admin duyệt/từ chối

### 3. **PhoneSearch (Tìm kiếm số điện thoại)**
- **Controller:** `AdminController.ExportPhoneSearchToExcel`
- **Source:** "PhoneSearch"
- **Mô tả:** Xuất kết quả tìm kiếm số điện thoại từ trang Admin
- **Trạng thái:** Xuất trực tiếp (Admin có quyền)

## Cấu trúc Database

### Bảng ExportRequests
```sql
CREATE TABLE ExportRequests (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL,
    request_time DATETIME NOT NULL,
    status VARCHAR(20) NOT NULL, -- pending, approved, rejected
    filter_params TEXT,
    file_data LONGBLOB,
    reject_reason TEXT,
    approved_time DATETIME,
    admin_approved_by VARCHAR(100),
    source VARCHAR(50) -- Trang xuất Excel: DN, Bacsi, PhoneSearch, etc.
);
```

## Cách sử dụng

### 1. **Cập nhật Database**
Chạy script SQL để thêm cột Source:
```sql
-- Script: Scripts/add_source_column_to_export_requests.sql
ALTER TABLE ExportRequests 
ADD COLUMN source VARCHAR(50) NULL COMMENT 'Trang xuất Excel: DN, Bacsi, PhoneSearch, etc.';
```

### 2. **Xem danh sách Export Requests**
- Truy cập: Admin > Duyệt yêu cầu export dữ liệu
- Cột "Nguồn" sẽ hiển thị badge màu xanh với tên nguồn
- Có thể lọc theo trạng thái: Chờ duyệt, Đã duyệt, Đã từ chối

### 3. **Xem chi tiết Export Request**
- Click "Chi tiết" để xem thông tin đầy đủ
- Hiển thị nguồn xuất Excel
- Có thể tải file Excel đã được duyệt

## Giao diện

### Danh sách Export Requests
- **Cột Nguồn:** Hiển thị badge màu xanh với tên nguồn
- **Cột Trạng thái:** Badge màu theo trạng thái
  - 🟡 Chờ duyệt (pending)
  - 🟢 Đã duyệt (approved)
  - 🔴 Đã từ chối (rejected)

### Trang chi tiết
- Hiển thị đầy đủ thông tin export request
- Bao gồm nguồn xuất Excel
- Có thể tải file Excel

## Code Implementation

### 1. **Model ExportRequest**
```csharp
public class ExportRequest
{
    // ... existing properties
    public string Source { get; set; } // Trang xuất Excel: DN, Bacsi, PhoneSearch, etc.
}
```

### 2. **Repository Pattern**
```csharp
// Thêm Source vào INSERT query
INSERT INTO ExportRequests (..., source) VALUES (..., @source)

// Đọc Source từ database
Source = reader.IsDBNull(reader.GetOrdinal("source")) ? null : reader.GetString(reader.GetOrdinal("source"))
```

### 3. **Controller Implementation**
```csharp
// DNController
Source = "DN"

// BacsiController  
Source = "Bacsi"

// AdminController (PhoneSearch)
Source = "PhoneSearch"
```

## Lợi ích

### 1. **Tracking & Analytics**
- Biết được xuất Excel từ trang nào
- Thống kê theo nguồn xuất
- Phân tích xu hướng sử dụng

### 2. **Quản lý & Bảo mật**
- Kiểm soát quyền xuất theo từng trang
- Audit trail đầy đủ
- Dễ dàng debug khi có vấn đề

### 3. **Báo cáo**
- Báo cáo theo nguồn xuất
- Thống kê hiệu suất từng trang
- Phân tích nhu cầu sử dụng

## Lưu ý

### 1. **Backward Compatibility**
- Cột Source có thể NULL cho dữ liệu cũ
- Không ảnh hưởng đến chức năng hiện tại

### 2. **Performance**
- Cột Source chỉ là VARCHAR(50), không ảnh hưởng performance
- Index có thể được thêm nếu cần query theo Source

### 3. **Maintenance**
- Dễ dàng thêm nguồn mới
- Code pattern thống nhất
- Dễ dàng mở rộng tính năng

## Troubleshooting

### 1. **Lỗi "Column 'source' doesn't exist"**
- Chạy script SQL để thêm cột Source
- Kiểm tra tên bảng và database

### 2. **Source hiển thị "Không xác định"**
- Kiểm tra dữ liệu trong database
- Cập nhật dữ liệu cũ nếu cần

### 3. **Badge không hiển thị**
- Kiểm tra Bootstrap CSS
- Kiểm tra logic hiển thị trong view 