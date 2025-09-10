@echo off
echo ========================================
echo    ChildGuard Setup Script
echo    Child Protection System Setup
echo ========================================
echo.

:: Check if Node.js is installed
echo [1/6] Checking Node.js installation...
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Node.js is not installed!
    echo Please install Node.js 18+ from https://nodejs.org/
    echo.
    pause
    exit /b 1
)

:: Display Node.js version
for /f "tokens=*" %%i in ('node --version') do set NODE_VERSION=%%i
echo ✓ Node.js version: %NODE_VERSION%

:: Check if npm is available
echo [2/6] Checking npm installation...
npm --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: npm is not available!
    echo Please reinstall Node.js with npm included.
    echo.
    pause
    exit /b 1
)

:: Display npm version
for /f "tokens=*" %%i in ('npm --version') do set NPM_VERSION=%%i
echo ✓ npm version: %NPM_VERSION%
echo.

:: Install dependencies
echo [3/6] Installing project dependencies...
echo This may take a few minutes...
echo.
npm install
if %errorlevel% neq 0 (
    echo ERROR: Failed to install dependencies!
    echo Please check your internet connection and try again.
    echo.
    pause
    exit /b 1
)
echo ✓ Dependencies installed successfully!
echo.

:: Install development dependencies
echo [4/6] Installing development dependencies...
npm install --save-dev @types/jest @types/node ts-jest jest
if %errorlevel% neq 0 (
    echo WARNING: Some development dependencies failed to install.
    echo The application should still work, but testing may not be available.
    echo.
)
echo ✓ Development dependencies installed!
echo.

:: Create necessary directories
echo [5/6] Creating necessary directories...
if not exist "dist" mkdir dist
if not exist "logs" mkdir logs
if not exist "temp" mkdir temp
if not exist "release" mkdir release
echo ✓ Directories created!
echo.

:: Run TypeScript compilation check
echo [6/6] Verifying TypeScript compilation...
npx tsc --noEmit
if %errorlevel% neq 0 (
    echo WARNING: TypeScript compilation has some issues.
    echo The application may still work, but please check for errors.
    echo.
) else (
    echo ✓ TypeScript compilation verified!
    echo.
)

:: Setup complete
echo ========================================
echo    Setup Complete!
echo ========================================
echo.
echo ChildGuard has been set up successfully!
echo.
echo Next steps:
echo   1. Run "run.bat" to start the application
echo   2. Or run "npm run dev" for development mode
echo   3. Or run "npm run build" to build for production
echo.
echo Default login credentials:
echo   Username: admin
echo   Password: admin123
echo.
echo IMPORTANT: Change the default password after first login!
echo.
echo For more information, see README.md
echo.
pause
