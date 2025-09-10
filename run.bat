@echo off
echo ========================================
echo    ChildGuard - Child Protection System
echo    Starting Application...
echo ========================================
echo.

:: Check if Node.js is available
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Node.js is not installed!
    echo Please run setup.bat first or install Node.js from https://nodejs.org/
    echo.
    pause
    exit /b 1
)

:: Check if node_modules exists
if not exist "node_modules" (
    echo ERROR: Dependencies not installed!
    echo Please run setup.bat first to install dependencies.
    echo.
    pause
    exit /b 1
)

:: Check if package.json exists
if not exist "package.json" (
    echo ERROR: package.json not found!
    echo Please make sure you're in the correct directory.
    echo.
    pause
    exit /b 1
)

:: Display startup information
echo Starting ChildGuard Application...
echo.
echo Default login credentials:
echo   Username: admin
echo   Password: admin123
echo.
echo IMPORTANT: Change default password after first login!
echo.
echo The application will open in a new window.
echo Close this console window to stop the application.
echo.

:: Start the application
echo [INFO] Launching ChildGuard...
npm start

:: If npm start fails, try alternative methods
if %errorlevel% neq 0 (
    echo.
    echo [WARNING] npm start failed, trying alternative startup...
    echo.
    
    :: Try running electron directly
    if exist "node_modules\.bin\electron.cmd" (
        echo [INFO] Starting with Electron directly...
        node_modules\.bin\electron.cmd .
    ) else (
        echo [ERROR] Could not start the application!
        echo.
        echo Troubleshooting steps:
        echo 1. Run setup.bat to reinstall dependencies
        echo 2. Check if all files are present
        echo 3. Try running "npm run dev" for development mode
        echo.
        pause
        exit /b 1
    )
)

echo.
echo Application closed.
pause
