-- Tạo bảng all_data_final trong database sakila
USE sakila;

-- Tạo bảng all_data_final
CREATE TABLE IF NOT EXISTS all_data_final (
    STT INT AUTO_INCREMENT PRIMARY KEY,
    CODE VARCHAR(50),
    PROJECTNAME VARCHAR(100),
    YEAR INT,
    CONTACTOBJECT VARCHAR(100),
    SBJNUM INT,
    FULLNAME VARCHAR(100),
    CITY VARCHAR(50),
    ADDRESS TEXT,
    STREET VARCHAR(100),
    WARD VARCHAR(50),
    DISTRICT VARCHAR(50),
    PHONENUMBER VARCHAR(20),
    EMAIL VARCHAR(100),
    DATEOFBIRTH INT,
    AGE INT,
    SEX VARCHAR(10),
    JOB VARCHAR(100),
    HOUSEHOLDINCOME VARCHAR(50),
    PERSONALINCOME VARCHAR(50),
    MARITALSTATUS VARCHAR(50),
    MOSTFREQUENTLYUSEDBRAND VARCHAR(100),
    SOURCE VARCHAR(100),
    Class VARCHAR(50),
    EDUCATION VARCHAR(50),
    PROVINCES VARCHAR(50),
    QC VARCHAR(50),
    QA VARCHAR(50),
    Nganhhang VARCHAR(100),
    ChuyenKhoa VARCHAR(100)
);

-- Xóa dữ liệu cũ nếu có
DELETE FROM all_data_final;

-- Thêm dữ liệu mẫu
INSERT INTO all_data_final (CODE, PROJECTNAME, YEAR, FULLNAME, CITY, AGE, SEX, JOB, MARITALSTATUS, Class, Nganhhang, ChuyenKhoa) VALUES
('P001', 'Dự án A', 2020, 'Nguyễn Văn A', 'HÀ NỘI', 25, 'Nam', 'Kỹ sư', 'Độc thân', 'Công nghệ', 'Công nghệ thông tin', 'Kỹ thuật'),
('P002', 'Dự án B', 2020, 'Trần Thị B', 'HỒ CHÍ MINH', 30, 'Nữ', 'Giáo viên', 'Đã kết hôn', 'Giáo dục', 'Giáo dục', 'Sư phạm'),
('P003', 'Dự án C', 2021, 'Lê Văn C', 'ĐÀ NẴNG', 28, 'Nam', 'Bác sĩ', 'Độc thân', 'Y tế', 'Y tế', 'Y khoa'),
('P004', 'Dự án A', 2021, 'Phạm Thị D', 'HÀ NỘI', 35, 'Nữ', 'Kế toán', 'Đã kết hôn', 'Tài chính', 'Tài chính', 'Kế toán'),
('P005', 'Dự án B', 2021, 'Hoàng Văn E', 'HỒ CHÍ MINH', 27, 'Nam', 'Thiết kế', 'Độc thân', 'Nghệ thuật', 'Truyền thông', 'Thiết kế'),
('P006', 'Dự án C', 2022, 'Vũ Thị F', 'ĐÀ NẴNG', 32, 'Nữ', 'Luật sư', 'Đã kết hôn', 'Pháp lý', 'Pháp lý', 'Luật'),
('P007', 'Dự án A', 2022, 'Đặng Văn G', 'HÀ NỘI', 29, 'Nam', 'Marketing', 'Độc thân', 'Marketing', 'Truyền thông', 'Marketing'),
('P008', 'Dự án B', 2022, 'Bùi Thị H', 'HỒ CHÍ MINH', 31, 'Nữ', 'Nhân sự', 'Đã kết hôn', 'Nhân sự', 'Dịch vụ', 'Quản trị'),
('P009', 'Dự án C', 2023, 'Ngô Văn I', 'ĐÀ NẴNG', 26, 'Nam', 'Lập trình', 'Độc thân', 'Công nghệ', 'Công nghệ thông tin', 'Kỹ thuật'),
('P010', 'Dự án A', 2023, 'Lý Thị K', 'HÀ NỘI', 33, 'Nữ', 'Bán hàng', 'Đã kết hôn', 'Thương mại', 'Thương mại', 'Kinh doanh'),
('P011', 'Dự án B', 2023, 'Trịnh Văn L', 'HỒ CHÍ MINH', 24, 'Nam', 'Sinh viên', 'Độc thân', 'Giáo dục', 'Giáo dục', 'Sinh viên'),
('P012', 'Dự án C', 2023, 'Đinh Thị M', 'ĐÀ NẴNG', 36, 'Nữ', 'Quản lý', 'Đã kết hôn', 'Quản lý', 'Dịch vụ', 'Quản trị'),
('P013', 'Dự án A', 2020, 'Tô Văn N', 'HÀ NỘI', 28, 'Nam', 'Kỹ sư', 'Độc thân', 'Công nghệ', 'Công nghệ thông tin', 'Kỹ thuật'),
('P014', 'Dự án B', 2020, 'Hồ Thị O', 'HỒ CHÍ MINH', 29, 'Nữ', 'Giáo viên', 'Đã kết hôn', 'Giáo dục', 'Giáo dục', 'Sư phạm'),
('P015', 'Dự án C', 2021, 'Dương Văn P', 'ĐÀ NẴNG', 31, 'Nam', 'Bác sĩ', 'Độc thân', 'Y tế', 'Y tế', 'Y khoa'),
('P016', 'Dự án A', 2021, 'Võ Thị Q', 'HÀ NỘI', 27, 'Nữ', 'Kế toán', 'Đã kết hôn', 'Tài chính', 'Tài chính', 'Kế toán'),
('P017', 'Dự án B', 2021, 'Lưu Văn R', 'HỒ CHÍ MINH', 30, 'Nam', 'Thiết kế', 'Độc thân', 'Nghệ thuật', 'Truyền thông', 'Thiết kế'),
('P018', 'Dự án C', 2022, 'Châu Thị S', 'ĐÀ NẴNG', 34, 'Nữ', 'Luật sư', 'Đã kết hôn', 'Pháp lý', 'Pháp lý', 'Luật'),
('P019', 'Dự án A', 2022, 'Huỳnh Văn T', 'HÀ NỘI', 25, 'Nam', 'Marketing', 'Độc thân', 'Marketing', 'Truyền thông', 'Marketing'),
('P020', 'Dự án B', 2022, 'Phan Thị U', 'HỒ CHÍ MINH', 32, 'Nữ', 'Nhân sự', 'Đã kết hôn', 'Nhân sự', 'Dịch vụ', 'Quản trị'),
('P021', 'Dự án C', 2023, 'Mai Văn V', 'ĐÀ NẴNG', 28, 'Nam', 'Lập trình', 'Độc thân', 'Công nghệ', 'Công nghệ thông tin', 'Kỹ thuật'),
('P022', 'Dự án A', 2023, 'Lâm Thị W', 'HÀ NỘI', 35, 'Nữ', 'Bán hàng', 'Đã kết hôn', 'Thương mại', 'Thương mại', 'Kinh doanh'),
('P023', 'Dự án B', 2023, 'Thạch Văn X', 'HỒ CHÍ MINH', 26, 'Nam', 'Sinh viên', 'Độc thân', 'Giáo dục', 'Giáo dục', 'Sinh viên'),
('P024', 'Dự án C', 2023, 'Sơn Thị Y', 'ĐÀ NẴNG', 33, 'Nữ', 'Quản lý', 'Đã kết hôn', 'Quản lý', 'Dịch vụ', 'Quản trị'),
('P025', 'Dự án A', 2020, 'Hà Văn Z', 'HÀ NỘI', 29, 'Nam', 'Kỹ sư', 'Độc thân', 'Công nghệ', 'Công nghệ thông tin', 'Kỹ thuật');

