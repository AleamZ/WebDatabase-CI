@echo off
echo Setting up test data for chart visualization...
echo.

REM Check if MySQL is accessible
mysql --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: MySQL is not installed or not in PATH
    echo Please ensure MySQL is installed and accessible from command line
    pause
    exit /b 1
)

echo Running test data setup script...
mysql -h localhost -u root -p1234 < create_test_data.sql

if %errorlevel% equ 0 (
    echo.
    echo ✅ Test data setup completed successfully!
    echo The charts should now display data properly.
    echo.
    echo You can now:
    echo 1. Run the application: dotnet run
    echo 2. Navigate to /DN/TestData to verify database connection
    echo 3. Navigate to /DN/TestChartData to verify chart data
    echo 4. Navigate to /DN to see the charts with data
) else (
    echo.
    echo ❌ Error occurred while setting up test data
    echo Please check MySQL connection and credentials
)

echo.
pause 