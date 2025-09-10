# ChildGuard - Advanced Child Protection System

ChildGuard là một hệ thống bảo vệ trẻ em tiên tiến được thiết kế để theo dõi và bảo vệ trẻ em khỏi nội dung không phù hợp trên máy tính Windows.

## 🛡️ Tính năng chính

### Giám sát thời gian thực
- **Keylogger an toàn**: Theo dõi các phím được gõ để phát hiện nội dung không phù hợp
- **Phân tích nội dung**: Sử dụng AI để phân tích và phân loại nội dung
- **Cảnh báo tức thì**: Thông báo ngay lập tức khi phát hiện nội dung có vấn đề

### Bộ lọc nội dung thông minh
- **Từ điển đa ngôn ngữ**: Hỗ trợ tiếng Việt và tiếng Anh
- **Phân loại mức độ nghiêm trọng**: Low, Medium, High, Critical
- **Phân tích ngữ cảnh**: Hiểu được ngữ cảnh để giảm false positive
- **Tùy chỉnh bộ lọc**: Cho phép thêm/xóa từ khóa tùy chỉnh

### Bảo mật và quyền riêng tư
- **Mã hóa dữ liệu**: Tất cả dữ liệu được mã hóa AES-256
- **Lưu trữ cục bộ**: Dữ liệu chỉ được lưu trên máy tính cá nhân
- **Tuân thủ COPPA/GDPR**: Thiết kế theo các tiêu chuẩn bảo vệ trẻ em quốc tế
- **Kiểm soát truy cập**: Xác thực phụ huynh và phân quyền người dùng

### 🎨 Giao diện hiện đại
- **Thiết kế Glassmorphism**: Hiệu ứng kính mờ với backdrop blur chuyên nghiệp
- **Gradient và Animation**: Màu sắc gradient đẹp mắt với animation mượt mà
- **Dark/Light Mode**: Chuyển đổi theme linh hoạt theo sở thích người dùng
- **Responsive Design**: Tối ưu cho mọi kích thước màn hình
- **Component Library**: Thư viện component tái sử dụng với styling nhất quán

### Dashboard quản lý
- **Dashboard chuyên nghiệp**: Thống kê trực quan với animated cards
- **Báo cáo chi tiết**: Báo cáo hàng ngày, tuần, tháng với biểu đồ đẹp
- **Quản lý hồ sơ trẻ**: Interface trực quan để tạo và quản lý hồ sơ
- **Cài đặt linh hoạt**: Form hiện đại với glassmorphism effects

## 🚀 Cài đặt và sử dụng

### Yêu cầu hệ thống
- Windows 10/11 (64-bit)
- Node.js 18+ (cho development)
- RAM: 4GB trở lên
- Dung lượng: 500MB trống

