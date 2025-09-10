# ChildGuard - Hướng dẫn khởi động nhanh

## 🚀 Bắt đầu nhanh chóng

### Bước 1: Khởi động script tương tác
```cmd
# Chạy script menu tương tác
quick-start.bat
```

### Bước 2: Cài đặt lần đầu
```cmd
# Hoặc chạy trực tiếp setup
setup.bat
```

### Bước 3: Khởi chạy ứng dụng
```cmd
# Chạy ứng dụng
run.bat
```

## 📋 Các script có sẵn

### 🔧 `quick-start.bat` - Menu tương tác
- **Chức năng**: Menu chính với các tùy chọn
- **Sử dụng**: Double-click hoặc chạy từ command line
- **Tính năng**:
  - [1] First-time setup
  - [2] Run application  
  - [3] Build production
  - [4] View project info
  - [5] Exit

### ⚙️ `setup.bat` - Cài đặt dự án
- **Chức năng**: Cài đặt dependencies và chuẩn bị môi trường
- **Thời gian**: 2-5 phút (tùy tốc độ internet)
- **Yêu cầu**: Node.js 18+ đã được cài đặt
- **Các bước thực hiện**:
  1. Kiểm tra Node.js và npm
  2. Cài đặt dependencies
  3. Cài đặt dev dependencies
  4. Tạo thư mục cần thiết
  5. Kiểm tra TypeScript compilation

### 🏃 `run.bat` - Khởi chạy ứng dụng
- **Chức năng**: Khởi động ChildGuard application
- **Yêu cầu**: Đã chạy setup.bat trước đó
- **Tính năng**:
  - Kiểm tra dependencies
  - Hiển thị thông tin đăng nhập
  - Khởi động Electron app
  - Fallback nếu npm start thất bại

### 🔨 `build.bat` - Build production
- **Chức năng**: Tạo bản build production và installer
- **Thời gian**: 3-10 phút
- **Output**: 
  - `dist/` - Build files
  - `release/` - Installer files (.exe, .msi)
- **Các bước**:
  1. Clean previous builds
  2. TypeScript compilation check
  3. Build application
  4. Run tests
  5. Create installer

## 🔑 Thông tin đăng nhập mặc định

```
Username: admin
Password: admin123
```

**⚠️ QUAN TRỌNG**: Thay đổi mật khẩu mặc định sau lần đăng nhập đầu tiên!

## 🛠️ Yêu cầu hệ thống

### Phần mềm cần thiết
- **Windows 10/11** (64-bit)
- **Node.js 18+** - [Download tại đây](https://nodejs.org/)
- **npm 8+** (đi kèm với Node.js)

### Phần cứng khuyến nghị
- **RAM**: 4GB minimum, 8GB recommended
- **Storage**: 500MB free space
- **CPU**: Intel i3 hoặc tương đương
- **Permissions**: Administrator privileges

## 🔍 Troubleshooting

### Lỗi thường gặp

#### 1. "Node.js is not installed"
```cmd
# Giải pháp: Cài đặt Node.js
# Download từ: https://nodejs.org/
# Chọn phiên bản LTS (Long Term Support)
```

#### 2. "Dependencies not installed"
```cmd
# Giải pháp: Chạy setup
setup.bat
```

#### 3. "npm start failed"
```cmd
# Giải pháp 1: Reinstall dependencies
npm install

# Giải pháp 2: Clear cache
npm cache clean --force
npm install

# Giải pháp 3: Delete node_modules và reinstall
rmdir /s node_modules
npm install
```

#### 4. "TypeScript compilation failed"
```cmd
# Kiểm tra TypeScript errors
npx tsc --noEmit

# Cài đặt TypeScript globally nếu cần
npm install -g typescript
```

#### 5. "Build failed"
```cmd
# Kiểm tra dependencies
npm install

# Chạy build với verbose output
npm run build --verbose
```

### Kiểm tra hệ thống

#### Kiểm tra Node.js
```cmd
node --version
npm --version
```

#### Kiểm tra project structure
```cmd
dir src
dir node_modules
```

#### Kiểm tra dependencies
```cmd
npm list --depth=0
```

## 📁 Cấu trúc thư mục

```
ChildGuard/
├── quick-start.bat          # 🎯 Menu tương tác chính
├── setup.bat               # ⚙️ Cài đặt dự án
├── run.bat                 # 🏃 Khởi chạy app
├── build.bat               # 🔨 Build production
├── package.json            # 📦 Dependencies
├── README.md               # 📖 Documentation chi tiết
├── PROJECT_SUMMARY.md      # 📋 Tổng quan dự án
├── src/                    # 💻 Source code
│   ├── main/              # Electron main process
│   ├── renderer/          # React frontend
│   └── shared/            # Shared code
├── tests/                  # 🧪 Test files
├── docs/                   # 📚 Documentation
└── scripts/               # 🔧 Build scripts
```

## 🎯 Các bước tiếp theo sau khi cài đặt

### 1. Đăng nhập lần đầu
- Sử dụng tài khoản mặc định: `admin` / `admin123`
- Thay đổi mật khẩu ngay lập tức
- Cấu hình email thông báo (tùy chọn)

### 2. Tạo hồ sơ trẻ em
- Vào mục "Children" → "Add Child"
- Nhập thông tin: tên, tuổi, mức độ hạn chế
- Cấu hình giờ sử dụng máy tính

### 3. Bắt đầu giám sát
- Chọn trẻ cần giám sát
- Nhấn "Start Monitoring"
- Theo dõi dashboard và alerts

### 4. Tùy chỉnh cài đặt
- Điều chỉnh độ nhạy bộ lọc
- Cấu hình thông báo
- Thiết lập báo cáo tự động

## 📞 Hỗ trợ

### Tài liệu
- **README.md** - Hướng dẫn chi tiết
- **docs/PRIVACY_POLICY.md** - Chính sách bảo mật
- **docs/DEPLOYMENT_GUIDE.md** - Hướng dẫn triển khai

### Liên hệ
- **Email**: support@childguard.com
- **Website**: https://childguard.com
- **Issues**: GitHub Issues

---

**ChildGuard v1.0.0** - Bảo vệ trẻ em trong thế giới số 🛡️
