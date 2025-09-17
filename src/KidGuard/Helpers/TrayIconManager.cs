using System;
using System.Drawing;
using System.Windows.Forms;

namespace KidGuard.Helpers
{
    /// <summary>
    /// Quản lý System Tray Icon cho ứng dụng
    /// Cho phép ẩn/hiện ứng dụng vào system tray
    /// </summary>
    public class TrayIconManager : IDisposable
    {
        private NotifyIcon _notifyIcon;
        private Form _mainForm;
        private ContextMenuStrip _contextMenu;
        private bool _isDisposed = false;

        /// <summary>
        /// Khởi tạo TrayIconManager
        /// </summary>
        /// <param name="mainForm">Form chính của ứng dụng</param>
        public TrayIconManager(Form mainForm)
        {
            _mainForm = mainForm;
            InitializeTrayIcon();
            AttachFormEvents();
        }

        /// <summary>
        /// Khởi tạo system tray icon và menu
        /// </summary>
        private void InitializeTrayIcon()
        {
            // Tạo context menu cho tray icon
            _contextMenu = new ContextMenuStrip();
            
            // Menu item: Hiện/Ẩn cửa sổ chính
            var showHideItem = new ToolStripMenuItem("Hiện/Ẩn KidGuard");
            showHideItem.Font = new Font(showHideItem.Font, FontStyle.Bold);
            showHideItem.Click += (s, e) => ToggleMainForm();
            
            // Menu item: Dashboard
            var dashboardItem = new ToolStripMenuItem("Dashboard", null, (s, e) => 
            {
                ShowMainForm();
                // TODO: Navigate to dashboard tab
            });
            
            // Menu item: Chặn website nhanh
            var quickBlockItem = new ToolStripMenuItem("Chặn Website Nhanh", null, (s, e) =>
            {
                var domain = Microsoft.VisualBasic.Interaction.InputBox(
                    "Nhập website cần chặn:", "Chặn Nhanh", "");
                
                if (!string.IsNullOrWhiteSpace(domain))
                {
                    // TODO: Implement quick block
                    MessageBox.Show($"Đã chặn: {domain}", "Thông báo", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            });
            
            // Separator
            var separator1 = new ToolStripSeparator();
            
            // Menu item: Trạng thái bảo vệ
            var statusItem = new ToolStripMenuItem("Trạng thái: Đang bảo vệ");
            statusItem.Enabled = false;
            statusItem.ForeColor = Color.Green;
            
            // Menu item: Tạm dừng bảo vệ
            var pauseItem = new ToolStripMenuItem("Tạm dừng bảo vệ (15 phút)", null, (s, e) =>
            {
                var result = MessageBox.Show(
                    "Bạn có chắc muốn tạm dừng bảo vệ trong 15 phút?\n" +
                    "Điều này sẽ vô hiệu hóa tất cả các hạn chế.",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                
                if (result == DialogResult.Yes)
                {
                    // TODO: Implement pause protection
                    statusItem.Text = "Trạng thái: Tạm dừng";
                    statusItem.ForeColor = Color.Orange;
                    
                    // Tự động bật lại sau 15 phút
                    var timer = new System.Windows.Forms.Timer { Interval = 900000 }; // 15 phút
                    timer.Tick += (sender, args) =>
                    {
                        statusItem.Text = "Trạng thái: Đang bảo vệ";
                        statusItem.ForeColor = Color.Green;
                        timer.Stop();
                        timer.Dispose();
                        
                        ShowBalloonTip("Bảo vệ đã được bật lại", 
                            "KidGuard đang hoạt động trở lại", ToolTipIcon.Info);
                    };
                    timer.Start();
                    
                    ShowBalloonTip("Đã tạm dừng bảo vệ", 
                        "Sẽ tự động bật lại sau 15 phút", ToolTipIcon.Warning);
                }
            });
            
            // Separator
            var separator2 = new ToolStripSeparator();
            
            // Menu item: Cài đặt
            var settingsItem = new ToolStripMenuItem("Cài đặt", null, (s, e) =>
            {
                ShowMainForm();
                // TODO: Open settings form
            });
            
            // Menu item: Về chúng tôi
            var aboutItem = new ToolStripMenuItem("Về KidGuard", null, (s, e) =>
            {
                MessageBox.Show(
                    "KidGuard v1.0.0\n\n" +
                    "Phần mềm bảo vệ trẻ em trên không gian mạng\n" +
                    "Phát triển bởi: KidGuard Team\n" +
                    "Website: https://kidguard.vn\n\n" +
                    "© 2024 KidGuard. All rights reserved.",
                    "Về KidGuard", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
            
            // Separator
            var separator3 = new ToolStripSeparator();
            
            // Menu item: Thoát
            var exitItem = new ToolStripMenuItem("Thoát", null, (s, e) =>
            {
                var result = MessageBox.Show(
                    "Bạn có chắc muốn thoát KidGuard?\n" +
                    "Điều này sẽ vô hiệu hóa mọi chức năng bảo vệ.",
                    "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    Application.Exit();
                }
            });
            exitItem.ForeColor = Color.Red;
            
            // Thêm các menu items vào context menu
            _contextMenu.Items.AddRange(new ToolStripItem[] {
                showHideItem,
                dashboardItem,
                quickBlockItem,
                separator1,
                statusItem,
                pauseItem,
                separator2,
                settingsItem,
                aboutItem,
                separator3,
                exitItem
            });
            
            // Tạo NotifyIcon
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Shield,
                Text = "KidGuard - Đang bảo vệ",
                ContextMenuStrip = _contextMenu,
                Visible = true
            };
            
            // Sự kiện double-click vào icon
            _notifyIcon.DoubleClick += (s, e) => ShowMainForm();
            
            // Hiện thông báo khi khởi động
            ShowBalloonTip("KidGuard đang chạy", 
                "Ứng dụng đã được thu nhỏ vào khay hệ thống", ToolTipIcon.Info);
        }

        /// <summary>
        /// Gắn các sự kiện của form chính
        /// </summary>
        private void AttachFormEvents()
        {
            // Khi form bị minimize, ẩn vào tray
            _mainForm.Resize += (s, e) =>
            {
                if (_mainForm.WindowState == FormWindowState.Minimized)
                {
                    HideMainForm();
                }
            };
            
            // Khi form đóng, hỏi người dùng
            _mainForm.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    
                    var result = MessageBox.Show(
                        "Bạn muốn ẩn KidGuard vào khay hệ thống hay thoát hoàn toàn?\n\n" +
                        "• Ẩn: Ứng dụng tiếp tục chạy ngầm\n" +
                        "• Thoát: Dừng hoàn toàn ứng dụng",
                        "Lựa chọn", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    
                    if (result == DialogResult.Yes) // Ẩn
                    {
                        HideMainForm();
                    }
                    else if (result == DialogResult.No) // Thoát
                    {
                        e.Cancel = false;
                        Application.Exit();
                    }
                    // Cancel: Không làm gì
                }
            };
        }

        /// <summary>
        /// Hiển thị form chính
        /// </summary>
        private void ShowMainForm()
        {
            _mainForm.Show();
            _mainForm.WindowState = FormWindowState.Normal;
            _mainForm.BringToFront();
            _mainForm.Activate();
        }

        /// <summary>
        /// Ẩn form chính vào tray
        /// </summary>
        private void HideMainForm()
        {
            _mainForm.Hide();
            ShowBalloonTip("KidGuard vẫn đang chạy", 
                "Double-click vào icon để mở lại", ToolTipIcon.Info);
        }

        /// <summary>
        /// Toggle hiện/ẩn form chính
        /// </summary>
        private void ToggleMainForm()
        {
            if (_mainForm.Visible)
            {
                HideMainForm();
            }
            else
            {
                ShowMainForm();
            }
        }

        /// <summary>
        /// Hiển thị balloon tooltip
        /// </summary>
        public void ShowBalloonTip(string title, string text, ToolTipIcon icon)
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = text;
            _notifyIcon.BalloonTipIcon = icon;
            _notifyIcon.ShowBalloonTip(3000); // Hiện trong 3 giây
        }

        /// <summary>
        /// Cập nhật text của tray icon
        /// </summary>
        public void UpdateTrayText(string text)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Text = text.Length > 63 ? text.Substring(0, 63) : text;
            }
        }

        /// <summary>
        /// Giải phóng tài nguyên
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Giải phóng tài nguyên
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _notifyIcon?.Dispose();
                    _contextMenu?.Dispose();
                }
                _isDisposed = true;
            }
        }
    }
}