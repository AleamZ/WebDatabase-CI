import pandas as pd
from datetime import datetime
import os

# Define the column structure as required by the DNImport
columns = [
    'STT', 'TenDN', 'Diachi', 'MaTinh_Dieutra', 'MaHuyen_Dieutra', 'MaXa_Dieutra',
    'DNTB_MaTinh', 'DNTB_MaHuyen', 'DNTB_MaXa', 'Region', 'Loaihinhkte', 'Email',
    'Dienthoai', 'Nam', 'Masothue', 'Vungkinhte', 'MaNganhC5_Chinh', 'TEN_NGANH',
    'SR_Doanhthu_Thuan_BH_CCDV', 'SR_Loinhuan_TruocThue', 'SoLaodong_DauNam',
    'SoLaodong_CuoiNam', 'Taisan_Tong_CK', 'Taisan_Tong_DK'
]

# Create sample data
test_data = [
    [
        1, 'Công ty TNHH Test 1', '123 Đường Test, Quận 1', '79', '760', '26734',
        '79', '760', '26734', 'Miền Nam', 'TNHH', 'test1@company.com',
        '0901234567', 2024, '0123456789', 'Đông Nam Bộ', 'C1611', 'Công nghệ thông tin',
        5000000.50, 800000.25, 25, 30, 10000000.75, 9500000.00
    ],
    [
        2, 'Công ty Cổ phần Test 2', '456 Đường Test, Ba Đình', '01', '001', '00001',
        '01', '001', '00001', 'Miền Bắc', 'Cổ phần', 'test2@company.vn',
        '0987654321', 2024, '9876543210', 'Đồng bằng Sông Hồng', 'C2910', 'Sản xuất ô tô',
        8000000.00, 1200000.50, 150, 180, 25000000.00, 20000000.00
    ],
    [
        3, 'Doanh nghiệp tư nhân Test 3', '789 Đường Test, Hải Châu', '48', '490', '19513',
        '48', '490', '19513', 'Miền Trung', 'Tư nhân', 'test3@company.vn',
        '0369258147', 2024, '1472583690', 'Duyên hải Nam Trung Bộ', 'F6320', 'Dịch vụ tài chính',
        2500000.00, 400000.00, 10, 12, 5000000.00, 4800000.00
    ]
]

# Create DataFrame
df = pd.DataFrame(test_data, columns=columns)

# Create the Excel file
output_file = 'test_dn_import.xlsx'
with pd.ExcelWriter(output_file, engine='openpyxl') as writer:
    df.to_excel(writer, sheet_name='DN_Data', index=False)

print(f"✅ Test Excel file created: {output_file}")
print(f"📊 File contains {len(df)} test records with {len(columns)} columns")
print("📋 Columns:", ', '.join(columns))
print(f"💾 File size: {os.path.getsize(output_file)} bytes") 