________________________________________
1. Mô tả công việc:
•	Phát triển và duy trì ứng dụng trên hệ điều hành Windows – phần mềm giám sát hoạt động máy tính của trẻ. 
•	Làm việc với các công nghệ như Windows Services, WinForms, Hooking API, System Events, Device Policy,...
•	Tối ưu hiệu suất và tài nguyên hệ thống cho ứng dụng chạy lâu dài.
•	Hợp tác cùng các team Backend/API để đồng bộ dữ liệu người dùng.
________________________________________
2. Yêu cầu bắt buộc:
•	Có kinh nghiệm lập trình Windows bằng C# (WinForms) từ 2 năm trở lên.
•	Có hiểu biết hoặc kinh nghiệm với C++ là lợi thế.
•	Biết sử dụng các API hệ thống Windows như Registry, Process, Services, Scheduler...
•	Biết cách tạo ứng dụng tự khởi động, chạy dưới quyền admin, hoặc thao tác với Windows Task Scheduler / GPO / Safe Mode.
•	Kinh nghiệm làm việc với Visual Studio, sử dụng Git, khả năng debug tốt.
________________________________________
3. Ưu tiên lớn nếu có:
•	Từng làm phần mềm giám sát máy tính, bảo vệ trẻ em, chống gỡ/chống kill, self-defense.
•	Có kinh nghiệm viết Windows Service, driver, hoặc từng làm antivirus, parental control, game launcher lock... 
•	Tư duy hệ thống, bảo mật, hiệu năng tốt.
# ChildGuard  

[![Release](https://github.com/quangminh1212/ChildGuard/actions/workflows/release.yml/badge.svg)](https://github.com/quangminh1212/ChildGuard/actions/workflows/release.yml)

Ứng dụng Windows (.NET 8, WinForms) để giám sát hoạt động và áp dụng chính sách bảo vệ trẻ em trên máy tính. Hỗ trợ Quiet Hours, chặn ứng dụng theo lịch, ghi log JSONL, báo cáo trực quan, tự khởi động linh hoạt, và UI song ngữ Anh/Việt với theme Sáng/Tối.

## Tính năng chính

- Giám sát hoạt động hệ thống
  - Theo dõi bàn phím/chuột (Low-Level hook, bật/tắt được trong cài đặt)
  - Theo dõi cửa sổ đang hoạt động (tên tiến trình/tiêu đề)
  - Ghi nhận tiến trình khởi động/thoát (ProcessStart/ProcessStop)
  - Ghi nhận cắm/rút thiết bị USB
- Chính sách & khung giờ yên lặng (Quiet Hours)
  - Cấu hình QuietHoursStart/QuietHoursEnd theo định dạng HH:mm; hỗ trợ nhiều khung giờ bổ sung (AdditionalQuietWindows)
  - Danh sách chặn (BlockedProcesses) và danh sách cho phép trong Quiet Hours (AllowedProcessesDuringQuietHours)
  - Quy tắc nâng cao theo ngày/giờ (PolicyRules) cho phép/ chặn theo lịch linh hoạt
  - Thực thi mềm với cảnh báo và đếm ngược đóng ứng dụng; giãn cách 30 giây để tránh spam
- Ghi log và lưu trữ
  - Ghi sự kiện dạng JSON Lines theo ngày (events-YYYYMMDD.jsonl)
  - Dọn dẹp log tự động theo số ngày (LogRetentionDays) và giới hạn dung lượng (LogMaxSizeMB)
  - Tự động tải lại cấu hình khi file cấu hình thay đổi (debounce)
- Giao diện người dùng
  - Cửa sổ Settings: cấu hình giám sát, Quiet Hours, blocked/allowed, lưu trữ log; chọn ngôn ngữ (EN/VI) và Theme (System/Light/Dark)
  - Cửa sổ Reports: lọc theo ngày/khoảng thời gian/loại sự kiện/tên tiến trình; Group by hour; xuất CSV và PNG biểu đồ
  - Policy Editor: chỉnh sửa PolicyRules (JSON) với định dạng đẹp
  - UI hiện đại, bo góc nhẹ, icon Segoe, dùng accent Windows; hỗ trợ Dark Mode
- Tự khởi động & cài đặt
  - Installer Inno Setup (self-contained): cấu hình tự khởi động dùng Scheduled Task (All Users/Current User), fallback HKCU\Run nếu bị chặn
  - Script gỡ tự khởi động (xóa Run key); hỗ trợ cài như Windows Service (tùy chọn)
- Khác
  - Thư mục dữ liệu mặc định ProgramData; tự động dùng LocalAppData khi không có quyền
  - Quy trình CI/CD GitHub Actions: build, tạo release, upload installer

---

## Key Features (EN)

- Activity monitoring: low-level keyboard/mouse (opt-in), active window, process start/stop, USB device changes
- Policy & Quiet Hours: main and additional time windows; block/allow lists; advanced day-of-week/time rules; friendly countdown warnings with anti-spam
- Logging & retention: daily JSONL; auto-clean by days and by max size; hot-reload config on file changes
- UI: Settings (incl. language EN/VI and System/Light/Dark theme), Reports (filters, group-by-hour, CSV/PNG export), Policy Editor (JSON)
- Autostart & installer: Inno Setup; Scheduled Task (all-users/current-user) with HKCU Run fallback; uninstall scripts; optional Windows Service
- Modern look: rounded sections, Segoe icons, Windows accent integration, Dark Mode
- Data directory fallback: ProgramData preferred, LocalAppData when lacking permissions
- CI/CD: GitHub Actions workflow to build artifacts and publish releases

---

## Quick Start / Cài đặt nhanh

- Yêu cầu: Windows 10/11, .NET SDK 8 (để build), Inno Setup 6 (nếu build installer)
- Build giải pháp và chạy UI thử nhanh:

```
# Build (Debug)
dotnet build ChildGuard.sln -c Debug
# Chạy UI
ChildGuard.UI\bin\Debug\net8.0-windows\ChildGuard.UI.exe
```

- Chạy Agent (dev) nếu cần:
```
ChildGuard.Agent\bin\Debug\net8.0-windows\ChildGuard.Agent.exe
```

- Build installer (self-contained) và tạo file cài đặt trong dist/:
```
powershell -ExecutionPolicy Bypass -File scripts\build_installer.ps1 -Configuration Release -Rid win-x64 -Version 1.0.4
```

- Cấu hình tự khởi động Agent sau khi cài đặt (thực tế installer đã làm, đây là hướng dẫn thủ công nếu cần):
```
# All users (yêu cầu quyền phù hợp)
powershell -ExecutionPolicy Bypass -File installer\tools\ensure_agent_autostart.ps1 -Mode allusers -TaskName ChildGuardAgent -ExePath "C:\Program Files\ChildGuard\Agent\ChildGuard.Agent.exe"
# Current user (fallback HKCU Run nếu tạo task bị chặn)
powershell -ExecutionPolicy Bypass -File installer\tools\ensure_agent_autostart.ps1 -Mode current -TaskName ChildGuardAgent -ExePath "C:\Program Files\ChildGuard\Agent\ChildGuard.Agent.exe"
```

- Gỡ tự khởi động (ví dụ trong môi trường dev):
```
powershell -ExecutionPolicy Bypass -File scripts\uninstall_agent_autostart.ps1
```

### Sử dụng nhanh

1) Mở UI -> Settings để chọn Language (EN/VI) và Theme (System/Light/Dark)
2) Cấu hình Quiet Hours, Blocked/Allowed, Log retention/size; Save -> config lưu tại ProgramData hoặc LocalAppData
3) Reports -> lọc theo ngày/khoảng giờ/loại sự kiện/tên process, xuất CSV hoặc PNG
4) Policy Editor -> chỉnh PolicyRules (JSON) theo lịch nâng cao
5) Khởi chạy Agent (hoặc dùng autostart) -> theo dõi hoạt động và thực thi chính sách

