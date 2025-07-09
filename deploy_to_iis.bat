@echo off
echo ====================================
echo    DEPLOY TO IIS - CI RESEARCH
echo ====================================

echo.
echo Step 1: Cleaning previous builds...
dotnet clean

echo.
echo Step 2: Building in Release mode...
dotnet build --configuration Release

echo.
echo Step 3: Publishing for IIS deployment...
dotnet publish --configuration Release --output "bin\Release\net8.0\publish" --no-build

echo.
echo Step 4: Checking published files...
dir "bin\Release\net8.0\publish"

echo.
echo Step 5: Checking main DLL exists...
if exist "bin\Release\net8.0\publish\CIResearch.dll" (
    echo ✅ CIResearch.dll found - Ready for IIS deployment
) else (
    echo ❌ CIResearch.dll NOT found - Build failed
    pause
    exit /b 1
)

echo.
echo Step 6: Checking web.config...
if exist "bin\Release\net8.0\publish\web.config" (
    echo ✅ web.config found
) else (
    echo ⚠️ web.config not found - copying from wwwroot
    copy "wwwroot\web.config" "bin\Release\net8.0\publish\"
)

echo.
echo ====================================
echo        DEPLOYMENT COMPLETE!
echo ====================================
echo.
echo Published files are in: bin\Release\net8.0\publish\
echo.
echo Next steps:
echo 1. Copy all files from bin\Release\net8.0\publish\ to your IIS web directory
echo 2. Make sure ASP.NET Core Hosting Bundle is installed on server
echo 3. Create Application Pool targeting .NET Core
echo 4. Set website to point to your copied files
echo.
echo Requirements check: run check_server_requirements.bat on target server
echo.
pause 