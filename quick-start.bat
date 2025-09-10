@echo off
echo ========================================
echo    ChildGuard - Quick Start
echo    Child Protection System
echo ========================================
echo.

echo Welcome to ChildGuard!
echo.
echo This script will help you get started quickly.
echo.

:MENU
echo Please choose an option:
echo.
echo [1] First-time setup (install dependencies)
echo [2] Run ChildGuard application
echo [3] Build production version
echo [4] View project information
echo [5] Exit
echo.
set /p choice="Enter your choice (1-5): "

if "%choice%"=="1" goto SETUP
if "%choice%"=="2" goto RUN
if "%choice%"=="3" goto BUILD
if "%choice%"=="4" goto INFO
if "%choice%"=="5" goto EXIT
echo Invalid choice. Please try again.
echo.
goto MENU

:SETUP
echo.
echo Running first-time setup...
call setup.bat
echo.
echo Setup completed! You can now run the application.
echo.
pause
goto MENU

:RUN
echo.
echo Starting ChildGuard application...
call run.bat
echo.
pause
goto MENU

:BUILD
echo.
echo Building production version...
call build.bat
echo.
pause
goto MENU

:INFO
echo.
echo ========================================
echo    ChildGuard Project Information
echo ========================================
echo.
echo Project: ChildGuard - Child Protection System
echo Version: 1.0.0
echo Platform: Windows 10/11
echo Technology: Electron + React + TypeScript
echo.
echo Features:
echo - Real-time keylogger monitoring
echo - Intelligent content filtering
echo - Multi-language support (Vietnamese + English)
echo - AES-256 encryption for data security
echo - COPPA/GDPR compliant privacy protection
echo - Professional Material-UI interface
echo - Real-time notifications and alerts
echo - Comprehensive reporting system
echo.
echo Default Login:
echo   Username: admin
echo   Password: admin123
echo.
echo IMPORTANT: Change default password after first login!
echo.
echo Files:
echo   setup.bat    - First-time setup and dependency installation
echo   run.bat      - Start the application
echo   build.bat    - Build production version
echo   README.md    - Detailed documentation
echo.
echo For more information, see README.md
echo.
pause
goto MENU

:EXIT
echo.
echo Thank you for using ChildGuard!
echo For support, please check the documentation or contact support.
echo.
exit /b 0
