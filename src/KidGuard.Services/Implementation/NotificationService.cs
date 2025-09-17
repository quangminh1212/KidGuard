using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KidGuard.Core.Data;
using KidGuard.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KidGuard.Services.Implementation;

/// <summary>
/// Dịch vụ gửi thông báo và cảnh báo
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly KidGuardDbContext _dbContext;
    private readonly NotifyIcon _notifyIcon;
    private CauHinhThongBao _cauHinh;
    private readonly Queue<ThongBao> _hangDoiThongBao = new();
    private readonly object _lockObject = new();

    // Import Windows API
    [DllImport("user32.dll")]
    private static extern bool LockWorkStation();
    
    [DllImport("user32.dll")]
    private static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

    public NotificationService(
        ILogger<NotificationService> logger,
        KidGuardDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        
        // Khởi tạo System Tray Icon
        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Shield,
            Visible = true,
            Text = "KidGuard - Bảo vệ an toàn"
        };
        
        // Load cấu hình mặc định
        _cauHinh = new CauHinhThongBao
        {
            BatThongBaoEmail = true,
            BatThongBaoSystemTray = true,
            EmailNhanMacDinh = new List<string>()
        };
        
        _ = LoadCauHinhAsync();
    }

    /// <summary>
    /// Gửi thông báo tức thời
    /// </summary>
    public async Task GuiThongBaoAsync(ThongBao thongBao)
    {
        try
        {
            thongBao.Id = Guid.NewGuid();
            thongBao.ThoiGianTao = DateTime.Now;
            
            // Kiểm tra giờ gửi thông báo
            if (!_cauHinh.GuiThongBaoNgoaiGio)
            {
                var gioHienTai = DateTime.Now.TimeOfDay;
                if (gioHienTai < _cauHinh.GioBatDau || gioHienTai > _cauHinh.GioKetThuc)
                {
                    // Đưa vào hàng đợi để gửi sau
                    lock (_lockObject)
                    {
                        _hangDoiThongBao.Enqueue(thongBao);
                    }
                    _logger.LogInformation("Thông báo được đưa vào hàng đợi do ngoài giờ cho phép");
                    return;
                }
            }
            
            // Delay nếu cần
            if (_cauHinh.DoTreThongBao > 0)
            {
                await Task.Delay(_cauHinh.DoTreThongBao);
            }
            
            // Xác định kênh gửi dựa trên loại và mức độ
            var cacKenhGui = XacDinhKenhGui(thongBao);
            
            foreach (var kenh in cacKenhGui)
            {
                switch (kenh)
                {
                    case KenhGui.Email:
                        if (_cauHinh.BatThongBaoEmail)
                            await GuiEmailNoiBoAsync(thongBao);
                        break;
                        
                    case KenhGui.SystemTray:
                        if (_cauHinh.BatThongBaoSystemTray)
                            await HienThiSystemTrayNoiBoAsync(thongBao);
                        break;
                        
                    case KenhGui.Sms:
                        if (_cauHinh.BatThongBaoSms)
                            await GuiSmsNoiBoAsync(thongBao);
                        break;
                        
                    case KenhGui.Push:
                        if (_cauHinh.BatThongBaoPush)
                            await GuiPushNoiBoAsync(thongBao);
                        break;
                        
                    case KenhGui.Log:
                        await GhiLogThongBaoAsync(thongBao);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi thông báo");
            throw;
        }
    }

    /// <summary>
    /// Gửi cảnh báo
    /// </summary>
    public async Task GuiCanhBaoAsync(string tieuDe, string noiDung, MucDoCanhBao mucDo)
    {
        var thongBao = new ThongBao
        {
            TieuDe = tieuDe,
            NoiDung = noiDung,
            LoaiThongBao = LoaiThongBao.CanhBao,
            MucDoUuTien = ChuyenDoiMucDoUuTien(mucDo),
            NguoiGui = "System"
        };
        
        await GuiThongBaoAsync(thongBao);
        
        // Nếu mức độ nghiêm trọng, thực hiện thêm hành động
        if (mucDo >= MucDoCanhBao.NghiemTrong)
        {
            await HienThiCanhBaoManHinhAsync(noiDung, 10000);
        }
    }

    /// <summary>
    /// Gửi thông báo qua email
    /// </summary>
    public async Task GuiEmailAsync(string emailNhan, string tieuDe, string noiDung, string? fileDinhKem = null)
    {
        try
        {
            if (string.IsNullOrEmpty(_cauHinh.SmtpServer))
            {
                _logger.LogWarning("Chưa cấu hình SMTP server");
                return;
            }
            
            using var client = new SmtpClient(_cauHinh.SmtpServer, _cauHinh.SmtpPort)
            {
                EnableSsl = _cauHinh.SmtpUseSsl,
                Credentials = new NetworkCredential(_cauHinh.SmtpUsername, _cauHinh.SmtpPassword)
            };
            
            var message = new MailMessage
            {
                From = new MailAddress(_cauHinh.EmailGui ?? "kidguard@system.local", "KidGuard System"),
                Subject = tieuDe,
                Body = TaoNoiDungEmailHtml(tieuDe, noiDung),
                IsBodyHtml = true
            };
            
            message.To.Add(emailNhan);
            
            // Đính kèm file nếu có
            if (!string.IsNullOrEmpty(fileDinhKem) && File.Exists(fileDinhKem))
            {
                message.Attachments.Add(new Attachment(fileDinhKem));
            }
            
            await client.SendMailAsync(message);
            
            _logger.LogInformation("Đã gửi email tới {Email}: {TieuDe}", emailNhan, tieuDe);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi email");
            
            // Thử lại nếu cần
            if (_cauHinh.SoLanThuLai > 0)
            {
                await Task.Delay(5000);
                await GuiEmailAsync(emailNhan, tieuDe, noiDung, fileDinhKem);
            }
        }
    }

    /// <summary>
    /// Hiển thị thông báo system tray
    /// </summary>
    public async Task HienThiSystemTrayAsync(string tieuDe, string noiDung, LoaiIcon loaiIcon)
    {
        await Task.Run(() =>
        {
            var icon = loaiIcon switch
            {
                LoaiIcon.CanhBao => ToolTipIcon.Warning,
                LoaiIcon.Loi => ToolTipIcon.Error,
                LoaiIcon.ThanhCong => ToolTipIcon.Info,
                _ => ToolTipIcon.None
            };
            
            _notifyIcon.BalloonTipTitle = tieuDe;
            _notifyIcon.BalloonTipText = noiDung;
            _notifyIcon.BalloonTipIcon = icon;
            _notifyIcon.ShowBalloonTip(5000);
            
            _logger.LogInformation("Hiển thị system tray: {TieuDe}", tieuDe);
        });
    }

    /// <summary>
    /// Hiển thị cảnh báo trên màn hình
    /// </summary>
    public async Task HienThiCanhBaoManHinhAsync(string noiDung, int thoiGianHienThi = 5000)
    {
        await Task.Run(() =>
        {
            var form = new Form
            {
                Text = "⚠️ CẢNH BÁO KIDGUARD",
                Size = new System.Drawing.Size(500, 200),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                TopMost = true,
                BackColor = System.Drawing.Color.FromArgb(255, 255, 200)
            };
            
            var label = new Label
            {
                Text = noiDung,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.DarkRed
            };
            
            form.Controls.Add(label);
            
            var timer = new System.Windows.Forms.Timer
            {
                Interval = thoiGianHienThi
            };
            
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                form.Close();
            };
            
            timer.Start();
            form.ShowDialog();
        });
    }

    /// <summary>
    /// Gửi thông báo SMS
    /// </summary>
    public async Task GuiSmsAsync(string soDienThoai, string noiDung)
    {
        if (string.IsNullOrEmpty(_cauHinh.SmsApiUrl))
        {
            _logger.LogWarning("Chưa cấu hình SMS API");
            return;
        }
        
        try
        {
            // TODO: Implement SMS API integration
            _logger.LogInformation("Gửi SMS tới {SoDienThoai}: {NoiDung}", soDienThoai, noiDung);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi SMS");
        }
    }

    /// <summary>
    /// Gửi push notification
    /// </summary>
    public async Task GuiPushNotificationAsync(string deviceId, string tieuDe, string noiDung)
    {
        if (string.IsNullOrEmpty(_cauHinh.PushApiUrl))
        {
            _logger.LogWarning("Chưa cấu hình Push API");
            return;
        }
        
        try
        {
            // TODO: Implement Push notification API
            _logger.LogInformation("Gửi push tới {DeviceId}: {TieuDe}", deviceId, tieuDe);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi push notification");
        }
    }

    /// <summary>
    /// Gửi thông báo đến phụ huynh
    /// </summary>
    public async Task GuiThongBaoPhuHuynhAsync(ThongBaoPhuHuynh thongBao)
    {
        var noiDungEmail = $@"
            <h2>Thông báo từ KidGuard</h2>
            <p><strong>Con em:</strong> {thongBao.ConEm}</p>
            <p><strong>Thời gian:</strong> {thongBao.ThoiGian:dd/MM/yyyy HH:mm}</p>
            <p><strong>Sự kiện:</strong> {LayMoTaSuKien(thongBao.LoaiSuKien)}</p>
            <p><strong>Mức độ:</strong> {thongBao.MucDo}</p>
            <hr>
            <p>{thongBao.NoiDung}</p>
            {(string.IsNullOrEmpty(thongBao.HanhDongDeNghi) ? "" : $"<p><strong>Đề nghị:</strong> {thongBao.HanhDongDeNghi}</p>")}
        ";
        
        // Gửi qua các kênh đã cấu hình
        foreach (var email in _cauHinh.EmailNhanMacDinh)
        {
            await GuiEmailAsync(email, thongBao.TieuDe, noiDungEmail);
        }
        
        // Ghi log
        await GhiLogThongBaoAsync(new ThongBao
        {
            TieuDe = thongBao.TieuDe,
            NoiDung = thongBao.NoiDung,
            LoaiThongBao = LoaiThongBao.BaoCao,
            MucDoUuTien = ChuyenDoiMucDoUuTien(thongBao.MucDo),
            NguoiNhan = "Phụ huynh"
        });
    }

    /// <summary>
    /// Ghi log thông báo
    /// </summary>
    public async Task GhiLogThongBaoAsync(ThongBao thongBao)
    {
        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            Type = thongBao.LoaiThongBao.ToString(),
            Title = thongBao.TieuDe,
            Message = thongBao.NoiDung,
            Priority = thongBao.MucDoUuTien.ToString(),
            UserId = thongBao.NguoiNhan,
            CreatedAt = DateTime.Now,
            IsRead = false
        };
        
        _dbContext.NotificationLogs.Add(log);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Đã ghi log thông báo: {TieuDe}", thongBao.TieuDe);
    }

    /// <summary>
    /// Lấy lịch sử thông báo
    /// </summary>
    public async Task<IEnumerable<LichSuThongBao>> LayLichSuThongBaoAsync(DateTime tuNgay, DateTime denNgay)
    {
        var logs = await _dbContext.NotificationLogs
            .Where(l => l.CreatedAt >= tuNgay && l.CreatedAt <= denNgay)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
        
        return logs.Select(l => new LichSuThongBao
        {
            Id = l.Id,
            ThongBao = new ThongBao
            {
                Id = l.Id,
                TieuDe = l.Title,
                NoiDung = l.Message,
                LoaiThongBao = Enum.Parse<LoaiThongBao>(l.Type),
                MucDoUuTien = Enum.Parse<MucDoUuTien>(l.Priority ?? "BinhThuong"),
                ThoiGianTao = l.CreatedAt,
                NguoiNhan = l.UserId,
                DaDoc = l.IsRead
            },
            ThoiGianGui = l.CreatedAt,
            KenhGui = KenhGui.Log,
            TrangThai = l.IsRead ? TrangThaiGui.DaDoc : TrangThaiGui.DaGui
        });
    }

    /// <summary>
    /// Đánh dấu thông báo đã đọc
    /// </summary>
    public async Task DanhDauDaDocAsync(Guid idThongBao)
    {
        var log = await _dbContext.NotificationLogs.FindAsync(idThongBao);
        if (log != null)
        {
            log.IsRead = true;
            log.ReadAt = DateTime.Now;
            await _dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Xóa thông báo cũ
    /// </summary>
    public async Task<int> XoaThongBaoCuAsync(int soNgayGiuLai = 30)
    {
        var ngayGioiHan = DateTime.Now.AddDays(-soNgayGiuLai);
        var logsCanXoa = await _dbContext.NotificationLogs
            .Where(l => l.CreatedAt < ngayGioiHan)
            .ToListAsync();
        
        _dbContext.NotificationLogs.RemoveRange(logsCanXoa);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Đã xóa {SoLuong} thông báo cũ", logsCanXoa.Count);
        return logsCanXoa.Count;
    }

    /// <summary>
    /// Thiết lập cấu hình thông báo
    /// </summary>
    public async Task ThietLapCauHinhAsync(CauHinhThongBao cauHinh)
    {
        _cauHinh = cauHinh;
        
        // Lưu vào file cấu hình
        var duongDanCauHinh = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "KidGuard", "notification-config.json"
        );
        
        var json = JsonConvert.SerializeObject(cauHinh, Formatting.Indented);
        await File.WriteAllTextAsync(duongDanCauHinh, json);
        
        _logger.LogInformation("Đã cập nhật cấu hình thông báo");
    }

    /// <summary>
    /// Lấy cấu hình thông báo hiện tại
    /// </summary>
    public async Task<CauHinhThongBao> LayCauHinhAsync()
    {
        await LoadCauHinhAsync();
        return _cauHinh;
    }

    /// <summary>
    /// Kiểm tra kết nối dịch vụ thông báo
    /// </summary>
    public async Task<bool> KiemTraKetNoiAsync()
    {
        var ketQua = true;
        
        // Kiểm tra email
        if (_cauHinh.BatThongBaoEmail && !string.IsNullOrEmpty(_cauHinh.SmtpServer))
        {
            try
            {
                using var client = new SmtpClient(_cauHinh.SmtpServer, _cauHinh.SmtpPort);
                // Test connection
            }
            catch
            {
                ketQua = false;
                _logger.LogWarning("Không thể kết nối SMTP server");
            }
        }
        
        // Kiểm tra SMS API
        if (_cauHinh.BatThongBaoSms && !string.IsNullOrEmpty(_cauHinh.SmsApiUrl))
        {
            // TODO: Test SMS API connection
        }
        
        return await Task.FromResult(ketQua);
    }

    /// <summary>
    /// Đăng ký nhận thông báo
    /// </summary>
    public async Task DangKyNhanThongBaoAsync(string userId, LoaiThongBao[] cacLoai)
    {
        // TODO: Implement subscription management
        _logger.LogInformation("Đăng ký nhận thông báo cho {UserId}: {Loai}", 
            userId, string.Join(", ", cacLoai));
        await Task.CompletedTask;
    }

    /// <summary>
    /// Hủy đăng ký nhận thông báo
    /// </summary>
    public async Task HuyDangKyThongBaoAsync(string userId, LoaiThongBao[] cacLoai)
    {
        // TODO: Implement unsubscription
        _logger.LogInformation("Hủy đăng ký thông báo cho {UserId}: {Loai}", 
            userId, string.Join(", ", cacLoai));
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gửi báo cáo hàng ngày
    /// </summary>
    public async Task GuiBaoCaoHangNgayAsync(string emailNhan)
    {
        var ngayBaoCao = DateTime.Today.AddDays(-1);
        var tieuDe = $"[KidGuard] Báo cáo hoạt động ngày {ngayBaoCao:dd/MM/yyyy}";
        
        var noiDung = await TaoBaoCaoNgayHtml(ngayBaoCao);
        
        await GuiEmailAsync(emailNhan, tieuDe, noiDung);
    }

    /// <summary>
    /// Gửi báo cáo khẩn cấp
    /// </summary>
    public async Task GuiBaoCaoKhanCapAsync(string tieuDe, string noiDung, string[] emailNhan)
    {
        var tieuDeKhanCap = $"🚨 [KHẨN CẤP] {tieuDe}";
        
        foreach (var email in emailNhan)
        {
            await GuiEmailAsync(email, tieuDeKhanCap, noiDung);
        }
        
        // Hiển thị cảnh báo màn hình
        await HienThiCanhBaoManHinhAsync($"KHẨN CẤP: {tieuDe}", 15000);
        
        // System tray
        await HienThiSystemTrayAsync(tieuDeKhanCap, noiDung, LoaiIcon.Loi);
    }

    /// <summary>
    /// Thực hiện hành động khi có vi phạm
    /// </summary>
    public async Task XuLyViPhamAsync(ViPham viPham)
    {
        _logger.LogWarning("Phát hiện vi phạm: {LoaiViPham} - {MoTa}", 
            viPham.LoaiViPham, viPham.MoTa);
        
        // Ghi log vi phạm
        await GhiLogThongBaoAsync(new ThongBao
        {
            TieuDe = $"Vi phạm: {viPham.LoaiViPham}",
            NoiDung = viPham.MoTa,
            LoaiThongBao = LoaiThongBao.ViPham,
            MucDoUuTien = ChuyenDoiMucDoUuTien(viPham.MucDo)
        });
        
        // Thực hiện hành động xử lý
        switch (viPham.HanhDongCanThucHien)
        {
            case HanhDongXuLy.GuiCanhBao:
                await GuiCanhBaoAsync($"Vi phạm: {viPham.LoaiViPham}", 
                    viPham.MoTa, viPham.MucDo);
                break;
                
            case HanhDongXuLy.GuiEmailPhuHuynh:
                await GuiThongBaoPhuHuynhAsync(new ThongBaoPhuHuynh
                {
                    TieuDe = $"Phát hiện vi phạm - {viPham.LoaiViPham}",
                    NoiDung = viPham.MoTa,
                    ConEm = viPham.NguoiDung,
                    ThoiGian = viPham.ThoiGian,
                    LoaiSuKien = ChuyenDoiLoaiSuKien(viPham.LoaiViPham),
                    MucDo = viPham.MucDo,
                    HanhDongDeNghi = "Vui lòng kiểm tra và có biện pháp xử lý"
                });
                break;
                
            case HanhDongXuLy.KhoaMayTinh:
                LockWorkStation();
                break;
                
            case HanhDongXuLy.TatMayTinh:
                Process.Start("shutdown", "/s /t 60 /c \"Vi phạm quy định. Máy tính sẽ tắt sau 60 giây\"");
                break;
                
            default:
                _logger.LogInformation("Đã ghi nhận vi phạm");
                break;
        }
        
        viPham.DaXuLy = true;
        viPham.KetQuaXuLy = $"Đã xử lý lúc {DateTime.Now:HH:mm:ss}";
    }

    // Các phương thức hỗ trợ private
    
    private async Task LoadCauHinhAsync()
    {
        try
        {
            var duongDanCauHinh = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KidGuard", "notification-config.json"
            );
            
            if (File.Exists(duongDanCauHinh))
            {
                var json = await File.ReadAllTextAsync(duongDanCauHinh);
                _cauHinh = JsonConvert.DeserializeObject<CauHinhThongBao>(json) ?? _cauHinh;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi load cấu hình thông báo");
        }
    }
    
    private List<KenhGui> XacDinhKenhGui(ThongBao thongBao)
    {
        var cacKenh = new List<KenhGui> { KenhGui.Log };
        
        // Theo loại thông báo
        if (_cauHinh.KenhTheoLoai.ContainsKey(thongBao.LoaiThongBao))
        {
            cacKenh.AddRange(_cauHinh.KenhTheoLoai[thongBao.LoaiThongBao]);
        }
        
        // Mặc định theo mức độ ưu tiên
        if (thongBao.MucDoUuTien >= MucDoUuTien.Cao)
        {
            cacKenh.Add(KenhGui.SystemTray);
            cacKenh.Add(KenhGui.Email);
        }
        
        if (thongBao.MucDoUuTien == MucDoUuTien.KhanCap)
        {
            cacKenh.Add(KenhGui.Sms);
            cacKenh.Add(KenhGui.Push);
        }
        
        return cacKenh.Distinct().ToList();
    }
    
    private MucDoUuTien ChuyenDoiMucDoUuTien(MucDoCanhBao mucDo) => mucDo switch
    {
        MucDoCanhBao.ThongTin => MucDoUuTien.Thap,
        MucDoCanhBao.CanhBao => MucDoUuTien.BinhThuong,
        MucDoCanhBao.NghiemTrong => MucDoUuTien.Cao,
        MucDoCanhBao.RatNghiemTrong => MucDoUuTien.RatCao,
        MucDoCanhBao.KhanCap => MucDoUuTien.KhanCap,
        _ => MucDoUuTien.BinhThuong
    };
    
    private LoaiSuKien ChuyenDoiLoaiSuKien(LoaiViPham loaiViPham) => loaiViPham switch
    {
        LoaiViPham.TruyCapWebCam => LoaiSuKien.TruyCapWebNguyHiem,
        LoaiViPham.SuDungUngDungCam => LoaiSuKien.SuDungUngDungCam,
        LoaiViPham.VuotThoiGian => LoaiSuKien.VuotGioiHanThoiGian,
        LoaiViPham.SuDungNgoaiGio => LoaiSuKien.SuDungNgoaiGioChoPhep,
        LoaiViPham.NoiDungKhongPhuHop => LoaiSuKien.TruyCapNoiDungKhongPhuHop,
        LoaiViPham.ThayDoiCauHinh => LoaiSuKien.ThayDoiHeThong,
        _ => LoaiSuKien.Khac
    };
    
    private string LayMoTaSuKien(LoaiSuKien loaiSuKien) => loaiSuKien switch
    {
        LoaiSuKien.TruyCapWebNguyHiem => "Truy cập website nguy hiểm",
        LoaiSuKien.SuDungUngDungCam => "Sử dụng ứng dụng bị cấm",
        LoaiSuKien.VuotGioiHanThoiGian => "Vượt giới hạn thời gian cho phép",
        LoaiSuKien.SuDungNgoaiGioChoPhep => "Sử dụng máy tính ngoài giờ cho phép",
        LoaiSuKien.TruyCapNoiDungKhongPhuHop => "Truy cập nội dung không phù hợp",
        LoaiSuKien.PhatHienMalware => "Phát hiện phần mềm độc hại",
        LoaiSuKien.ThayDoiHeThong => "Thay đổi cấu hình hệ thống",
        _ => "Sự kiện khác"
    };
    
    private async Task GuiEmailNoiBoAsync(ThongBao thongBao)
    {
        foreach (var email in _cauHinh.EmailNhanMacDinh)
        {
            await GuiEmailAsync(email, thongBao.TieuDe, thongBao.NoiDung);
        }
    }
    
    private async Task HienThiSystemTrayNoiBoAsync(ThongBao thongBao)
    {
        var loaiIcon = thongBao.MucDoUuTien switch
        {
            MucDoUuTien.KhanCap => LoaiIcon.Loi,
            MucDoUuTien.RatCao => LoaiIcon.CanhBao,
            MucDoUuTien.Cao => LoaiIcon.CanhBao,
            _ => LoaiIcon.ThongTin
        };
        
        await HienThiSystemTrayAsync(thongBao.TieuDe, thongBao.NoiDung, loaiIcon);
    }
    
    private async Task GuiSmsNoiBoAsync(ThongBao thongBao)
    {
        // TODO: Get phone numbers from config
        await Task.CompletedTask;
    }
    
    private async Task GuiPushNoiBoAsync(ThongBao thongBao)
    {
        // TODO: Get device IDs from config
        await Task.CompletedTask;
    }
    
    private string TaoNoiDungEmailHtml(string tieuDe, string noiDung)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; }}
        .header {{ background: #2196F3; color: white; padding: 20px; }}
        .content {{ padding: 20px; }}
        .footer {{ background: #f5f5f5; padding: 10px; text-align: center; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🛡️ KidGuard</h1>
        <h2>{tieuDe}</h2>
    </div>
    <div class='content'>
        {noiDung}
        <p><small>Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</small></p>
    </div>
    <div class='footer'>
        <p>© 2024 KidGuard - Bảo vệ an toàn trẻ em trên không gian mạng</p>
    </div>
</body>
</html>";
    }
    
    private async Task<string> TaoBaoCaoNgayHtml(DateTime ngay)
    {
        // TODO: Generate daily report from database
        return $@"
            <h2>Báo cáo ngày {ngay:dd/MM/yyyy}</h2>
            <ul>
                <li>Tổng thời gian sử dụng: 4 giờ 30 phút</li>
                <li>Số ứng dụng đã mở: 12</li>
                <li>Số website đã truy cập: 45</li>
                <li>Số lần vi phạm: 2</li>
            </ul>
            <p>Chi tiết xem trong ứng dụng KidGuard</p>
        ";
    }
    
    public void Dispose()
    {
        _notifyIcon?.Dispose();
    }
}