### Cài đặt cho người dùng cuối
1. Tải file installer từ [Releases](https://github.com/your-repo/releases)
2. Chạy `ChildGuard-Setup.exe` với quyền Administrator
3. Làm theo hướng dẫn cài đặt
4. Khởi động ứng dụng và đăng nhập với tài khoản mặc định

### Tài khoản mặc định
```
Username: admin
Password: admin123
```
**⚠️ Quan trọng**: Thay đổi mật khẩu mặc định sau lần đăng nhập đầu tiên!

## 🛠️ Development

### Cài đặt môi trường phát triển
```bash
# Clone repository
git clone https://github.com/your-repo/childguard.git
cd childguard

# Cài đặt dependencies
npm install

# Chạy ở chế độ development
npm run dev

# Build production
npm run build

# Tạo installer
npm run dist
```

### Cấu trúc dự án
```
src/
├── main/                 # Electron main process
│   ├── services/        # Core services (Keylogger, Filter, etc.)
│   ├── database/        # Database management
│   ├── config/          # Configuration management
│   └── utils/           # Utilities
├── renderer/            # React frontend
│   ├── components/      # UI components
│   ├── pages/          # Application pages
│   ├── contexts/       # React contexts
│   └── hooks/          # Custom hooks
└── shared/             # Shared types and constants
```

### Tech Stack
- **Frontend**: React 18, TypeScript, Material-UI v5, Framer Motion
- **Design**: Glassmorphism, Inter Font, Custom Theme System
- **Backend**: Electron, Node.js
- **Database**: SQLite với mã hóa AES-256
- **Build**: Webpack, Electron Builder
- **Security**: bcrypt, crypto, node-forge

## 📋 Hướng dẫn sử dụng

### 1. Đăng nhập lần đầu
- Sử dụng tài khoản admin mặc định
- Thay đổi mật khẩu ngay lập tức
- Cấu hình email thông báo (tùy chọn)

### 2. Tạo hồ sơ trẻ em
- Vào mục "Children" → "Add Child"
- Nhập thông tin: tên, tuổi, mức độ hạn chế
- Cấu hình giờ sử dụng máy tính cho phép

### 3. Bắt đầu giám sát
- Chọn trẻ cần giám sát
- Nhấn "Start Monitoring"
- Hệ thống sẽ bắt đầu theo dõi hoạt động

### 4. Xem báo cáo và cảnh báo
- Dashboard: Tổng quan hoạt động hàng ngày
- Alerts: Danh sách cảnh báo theo thời gian thực
- Reports: Báo cáo chi tiết theo ngày/tuần/tháng

## ⚙️ Cấu hình nâng cao

### Tùy chỉnh bộ lọc nội dung
```javascript
// Thêm từ khóa tùy chỉnh
const customRule = {
  pattern: "từ_khóa_mới",
  category: "custom",
  severity: "medium",
  isRegex: false
};
```

### Cài đặt thông báo email
1. Vào Settings → Notifications
2. Bật "Email notifications"
3. Nhập thông tin SMTP server
4. Test cấu hình

### Backup và khôi phục dữ liệu
```bash
# Backup tự động hàng ngày
# File backup: %USERPROFILE%/AppData/Roaming/ChildGuard/backups/

# Khôi phục thủ công
Settings → Backup & Data → Restore from Backup
```

## 🔒 Bảo mật và quyền riêng tư

### Mã hóa dữ liệu
- Tất cả keystroke được mã hóa AES-256-GCM
- Mật khẩu được hash với bcrypt (12 rounds)
- Database được mã hóa toàn bộ

### Quyền riêng tư
- Dữ liệu chỉ lưu trữ cục bộ
- Không gửi dữ liệu lên internet
- Tuân thủ COPPA và GDPR
- Có thể xóa dữ liệu hoàn toàn

### Kiểm soát truy cập
- Xác thực phụ huynh bắt buộc
- Session timeout tự động
- Khóa tài khoản sau nhiều lần đăng nhập sai
- Audit log đầy đủ

## 📊 Giám sát và báo cáo

### Metrics được theo dõi
- Tổng số keystroke hàng ngày
- Số lượng cảnh báo theo mức độ
- Thời gian sử dụng máy tính
- Ứng dụng được sử dụng nhiều nhất

### Loại báo cáo
- **Daily Report**: Hoạt động trong ngày
- **Weekly Summary**: Tổng kết tuần
- **Monthly Analytics**: Phân tích xu hướng tháng
- **Custom Reports**: Báo cáo tùy chỉnh theo khoảng thời gian

## 🆘 Hỗ trợ và khắc phục sự cố

### Vấn đề thường gặp

**Q: Ứng dụng không khởi động được?**
A: Chạy với quyền Administrator và kiểm tra Windows Defender

**Q: Keylogger không hoạt động?**
A: Kiểm tra quyền Administrator và tắt antivirus tạm thời

**Q: Quên mật khẩu admin?**
A: Xóa file config.json trong thư mục userData và khởi động lại

### Log files
```
%USERPROFILE%/AppData/Roaming/ChildGuard/logs/
├── childguard.log      # General logs
├── error.log          # Error logs
├── security.log       # Security events
└── audit.log          # User actions
```

## 📄 License

MIT License - Xem file [LICENSE](LICENSE) để biết chi tiết.

## 🤝 Đóng góp

Chúng tôi hoan nghênh mọi đóng góp! Vui lòng đọc [CONTRIBUTING.md](CONTRIBUTING.md) trước khi submit PR.

## ⚠️ Lưu ý quan trọng

- Sử dụng phần mềm này một cách có trách nhiệm
- Tuân thủ luật pháp địa phương về quyền riêng tư
- Thông báo cho trẻ em về việc giám sát (khuyến nghị)
- Không sử dụng để theo dõi người khác mà không có sự đồng ý

## 📞 Liên hệ

- Email: support@childguard.com
- Website: https://childguard.com
- Issues: https://github.com/your-repo/issues

---

**ChildGuard v1.0.0** - Bảo vệ trẻ em trong thế giới số 🛡️
