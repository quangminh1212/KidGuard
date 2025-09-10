@echo off
echo ========================================
echo    ChildGuard Build Script
echo    Building Production Version
echo ========================================
echo.

:: Check if Node.js is available
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Node.js is not installed!
    echo Please install Node.js 18+ from https://nodejs.org/
    echo.
    pause
    exit /b 1
)

:: Check if dependencies are installed
if not exist "node_modules" (
    echo ERROR: Dependencies not installed!
    echo Please run setup.bat first.
    echo.
    pause
    exit /b 1
)

echo [1/5] Cleaning previous builds...
if exist "dist" rmdir /s /q "dist"
if exist "release" rmdir /s /q "release"
mkdir dist
mkdir release
echo ✓ Clean completed!
echo.

echo [2/5] Running TypeScript compilation...
npx tsc --noEmit
if %errorlevel% neq 0 (
    echo ERROR: TypeScript compilation failed!
    echo Please fix TypeScript errors before building.
    echo.
    pause
    exit /b 1
)
echo ✓ TypeScript compilation successful!
echo.

echo [3/5] Building application...
npm run build
if %errorlevel% neq 0 (
    echo ERROR: Build failed!
    echo Please check the error messages above.
    echo.
    pause
    exit /b 1
)
echo ✓ Application build successful!
echo.

echo [4/5] Running tests...
npm test
if %errorlevel% neq 0 (
    echo WARNING: Some tests failed!
    echo Build will continue, but please review test results.
    echo.
)
echo ✓ Tests completed!
echo.

echo [5/5] Creating installer...
npm run dist
if %errorlevel% neq 0 (
    echo ERROR: Installer creation failed!
    echo Please check electron-builder configuration.
    echo.
    pause
    exit /b 1
)
echo ✓ Installer created successfully!
echo.

:: Display build results
echo ========================================
echo    Build Complete!
echo ========================================
echo.
echo Build artifacts created:
if exist "dist\main.js" echo ✓ dist\main.js
if exist "dist\renderer.js" echo ✓ dist\renderer.js
if exist "dist\index.html" echo ✓ dist\index.html
echo.
echo Installer files:
if exist "release\*.exe" (
    for %%f in (release\*.exe) do echo ✓ %%f
)
if exist "release\*.msi" (
    for %%f in (release\*.msi) do echo ✓ %%f
)
echo.
echo The installer is ready for distribution!
echo.
pause
