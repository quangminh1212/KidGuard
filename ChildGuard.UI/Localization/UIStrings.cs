using System.Globalization;

namespace ChildGuard.UI.Localization;

public static class UIStrings
{
    public const string DefaultLanguage = "en";
    private static string _current = DefaultLanguage;

    public static string CurrentLanguage => _current;

    public static void SetLanguage(string? lang)
    {
        _current = string.Equals(lang, "vi", StringComparison.OrdinalIgnoreCase) ? "vi" : "en";
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(_current == "vi" ? "vi-VN" : "en-US");
        }
        catch { }
    }

    private static readonly Dictionary<string, (string en, string vi)> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        // App & Menus
        ["App.Title"] = ("ChildGuard UI", "ChildGuard UI"),
        ["Menu.Settings"] = ("Settings", "Cài đặt"),
        ["Menu.Reports"] = ("Reports", "Báo cáo"),
        ["Menu.PolicyEditor"] = ("Policy Editor", "Trình sửa chính sách"),

        // Common Buttons
        ["Buttons.Start"] = ("Start", "Bắt đầu"),
        ["Buttons.Stop"] = ("Stop", "Dừng"),
        ["Buttons.Save"] = ("Save", "Lưu"),
        ["Buttons.Cancel"] = ("Cancel", "Hủy"),
        ["Buttons.ExportCsv"] = ("Export CSV", "Xuất CSV"),
        ["Buttons.ExportChart"] = ("Export Chart", "Xuất biểu đồ"),
        ["Buttons.TrendChart"] = ("Trend Chart", "Biểu đồ xu hướng"),
        ["Buttons.OpenConfig"] = ("Open config", "Mở cấu hình"),

        // Form1 (main UI)
        ["Form1.EnableInput"] = ("Enable input monitoring (keyboard/mouse)", "Bật theo dõi bàn phím/chuột"),
        ["Labels.KeysCount"] = ("KeyPress Count: {0}", "Số lần nhấn phím: {0}"),
        ["Labels.MouseCount"] = ("Mouse Event Count: {0}", "Số sự kiện chuột: {0}"),
        ["Form1.Section.Activity"] = ("Activity", "Hoạt động"),
        ["Form1.Section.Controls"] = ("Controls", "Điều khiển"),

        // Settings
        ["Settings.Title"] = ("Settings", "Cài đặt"),
        ["Settings.EnableInput"] = ("Enable input monitoring (keyboard/mouse)", "Bật theo dõi bàn phím/chuột"),
        ["Settings.EnableActiveWindow"] = ("Enable active window log", "Bật nhật ký cửa sổ hoạt động"),
        ["Settings.Blocked"] = ("Blocked processes (one per line):", "Chặn tiến trình (mỗi dòng một mục):"),
        ["Settings.AllowedQuiet"] = ("Allowed processes during Quiet Hours (one per line):", "Tiến trình được phép trong Giờ yên lặng (mỗi dòng):"),
        ["Settings.Quiet"] = ("Quiet hours (local time):", "Khung giờ yên lặng (theo giờ máy):"),
        ["Settings.Retention"] = ("Log retention (days, >=1, default 14):", "Số ngày lưu log (>=1, mặc định 14):"),
        ["Settings.CloseWarn"] = ("Block close warning (seconds):", "Cảnh báo đóng ứng dụng (giây):"),
        ["Settings.MaxSize"] = ("Log max size (MB):", "Dung lượng log tối đa (MB):"),
        ["Settings.AdditionalQuiet"] = ("Additional quiet windows (HH:mm-HH:mm, per line):", "Khung giờ yên lặng bổ sung (HH:mm-HH:mm, mỗi dòng):"),
        ["Settings.ConfigPath"] = ("Config path: {0}", "Đường dẫn cấu hình: {0}"),
        ["Settings.Language"] = ("Language", "Ngôn ngữ"),
        ["Settings.Language.English"] = ("English", "Tiếng Anh"),
        ["Settings.Language.Vietnamese"] = ("Vietnamese", "Tiếng Việt"),
        ["Settings.LanguageChanged"] = ("Language will apply next time you open the window.", "Ngôn ngữ sẽ áp dụng khi bạn mở lại cửa sổ."),
        ["Settings.Theme"] = ("Theme", "Giao diện"),
        ["Settings.Theme.System"] = ("System", "Theo hệ thống"),
        ["Settings.Theme.Light"] = ("Light", "Sáng"),
        ["Settings.Theme.Dark"] = ("Dark", "Tối"),
        ["Settings.Section.General"] = ("General", "Chung"),
        ["Settings.Section.Blocked"] = ("Blocked processes", "Chặn tiến trình"),
        ["Settings.Section.AllowedQuiet"] = ("Allowed during Quiet Hours", "Được phép trong Giờ yên lặng"),
        ["Settings.Section.QuietHours"] = ("Quiet Hours", "Khung giờ yên lặng"),
        ["Settings.Section.Retention"] = ("Log retention & limits", "Lưu trữ & giới hạn log"),
        ["Settings.Section.AdditionalQuiet"] = ("Additional quiet windows", "Khung giờ bổ sung"),
        ["Settings.Section.Config"] = ("Configuration", "Cấu hình"),

        // Reports
        ["Reports.Title"] = ("Reports", "Báo cáo"),
        ["Reports.Section.Filters"] = ("Filters", "Bộ lọc"),
        ["Reports.Section.Table"] = ("Events", "Sự kiện"),
        ["Reports.Section.Counts"] = ("Counts", "Tần suất"),
        ["Reports.Section.Trend"] = ("Trend by day", "Xu hướng theo ngày"),
        ["Reports.Load"] = ("Load", "Tải"),
        ["Reports.ProcessFilter"] = ("Process filter", "Lọc tiến trình"),
        ["Reports.GroupByHour"] = ("Group by hour", "Nhóm theo giờ"),
        ["Reports.DateTo"] = ("To", "Đến"),
        ["Reports.TimeFilter"] = ("Time filter", "Lọc theo thời gian"),
        ["Reports.TimeFrom"] = ("From", "Từ"),
        ["Reports.TimeTo"] = ("To", "Đến"),
        ["Reports.Timestamp"] = ("Timestamp", "Thời gian"),
        ["Reports.Type"] = ("Type", "Loại"),
        ["Reports.Data"] = ("Data", "Dữ liệu"),
        ["Reports.SummaryPrefix"] = ("Summary", "Tổng kết"),
        ["Reports.SummaryTotal"] = ("total={0}", "tổng={0}"),
        ["Reports.NoDataExport"] = ("No data to export.", "Không có dữ liệu để xuất."),
        ["Reports.ExportedCsv"] = ("CSV exported.", "Đã xuất CSV."),
        ["Reports.ExportError"] = ("Export error: {0}", "Lỗi xuất: {0}"),
        ["Reports.PngExported"] = ("PNG chart exported.", "Đã xuất biểu đồ PNG."),
        ["Reports.PngTrendExported"] = ("PNG trend chart exported.", "Đã xuất biểu đồ xu hướng PNG."),
        ["Reports.TrendNoData"] = ("No trend data to export.", "Không có dữ liệu xu hướng để xuất."),
        ["Reports.Event.All"] = ("All", "Tất cả"),
        ["Reports.Event.Keyboard"] = ("Keyboard", "Bàn phím"),
        ["Reports.Event.Mouse"] = ("Mouse", "Chuột"),
        ["Reports.Event.ActiveWindow"] = ("ActiveWindow", "Cửa sổ hoạt động"),
        ["Reports.Event.ProcessStart"] = ("ProcessStart", "Bắt đầu tiến trình"),
        ["Reports.Event.ProcessStop"] = ("ProcessStop", "Dừng tiến trình"),
        ["Reports.Event.UsbDeviceChange"] = ("UsbDeviceChange", "Thay đổi USB"),

        // Policy Editor
        ["Policy.Title"] = ("Policy Editor", "Trình sửa chính sách"),
        ["Policy.ConfigPath"] = ("Config path: {0}", "Đường dẫn cấu hình: {0}"),
        ["Policy.InvalidJson"] = ("Invalid JSON: {0}", "JSON không hợp lệ: {0}"),

        // General
        ["General.AppName"] = ("ChildGuard", "ChildGuard"),
    };

    public static string Get(string key)
    {
        if (Map.TryGetValue(key, out var v))
        {
            return _current == "vi" ? v.vi : v.en;
        }
        return key;
    }

    public static string Format(string key, params object[] args)
        => string.Format(Get(key), args);
}

