-- Script thêm cột Source vào bảng ExportRequests
-- Chạy script này để cập nhật cấu trúc bảng

USE sakila;

-- Thêm cột Source vào bảng ExportRequests
ALTER TABLE ExportRequests 
ADD COLUMN source VARCHAR(50) NULL COMMENT 'Trang xuất Excel: DN, Bacsi, PhoneSearch, etc.';

-- Cập nhật dữ liệu cũ (nếu có)
-- UPDATE ExportRequests SET source = 'Unknown' WHERE source IS NULL;

-- Hiển thị cấu trúc bảng sau khi cập nhật
DESCRIBE ExportRequests; 