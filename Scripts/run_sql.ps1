# Script PowerShell để chạy SQL script
param(
    [string]$Server = "localhost",
    [string]$Database = "sakila",
    [string]$User = "root",
    [string]$Password = "1234"
)

# Đường dẫn đến file SQL
$sqlFile = "Scripts\create_sample_data.sql"

# Kiểm tra xem file SQL có tồn tại không
if (-not (Test-Path $sqlFile)) {
    Write-Host "Không tìm thấy file SQL: $sqlFile" -ForegroundColor Red
    exit 1
}

# Đọc nội dung file SQL
$sqlContent = Get-Content $sqlFile -Raw

Write-Host "Đang chạy SQL script để tạo dữ liệu mẫu..." -ForegroundColor Yellow
Write-Host "Server: $Server" -ForegroundColor Cyan
Write-Host "Database: $Database" -ForegroundColor Cyan
Write-Host "User: $User" -ForegroundColor Cyan

try {
    # Tạo connection string
    $connectionString = "Server=$Server;Database=$Database;User=$User;Password=$Password;"
    
    # Tạo connection
    $connection = New-Object System.Data.Odbc.OdbcConnection
    $connection.ConnectionString = "Driver={MySQL ODBC 8.0 Driver};$connectionString"
    
    # Mở connection
    $connection.Open()
    Write-Host "Kết nối thành công đến database!" -ForegroundColor Green
    
    # Tạo command
    $command = $connection.CreateCommand()
    $command.CommandText = $sqlContent
    
    # Thực thi command
    $result = $command.ExecuteNonQuery()
    
    Write-Host "Đã thực thi SQL script thành công!" -ForegroundColor Green
    Write-Host "Số dòng bị ảnh hưởng: $result" -ForegroundColor Green
    
    # Đóng connection
    $connection.Close()
    
} catch {
    Write-Host "Lỗi khi thực thi SQL script: $($_.Exception.Message)" -ForegroundColor Red
    
    # Thử cách khác - sử dụng mysql command line nếu có
    Write-Host "Thử sử dụng mysql command line..." -ForegroundColor Yellow
    
    try {
        # Kiểm tra xem mysql có trong PATH không
        $mysqlPath = Get-Command mysql -ErrorAction SilentlyContinue
        if ($mysqlPath) {
            $mysqlArgs = "-h", $Server, "-u", $User, "-p$Password", $Database, "-e", "source $sqlFile"
            & mysql @mysqlArgs
            Write-Host "Đã thực thi SQL script bằng mysql command line!" -ForegroundColor Green
        } else {
            Write-Host "Không tìm thấy mysql command line. Vui lòng chạy SQL script thủ công." -ForegroundColor Red
            Write-Host "File SQL: $sqlFile" -ForegroundColor Cyan
        }
    } catch {
        Write-Host "Không thể chạy mysql command line: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Vui lòng chạy SQL script thủ công trong MySQL Workbench hoặc phpMyAdmin" -ForegroundColor Yellow
        Write-Host "File SQL: $sqlFile" -ForegroundColor Cyan
    }
}

Write-Host "Hoàn thành!" -ForegroundColor Green 