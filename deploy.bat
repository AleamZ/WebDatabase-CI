@echo off
echo ================================================
echo  CI Research - Deployment Script
echo ================================================
echo.

REM Set environment for production
set ASPNETCORE_ENVIRONMENT=Production

echo [1/5] Setting environment to Production...
echo Environment: %ASPNETCORE_ENVIRONMENT%
echo.

echo [2/5] Cleaning previous builds...
if exist "publish" rmdir /s /q "publish"
if exist "bin\Release" rmdir /s /q "bin\Release"
echo.

echo [3/5] Building application...
dotnet clean
dotnet restore
dotnet build --configuration Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)
echo Build completed successfully!
echo.

echo [4/5] Publishing application...
dotnet publish --configuration Release --output ./publish --self-contained false
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Publish failed!
    pause
    exit /b 1
)
echo Publish completed successfully!
echo.

echo [5/5] Copying additional deployment files...
copy "appsettings.Production.json" "publish\" >nul
copy "setup_production_database.sql" "publish\" >nul
copy "update_existing_passwords.sql" "publish\" >nul
copy "DEPLOYMENT_GUIDE.md" "publish\" >nul
echo Deployment files copied!
echo.

echo ================================================
echo  DEPLOYMENT READY!
echo ================================================
echo.
echo Next steps:
echo 1. Copy contents of 'publish' folder to your web server
echo 2. Run setup_production_database.sql on MySQL server
echo 3. Run update_existing_passwords.sql (ONCE only)
echo 4. Set ASPNETCORE_ENVIRONMENT=Production on server
echo 5. Test connection: https://yoursite.com/LoginRegister/TestConnection
echo.
echo Login credentials:
echo   Username: admin
echo   Password: admin123
echo.
echo Full deployment guide available in DEPLOYMENT_GUIDE.md
echo.
pause 