-- Thêm thêm dữ liệu để có đủ số lượng
INSERT INTO all_data_final (CODE, PROJECTNAME, YEAR, FULLNAME, CITY, AGE, SEX, JOB, MARITALSTATUS, Class, Nganhhang, ChuyenKhoa) VALUES
('P026', 'Dự án B', 2020, 'Nguyễn Thị AA', 'HỒ CHÍ MINH', 31, 'Nữ', 'Giáo viên', 'Đã kết hôn', 'Giáo dục', 'Giáo dục', 'Sư phạm'),
('P027', 'Dự án C', 2021, 'Trần Văn BB', 'ĐÀ NẴNG', 27, 'Nam', 'Bác sĩ', 'Độc thân', 'Y tế', 'Y tế', 'Y khoa'),
('P028', 'Dự án A', 2021, 'Lê Thị CC', 'HÀ NỘI', 34, 'Nữ', 'Kế toán', 'Đã kết hôn', 'Tài chính', 'Tài chính', 'Kế toán'),
('P029', 'Dự án B', 2021, 'Phạm Văn DD', 'HỒ CHÍ MINH', 28, 'Nam', 'Thiết kế', 'Độc thân', 'Nghệ thuật', 'Truyền thông', 'Thiết kế'),
('P030', 'Dự án C', 2022, 'Hoàng Thị EE', 'ĐÀ NẴNG', 30, 'Nữ', 'Luật sư', 'Đã kết hôn', 'Pháp lý', 'Pháp lý', 'Luật');

-- Hiển thị số lượng dữ liệu đã tạo
SELECT COUNT(*) as TotalRecords FROM all_data_final; 