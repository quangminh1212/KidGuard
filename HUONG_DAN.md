# 📚 HƯỚNG DẪN SỬ DỤNG KIDGUARD

## 📋 Mục Lục
- [Giới Thiệu](#giới-thiệu)
- [Yêu Cầu Hệ Thống](#yêu-cầu-hệ-thống)
- [Cài Đặt](#cài-đặt)
- [Hướng Dẫn Sử Dụng](#hướng-dẫn-sử-dụng)
- [Cấu Trúc Dự Án](#cấu-trúc-dự-án)
- [API và Services](#api-và-services)
- [Cấu Hình](#cấu-hình)
- [Xử Lý Lỗi](#xử-lý-lỗi)
- [Đóng Góp](#đóng-góp)

## 🎯 Giới Thiệu

KidGuard là phần mềm kiểm soát và bảo vệ trẻ em trên không gian mạng, được phát triển bằng C# .NET 9.0 với Windows Forms. Phần mềm giúp phụ huynh:

- 🚫 **Chặn website không phù hợp**: Tự động chặn các trang web thuộc danh mục nguy hiểm
- 🎮 **Kiểm soát ứng dụng**: Giám sát và hạn chế thời gian sử dụng game, mạng xã hội
- ⏰ **Quản lý thời gian**: Đặt giới hạn thời gian sử dụng máy tính hàng ngày
- 📊 **Báo cáo chi tiết**: Theo dõi hoạt động của con cái qua báo cáo hàng ngày
- 🔒 **Bảo mật cao**: Yêu cầu quyền Administrator, mã hóa cấu hình

## 💻 Yêu Cầu Hệ Thống

### Yêu Cầu Tối Thiểu
- **Hệ điều hành**: Windows 10 version 1903 trở lên hoặc Windows 11
- **RAM**: 4GB (khuyến nghị 8GB)
- **Dung lượng**: 200MB cho cài đặt, 1GB cho logs và dữ liệu
- **.NET Runtime**: .NET 9.0 Desktop Runtime
- **Quyền**: Administrator (bắt buộc)

### Yêu Cầu Phát Triển
- Visual Studio 2022 (17.8+) hoặc Visual Studio Code
- .NET 9.0 SDK
- Git cho version control

## 📦 Cài Đặt

### Cách 1: Cài Đặt Từ Source Code

```bash
# 1. Clone repository
git clone https://github.com/quangminh1212/KidGuard.git
cd KidGuard

# 2. Build dự án
dotnet build --configuration Release

# 3. Chạy ứng dụng (yêu cầu quyền Admin)
dotnet run --project src/KidGuard
```

### Cách 2: Cài Đặt Từ File Thực Thi

1. Tải file `KidGuard-Setup.exe` từ [Releases](https://github.com/quangminh1212/KidGuard/releases)
2. Chạy file setup với quyền Administrator
3. Làm theo hướng dẫn cài đặt
4. Khởi động từ Desktop shortcut

### Cách 3: Build File Thực Thi

```bash
# Build self-contained executable
dotnet publish src/KidGuard -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# File exe sẽ ở: src/KidGuard/bin/Release/net9.0-windows/win-x64/publish/
```

## 📖 Hướng Dẫn Sử Dụng

### 1. Khởi Động Ứng Dụng

- **Bước 1**: Click chuột phải vào KidGuard.exe
- **Bước 2**: Chọn "Run as Administrator"
- **Bước 3**: Xác nhận UAC prompt
- **Bước 4**: Đăng nhập với mật khẩu phụ huynh (mặc định: admin123)

### 2. Chặn Website

#### Chặn Thủ Công
1. Vào tab **"Website Blocking"**
2. Nhập địa chỉ website (VD: facebook.com)
3. Chọn danh mục (Social Media, Gaming, Adult, v.v.)
4. Click **"Block Website"**

#### Chặn Theo Danh Mục
1. Vào **Settings** → **Category Blocking**
2. Chọn các danh mục muốn chặn:
   - ✅ Mạng xã hội (Facebook, TikTok, Instagram)
   - ✅ Game online (Steam, Epic Games, Garena)
   - ✅ Video/Streaming (YouTube, Netflix)
   - ✅ Nội dung người lớn
3. Click **"Apply"** để áp dụng

#### Import Danh Sách
1. Chuẩn bị file `.txt` với mỗi dòng là một domain
2. Click **"Import List"**
3. Chọn file và category
4. Xác nhận import

### 3. Kiểm Soát Ứng Dụng

#### Chặn Ứng Dụng
1. Vào tab **"Application Control"**
2. Xem danh sách ứng dụng đang chạy
3. Click chuột phải → **"Block Application"**
4. Ứng dụng sẽ bị tắt ngay lập tức

#### Đặt Giới Hạn Thời Gian
1. Chọn ứng dụng từ danh sách
2. Click **"Set Time Limit"**
3. Nhập thời gian tối đa mỗi ngày (giờ:phút)
4. Click **"Apply"**

Ví dụ giới hạn:
- Game: 2 giờ/ngày
- YouTube: 1 giờ/ngày
- Facebook: 30 phút/ngày

### 4. Xem Báo Cáo

#### Báo Cáo Hàng Ngày
1. Vào tab **"Reports"**
2. Chọn ngày cần xem
3. Xem chi tiết:
   - 📱 Ứng dụng đã sử dụng
   - 🌐 Website đã truy cập
   - ⏱️ Thời gian sử dụng
   - 🚫 Số lần vi phạm

#### Export Báo Cáo
1. Click **"Export Report"**
2. Chọn định dạng (PDF, Excel, Text)
3. Lưu file để xem sau

### 5. Cấu Hình Nâng Cao

#### Lịch Trình Sử Dụng
```json
{
  "Schedule": {
    "Monday-Friday": {
      "AllowedHours": "16:00-20:00",
      "HomeworkMode": "14:00-16:00"
    },
    "Weekend": {
      "AllowedHours": "08:00-21:00",
      "BreakEvery": "2 hours"
    }
  }
}
```

#### Chế Độ Khẩn Cấp
- Nhấn `Ctrl + Shift + F12` để tạm dừng mọi hạn chế (yêu cầu mật khẩu)
- Tự động bật lại sau 30 phút

## 🏗️ Cấu Trúc Dự Án

```
KidGuard/
├── 📁 src/
│   ├── 📁 KidGuard.Core/           # Thư viện core - Models và Interfaces
│   │   ├── 📁 Models/              # Các data models
│   │   │   ├── BlockedWebsite.cs   # Model website bị chặn
│   │   │   ├── MonitoredApplication.cs # Model ứng dụng giám sát
│   │   │   └── ApplicationModels.cs # Các models khác
│   │   └── 📁 Interfaces/          # Các interface định nghĩa service
│   │       ├── IWebsiteBlockingService.cs
│   │       ├── IApplicationMonitoringService.cs
│   │       └── IActivityLoggerService.cs
│   │
│   ├── 📁 KidGuard.Services/       # Business logic layer
│   │   └── 📁 Implementation/      # Triển khai các services
│   │       ├── WebsiteBlockingService.cs      # Xử lý chặn website
│   │       ├── ApplicationMonitoringService.cs # Giám sát ứng dụng
│   │       └── ActivityLoggerService.cs       # Ghi log hoạt động
│   │
│   └── 📁 KidGuard/                # Windows Forms UI
│       ├── Program.cs              # Entry point
│       ├── 📁 Forms/               # Các form giao diện
│       │   ├── MainForm.cs         # Form chính
│       │   └── MainForm.Designer.cs
│       ├── appsettings.json        # File cấu hình
│       └── app.manifest            # Windows manifest
│
├── 📁 tests/                       # Unit tests (đang phát triển)
├── 📁 docs/                        # Tài liệu
├── 📄 README.md                    # Readme tiếng Anh
├── 📄 HUONG_DAN.md                # Hướng dẫn tiếng Việt
└── 📄 KidGuard.sln                # Solution file
```

## 🔧 API và Services

### IWebsiteBlockingService
```csharp
// Chặn một website
await blockingService.BlockWebsiteAsync("facebook.com", "Social Media");

// Bỏ chặn website
await blockingService.UnblockWebsiteAsync("facebook.com");

// Lấy danh sách website bị chặn
var blockedSites = await blockingService.GetBlockedWebsitesAsync();

// Import danh sách chặn
await blockingService.ImportBlockListAsync(domainList, "Gaming");
```

### IApplicationMonitoringService
```csharp
// Lấy ứng dụng đang chạy
var runningApps = await monitoringService.GetRunningApplicationsAsync();

// Chặn ứng dụng
await monitoringService.BlockApplicationAsync("chrome.exe", "Giờ học");

// Đặt giới hạn thời gian
await monitoringService.SetTimeLimitAsync("game.exe", TimeSpan.FromHours(2));

// Bắt đầu giám sát
await monitoringService.StartMonitoringAsync(cancellationToken);
```

### IActivityLoggerService
```csharp
// Ghi log hoạt động
await loggerService.LogApplicationLaunchAsync("notepad.exe");

// Lấy log gần đây
var recentLogs = await loggerService.GetRecentActivitiesAsync(24); // 24 giờ

// Export báo cáo
await loggerService.ExportLogsAsync("report.txt", startDate, endDate);

// Lấy thống kê
var summary = await loggerService.GetActivitySummaryAsync(startDate, endDate);
```

## ⚙️ Cấu Hình

### File appsettings.json
```json
{
  "KidGuard": {
    "Monitoring": {
      "CheckIntervalSeconds": 5,          // Kiểm tra mỗi 5 giây
      "EnableApplicationMonitoring": true, // Bật giám sát ứng dụng
      "EnableWebsiteBlocking": true,      // Bật chặn website
      "EnableActivityLogging": true       // Bật ghi log
    },
    "DataRetention": {
      "ActivityLogDays": 90,              // Giữ log 90 ngày
      "UsageStatsDays": 365               // Giữ thống kê 1 năm
    },
    "DefaultSettings": {
      "BlockSocialMediaByDefault": false,
      "BlockGamingByDefault": false,
      "BlockAdultContentByDefault": true,  // Mặc định chặn nội dung 18+
      "DailyScreenTimeLimit": "08:00:00"  // Giới hạn 8 giờ/ngày
    }
  }
}
```

### Vị Trí Lưu Dữ Liệu
- **Logs**: `C:\Users\[Username]\AppData\Local\KidGuard\Logs\`
- **Config**: `C:\Users\[Username]\AppData\Local\KidGuard\Config\`
- **Database**: `C:\Users\[Username]\AppData\Local\KidGuard\Data\`

## 🐛 Xử Lý Lỗi

### Lỗi Thường Gặp

#### 1. Lỗi "Access Denied"
**Nguyên nhân**: Không có quyền Administrator
**Giải pháp**: 
```powershell
# Chạy PowerShell as Admin
Start-Process "KidGuard.exe" -Verb RunAs
```

#### 2. Lỗi "Hosts file locked"
**Nguyên nhân**: Antivirus đang chặn
**Giải pháp**: 
- Tạm tắt antivirus
- Thêm KidGuard vào whitelist
- Hoặc chạy: `attrib -r C:\Windows\System32\drivers\etc\hosts`

#### 3. Lỗi ".NET Runtime not found"
**Nguyên nhân**: Chưa cài .NET 9
**Giải pháp**:
```powershell
# Cài .NET 9 Runtime
winget install Microsoft.DotNet.DesktopRuntime.9
```

### Debug Mode
Để bật debug mode, thêm vào appsettings.json:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

## 🤝 Đóng Góp

### Quy Trình Đóng Góp
1. Fork repository
2. Tạo branch mới: `git checkout -b feature/TenTinhNang`
3. Commit thay đổi: `git commit -m 'feat: Thêm tính năng XYZ'`
4. Push lên branch: `git push origin feature/TenTinhNang`
5. Tạo Pull Request

### Coding Standards
- Sử dụng C# 12 features
- Follow .NET naming conventions
- Viết unit tests cho features mới
- Comment code bằng tiếng Việt hoặc tiếng Anh
- Format code với: `dotnet format`

### Báo Lỗi
Tạo issue trên GitHub với:
- Mô tả lỗi chi tiết
- Các bước tái hiện
- Screenshot nếu có
- Log files từ `%LOCALAPPDATA%\KidGuard\Logs`

## 📞 Liên Hệ

- **GitHub**: [github.com/quangminh1212/KidGuard](https://github.com/quangminh1212/KidGuard)
- **Email**: support@kidguard.vn
- **Website**: https://kidguard.vn

## 📜 Giấy Phép

Dự án được phát hành theo giấy phép MIT. Xem file [LICENSE](LICENSE) để biết thêm chi tiết.

---

© 2024 KidGuard - Bảo vệ trẻ em trên không gian mạng