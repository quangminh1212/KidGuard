@echo off
echo 🧪 ChildGuard Build Test
echo ========================

echo.
echo 📦 Installing dependencies...
call npm install

echo.
echo 🔨 Building renderer...
call npm run build:renderer

echo.
echo 🔨 Building main process...
call npx webpack --config webpack.main.config.js --mode production

echo.
echo ✅ Build test completed!
echo.
echo 🚀 To start the application:
echo npm start

pause