### Vị trí dữ liệu

- Config: C:/ProgramData/ChildGuard/config.json (ưu tiên) hoặc %LOCALAPPDATA%/ChildGuard/config.json
- Logs: [DataDirectory]/logs/events-YYYYMMDD.jsonl

---

## Troubleshooting

- Không tạo được Scheduled Task (Access Denied / 0x41306):
  - Thử lại chế độ Current User trong installer hoặc dùng lệnh `ensure_agent_autostart.ps1 -Mode current`
  - Nếu vẫn bị chặn, script sẽ fallback tự động sang HKCU\Run; kiểm tra khóa `HKCU:Software\Microsoft\Windows\CurrentVersion\Run` tên `ChildGuardAgent`
- PowerShell ExecutionPolicy chặn script:
  - Chạy các script kèm `-ExecutionPolicy Bypass` hoặc mở PowerShell với quyền phù hợp
- Inno Setup không có ISCC.exe trong PATH:
  - Cài Inno Setup 6, sau đó chạy lại `scripts/build_installer.ps1` hoặc thêm đường dẫn ISCC vào PATH
- UI không hiện lên phía trước:
  - Chạy `scripts/bring_ui_front.ps1` để đưa cửa sổ UI ra foreground
- Không ghi log hoặc không thấy file config:
  - Kiểm tra quyền ghi tại `C:/ProgramData/ChildGuard` hoặc `%LOCALAPPDATA%/ChildGuard`

## Screenshots

- Ảnh giao diện chính (ví dụ):

![Main UI](docs/screenshots/childguard_ui_main.png)

- Settings:

![Settings](docs/screenshots/childguard_settings.png)

- Reports:

![Reports](docs/screenshots/childguard_reports.png)

- Tạo nhanh ảnh chụp UI (dev):
```
powershell -ExecutionPolicy Bypass -File scripts\capture_ui_screenshot.ps1
```
Ảnh sẽ lưu vào docs/screenshots/childguard_ui_main.png. Bạn có thể thêm ảnh khác vào thư mục docs/screenshots/ và tham chiếu trong README.
