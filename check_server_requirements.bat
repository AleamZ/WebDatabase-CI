@echo off
echo ====================================
echo  SERVER REQUIREMENTS CHECK
echo ====================================

echo.
echo Checking ASP.NET Core Runtime...
dotnet --version 2>nul
if %errorlevel% equ 0 (
    echo ✅ .NET Core Runtime found
    dotnet --list-runtimes | findstr "Microsoft.AspNetCore.App"
    if %errorlevel% equ 0 (
        echo ✅ ASP.NET Core Runtime found
    ) else (
        echo ❌ ASP.NET Core Runtime NOT found
        echo Please install: ASP.NET Core Hosting Bundle
        echo Download: https://dotnet.microsoft.com/download/dotnet/8.0
    )
) else (
    echo ❌ .NET Core NOT found
    echo Please install: ASP.NET Core Hosting Bundle
    echo Download: https://dotnet.microsoft.com/download/dotnet/8.0
)

echo.
echo Checking IIS features...
powershell -Command "Get-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole" | findstr "Enabled"
if %errorlevel% equ 0 (
    echo ✅ IIS Web Server Role enabled
) else (
    echo ❌ IIS Web Server Role NOT enabled
    echo Please enable IIS in Windows Features
)

echo.
echo Checking ASP.NET Core Module...
if exist "%SystemRoot%\System32\inetsrv\aspnetcorev2.dll" (
    echo ✅ ASP.NET Core Module V2 found
) else (
    echo ❌ ASP.NET Core Module V2 NOT found
    echo Install ASP.NET Core Hosting Bundle
)

echo.
echo Checking MySQL Connection...
echo Please ensure MySQL is accessible from this server
echo Connection string: Server=localhost;Database=sakila;User=root;Password=1234;

echo.
echo ====================================
echo  REQUIREMENTS CHECK COMPLETE
echo ====================================

pause 