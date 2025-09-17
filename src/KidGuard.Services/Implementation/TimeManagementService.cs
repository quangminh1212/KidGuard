using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using KidGuard.Core.Data;
using KidGuard.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KidGuard.Services.Implementation;

/// <summary>
/// Dịch vụ quản lý thời gian sử dụng máy tính
/// </summary>
public class TimeManagementService : ITimeManagementService
{
    private readonly ILogger<TimeManagementService> _logger;
    private readonly KidGuardDbContext _dbContext;
    private readonly Dictionary<string, PhienSuDung> _phienHienTai = new();
    private readonly SemaphoreSlim _khoaDongBo = new(1, 1);
    private Timer? _timerKiemTra;

    // Import Windows API để khóa máy tính
    [DllImport("user32.dll")]
    private static extern bool LockWorkStation();

    public TimeManagementService(
        ILogger<TimeManagementService> logger,
        KidGuardDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        
        // Khởi tạo timer kiểm tra định kỳ mỗi phút
        _timerKiemTra = new Timer(
            KiemTraDinhKy, 
            null, 
            TimeSpan.FromMinutes(1), 
            TimeSpan.FromMinutes(1)
        );
    }

    /// <summary>
    /// Bắt đầu theo dõi phiên sử dụng mới
    /// </summary>
    public async Task<Guid> BatDauPhienSuDungAsync(string nguoiDung)
    {
        await _khoaDongBo.WaitAsync();
        try
        {
            // Kiểm tra có được phép sử dụng không
            if (!await KiemTraChoPhepSuDungAsync(nguoiDung))
            {
                _logger.LogWarning("Người dùng {NguoiDung} không được phép sử dụng vào thời điểm này", nguoiDung);
                await TamKhoaMayTinhAsync("Ngoài giờ cho phép sử dụng");
                throw new UnauthorizedAccessException("Không trong giờ cho phép sử dụng");
            }

            // Kết thúc phiên cũ nếu có
            if (_phienHienTai.ContainsKey(nguoiDung))
            {
                await KetThucPhienSuDungAsync(_phienHienTai[nguoiDung].Id);
            }

            var phienMoi = new PhienSuDung
            {
                Id = Guid.NewGuid(),
                NguoiDung = nguoiDung,
                ThoiGianBatDau = DateTime.Now
            };

            // Lưu vào database
            var usageSession = new UsageSession
            {
                Id = phienMoi.Id,
                UserId = nguoiDung,
                StartTime = phienMoi.ThoiGianBatDau,
                IsActive = true
            };

            _dbContext.UsageSessions.Add(usageSession);
            await _dbContext.SaveChangesAsync();

            // Lưu vào bộ nhớ
            _phienHienTai[nguoiDung] = phienMoi;

            _logger.LogInformation("Bắt đầu phiên sử dụng mới cho {NguoiDung}: {IdPhien}", nguoiDung, phienMoi.Id);
            return phienMoi.Id;
        }
        finally
        {
            _khoaDongBo.Release();
        }
    }

    /// <summary>
    /// Kết thúc phiên sử dụng hiện tại
    /// </summary>
    public async Task KetThucPhienSuDungAsync(Guid idPhien)
    {
        await _khoaDongBo.WaitAsync();
        try
        {
            var session = await _dbContext.UsageSessions
                .FirstOrDefaultAsync(s => s.Id == idPhien);

            if (session != null && session.IsActive)
            {
                session.EndTime = DateTime.Now;
                session.IsActive = false;
                session.Duration = (int)(session.EndTime.Value - session.StartTime).TotalMinutes;
                
                await _dbContext.SaveChangesAsync();

                // Xóa khỏi bộ nhớ
                var nguoiDung = _phienHienTai.FirstOrDefault(p => p.Value.Id == idPhien).Key;
                if (nguoiDung != null)
                {
                    _phienHienTai.Remove(nguoiDung);
                }

                _logger.LogInformation("Kết thúc phiên sử dụng {IdPhien}", idPhien);
            }
        }
        finally
        {
            _khoaDongBo.Release();
        }
    }

    /// <summary>
    /// Lấy thời gian sử dụng trong ngày
    /// </summary>
    public async Task<TimeSpan> LayThoiGianSuDungTrongNgayAsync(string nguoiDung, DateTime? ngay = null)
    {
        var ngayKiemTra = (ngay ?? DateTime.Now).Date;
        
        var sessions = await _dbContext.UsageSessions
            .Where(s => s.UserId == nguoiDung &&
                       s.StartTime.Date == ngayKiemTra)
            .ToListAsync();

        var tongPhut = sessions.Sum(s => 
        {
            var endTime = s.EndTime ?? DateTime.Now;
            return (int)(endTime - s.StartTime).TotalMinutes;
        });

        return TimeSpan.FromMinutes(tongPhut);
    }

    /// <summary>
    /// Lấy thời gian sử dụng trong tuần
    /// </summary>
    public async Task<TimeSpan> LayThoiGianSuDungTrongTuanAsync(string nguoiDung)
    {
        var homNay = DateTime.Now.Date;
        var dauTuan = homNay.AddDays(-(int)homNay.DayOfWeek);
        
        var sessions = await _dbContext.UsageSessions
            .Where(s => s.UserId == nguoiDung &&
                       s.StartTime >= dauTuan)
            .ToListAsync();

        var tongPhut = sessions.Sum(s => 
        {
            var endTime = s.EndTime ?? DateTime.Now;
            return (int)(endTime - s.StartTime).TotalMinutes;
        });

        return TimeSpan.FromMinutes(tongPhut);
    }

    /// <summary>
    /// Lấy lịch sử phiên sử dụng
    /// </summary>
    public async Task<IEnumerable<PhienSuDung>> LayLichSuPhienAsync(string nguoiDung, DateTime tuNgay, DateTime denNgay)
    {
        var sessions = await _dbContext.UsageSessions
            .Where(s => s.UserId == nguoiDung &&
                       s.StartTime >= tuNgay &&
                       s.StartTime <= denNgay)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        return sessions.Select(s => new PhienSuDung
        {
            Id = s.Id,
            NguoiDung = s.UserId,
            ThoiGianBatDau = s.StartTime,
            ThoiGianKetThuc = s.EndTime,
            GhiChu = s.Notes
        });
    }

    /// <summary>
    /// Thiết lập giới hạn thời gian hàng ngày
    /// </summary>
    public async Task ThietLapGioiHanNgayAsync(string nguoiDung, TimeSpan gioiHan)
    {
        var settings = await _dbContext.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == nguoiDung);

        if (settings == null)
        {
            settings = new UserSettings
            {
                Id = Guid.NewGuid(),
                UserId = nguoiDung,
                DailyTimeLimit = (int)gioiHan.TotalMinutes,
                CreatedAt = DateTime.Now
            };
            _dbContext.UserSettings.Add(settings);
        }
        else
        {
            settings.DailyTimeLimit = (int)gioiHan.TotalMinutes;
            settings.UpdatedAt = DateTime.Now;
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Đã thiết lập giới hạn {GioiHan} phút/ngày cho {NguoiDung}", 
            gioiHan.TotalMinutes, nguoiDung);
    }

    /// <summary>
    /// Lấy giới hạn thời gian hàng ngày
    /// </summary>
    public async Task<TimeSpan?> LayGioiHanNgayAsync(string nguoiDung)
    {
        var settings = await _dbContext.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == nguoiDung);

        if (settings?.DailyTimeLimit != null)
        {
            return TimeSpan.FromMinutes(settings.DailyTimeLimit.Value);
        }

        return null;
    }

    /// <summary>
    /// Kiểm tra có vượt quá giới hạn không
    /// </summary>
    public async Task<bool> KiemTraVuotGioiHanAsync(string nguoiDung)
    {
        var gioiHan = await LayGioiHanNgayAsync(nguoiDung);
        if (!gioiHan.HasValue) return false;

        var daSuDung = await LayThoiGianSuDungTrongNgayAsync(nguoiDung);
        return daSuDung > gioiHan.Value;
    }

    /// <summary>
    /// Lấy thời gian còn lại trong ngày
    /// </summary>
    public async Task<TimeSpan> LayThoiGianConLaiAsync(string nguoiDung)
    {
        var gioiHan = await LayGioiHanNgayAsync(nguoiDung);
        if (!gioiHan.HasValue) return TimeSpan.MaxValue;

        var daSuDung = await LayThoiGianSuDungTrongNgayAsync(nguoiDung);
        var conLai = gioiHan.Value - daSuDung;
        
        return conLai > TimeSpan.Zero ? conLai : TimeSpan.Zero;
    }

    /// <summary>
    /// Thiết lập lịch sử dụng được phép
    /// </summary>
    public async Task ThietLapLichSuDungAsync(string nguoiDung, LichSuDung lichSuDung)
    {
        await _khoaDongBo.WaitAsync();
        try
        {
            // Xóa lịch cũ nếu có
            var schedules = await _dbContext.ScheduleRules
                .Where(s => s.UserId == nguoiDung)
                .ToListAsync();
            
            _dbContext.ScheduleRules.RemoveRange(schedules);

            // Thêm lịch mới
            foreach (var khungGio in lichSuDung.CacKhungGio)
            {
                var schedule = new ScheduleRule
                {
                    Id = Guid.NewGuid(),
                    UserId = nguoiDung,
                    DayOfWeek = (int)khungGio.NgayTrongTuan,
                    StartTime = khungGio.GioBatDau,
                    EndTime = khungGio.GioKetThuc,
                    IsAllowed = khungGio.ChoPhepSuDung,
                    MaxDuration = khungGio.GioiHanThoiGian?.TotalMinutes,
                    CreatedAt = DateTime.Now
                };
                _dbContext.ScheduleRules.Add(schedule);
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Đã thiết lập lịch sử dụng cho {NguoiDung}", nguoiDung);
        }
        finally
        {
            _khoaDongBo.Release();
        }
    }

    /// <summary>
    /// Lấy lịch sử dụng hiện tại
    /// </summary>
    public async Task<LichSuDung?> LayLichSuDungAsync(string nguoiDung)
    {
        var schedules = await _dbContext.ScheduleRules
            .Where(s => s.UserId == nguoiDung)
            .ToListAsync();

        if (!schedules.Any()) return null;

        return new LichSuDung
        {
            Id = Guid.NewGuid(),
            NguoiDung = nguoiDung,
            CacKhungGio = schedules.Select(s => new KhungGioChoPhep
            {
                NgayTrongTuan = (DayOfWeek)s.DayOfWeek,
                GioBatDau = s.StartTime,
                GioKetThuc = s.EndTime,
                ChoPhepSuDung = s.IsAllowed,
                GioiHanThoiGian = s.MaxDuration.HasValue 
                    ? TimeSpan.FromMinutes(s.MaxDuration.Value) 
                    : null
            }).ToList(),
            KichHoat = true,
            NgayTao = schedules.Min(s => s.CreatedAt)
        };
    }

    /// <summary>
    /// Kiểm tra có được phép sử dụng vào thời điểm hiện tại
    /// </summary>
    public async Task<bool> KiemTraChoPhepSuDungAsync(string nguoiDung, DateTime? thoiDiem = null)
    {
        var kiemTraLuc = thoiDiem ?? DateTime.Now;
        var ngayTrongTuan = (int)kiemTraLuc.DayOfWeek;
        var gioHienTai = kiemTraLuc.TimeOfDay;

        var schedule = await _dbContext.ScheduleRules
            .FirstOrDefaultAsync(s => s.UserId == nguoiDung &&
                                     s.DayOfWeek == ngayTrongTuan &&
                                     s.StartTime <= gioHienTai &&
                                     s.EndTime >= gioHienTai);

        return schedule?.IsAllowed ?? true; // Mặc định cho phép nếu không có lịch
    }

    /// <summary>
    /// Tạm khóa máy tính khi hết thời gian
    /// </summary>
    public async Task TamKhoaMayTinhAsync(string lyDo)
    {
        _logger.LogWarning("Khóa máy tính: {LyDo}", lyDo);
        
        // Ghi log
        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            Type = "LockScreen",
            Title = "Máy tính bị khóa",
            Message = lyDo,
            CreatedAt = DateTime.Now,
            IsRead = false
        };
        
        _dbContext.NotificationLogs.Add(log);
        await _dbContext.SaveChangesAsync();
        
        // Khóa máy tính
        LockWorkStation();
    }

    /// <summary>
    /// Gửi cảnh báo sắp hết thời gian
    /// </summary>
    public async Task GuiCanhBaoSapHetThoiGianAsync(string nguoiDung, TimeSpan thoiGianConLai)
    {
        var thongBao = $"Còn {thoiGianConLai.TotalMinutes:F0} phút sử dụng trong ngày hôm nay";
        
        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            Type = "TimeWarning",
            Title = "Cảnh báo thời gian",
            Message = thongBao,
            UserId = nguoiDung,
            Priority = "High",
            CreatedAt = DateTime.Now
        };
        
        _dbContext.NotificationLogs.Add(log);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Đã gửi cảnh báo thời gian cho {NguoiDung}: {ThongBao}", nguoiDung, thongBao);
    }

    /// <summary>
    /// Lấy thống kê sử dụng theo ngày
    /// </summary>
    public async Task<ThongKeSuDungTheoNgay> LayThongKeTheoNgayAsync(string nguoiDung, DateTime ngay)
    {
        var ngayKiemTra = ngay.Date;
        var sessions = await _dbContext.UsageSessions
            .Where(s => s.UserId == nguoiDung &&
                       s.StartTime.Date == ngayKiemTra)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        var thongKe = new ThongKeSuDungTheoNgay
        {
            Ngay = ngayKiemTra,
            NguoiDung = nguoiDung,
            SoPhienSuDung = sessions.Count,
            GioiHanNgay = await LayGioiHanNgayAsync(nguoiDung)
        };

        if (sessions.Any())
        {
            thongKe.ThoiGianBatDauSom = sessions.First().StartTime.TimeOfDay;
            thongKe.ThoiGianKetThucMuon = (sessions.Last().EndTime ?? DateTime.Now).TimeOfDay;
            
            // Tính tổng thời gian và phân bố theo giờ
            foreach (var session in sessions)
            {
                var endTime = session.EndTime ?? DateTime.Now;
                var duration = endTime - session.StartTime;
                thongKe.TongThoiGianSuDung += duration;
                
                // Phân bố theo giờ
                var startHour = session.StartTime.Hour;
                var endHour = endTime.Hour;
                
                for (int hour = startHour; hour <= endHour; hour++)
                {
                    var minutesInHour = 60.0;
                    
                    if (hour == startHour)
                        minutesInHour -= session.StartTime.Minute;
                    if (hour == endHour)
                        minutesInHour = endTime.Minute;
                    
                    if (!thongKe.TheoGio.ContainsKey(hour))
                        thongKe.TheoGio[hour] = TimeSpan.Zero;
                    
                    thongKe.TheoGio[hour] += TimeSpan.FromMinutes(minutesInHour);
                }
            }
            
            thongKe.ChiTietPhien = sessions.Select(s => new PhienSuDung
            {
                Id = s.Id,
                NguoiDung = s.UserId,
                ThoiGianBatDau = s.StartTime,
                ThoiGianKetThuc = s.EndTime
            }).ToList();
        }

        return thongKe;
    }

    /// <summary>
    /// Lấy thống kê sử dụng theo tuần
    /// </summary>
    public async Task<ThongKeSuDungTheoTuan> LayThongKeTheoTuanAsync(string nguoiDung, DateTime tuanBatDau)
    {
        var batDau = tuanBatDau.Date;
        var ketThuc = batDau.AddDays(7);
        
        var thongKe = new ThongKeSuDungTheoTuan
        {
            TuanBatDau = batDau,
            TuanKetThuc = ketThuc.AddDays(-1),
            NguoiDung = nguoiDung
        };

        for (int i = 0; i < 7; i++)
        {
            var ngay = batDau.AddDays(i);
            var thongKeNgay = await LayThongKeTheoNgayAsync(nguoiDung, ngay);
            
            thongKe.TheoNgay[ngay.DayOfWeek] = thongKeNgay.TongThoiGianSuDung;
            thongKe.TongThoiGianSuDung += thongKeNgay.TongThoiGianSuDung;
            
            if (thongKeNgay.VuotGioiHan)
                thongKe.SoNgayVuotGioiHan++;
            
            if (!thongKe.NgaySuDungNhieuNhat.HasValue || 
                thongKeNgay.TongThoiGianSuDung > thongKe.TheoNgay[thongKe.NgaySuDungNhieuNhat.Value.DayOfWeek])
            {
                thongKe.NgaySuDungNhieuNhat = ngay;
            }
            
            if (!thongKe.NgaySuDungItNhat.HasValue || 
                thongKeNgay.TongThoiGianSuDung < thongKe.TheoNgay[thongKe.NgaySuDungItNhat.Value.DayOfWeek])
            {
                thongKe.NgaySuDungItNhat = ngay;
            }
        }

        return thongKe;
    }

    /// <summary>
    /// Lấy thống kê sử dụng theo tháng
    /// </summary>
    public async Task<ThongKeSuDungTheoThang> LayThongKeTheoThangAsync(string nguoiDung, int thang, int nam)
    {
        var batDau = new DateTime(nam, thang, 1);
        var soNgay = DateTime.DaysInMonth(nam, thang);
        
        var thongKe = new ThongKeSuDungTheoThang
        {
            Thang = thang,
            Nam = nam,
            NguoiDung = nguoiDung,
            SoNgayTrongThang = soNgay
        };

        for (int ngay = 1; ngay <= soNgay; ngay++)
        {
            var ngayKiemTra = new DateTime(nam, thang, ngay);
            var thongKeNgay = await LayThongKeTheoNgayAsync(nguoiDung, ngayKiemTra);
            
            thongKe.TheoNgay[ngay] = thongKeNgay.TongThoiGianSuDung;
            thongKe.TongThoiGianSuDung += thongKeNgay.TongThoiGianSuDung;
            
            if (thongKeNgay.VuotGioiHan)
                thongKe.SoNgayVuotGioiHan++;
        }

        // Tính xu hướng so với tháng trước
        if (thang > 1)
        {
            var thongKeThangTruoc = await LayThongKeTheoThangAsync(nguoiDung, thang - 1, nam);
            thongKe.XuHuong = thongKe.TongThoiGianSuDung - thongKeThangTruoc.TongThoiGianSuDung;
        }

        return thongKe;
    }

    /// <summary>
    /// Thêm thời gian bonus
    /// </summary>
    public async Task ThemThoiGianBonusAsync(string nguoiDung, TimeSpan thoiGianBonus, string lyDo)
    {
        var settings = await _dbContext.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == nguoiDung);

        if (settings != null && settings.DailyTimeLimit.HasValue)
        {
            settings.DailyTimeLimit += (int)thoiGianBonus.TotalMinutes;
            settings.UpdatedAt = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Đã thêm {Phut} phút bonus cho {NguoiDung}: {LyDo}", 
                thoiGianBonus.TotalMinutes, nguoiDung, lyDo);
        }
    }

    /// <summary>
    /// Reset thời gian sử dụng
    /// </summary>
    public async Task ResetThoiGianSuDungAsync(string nguoiDung)
    {
        // Kết thúc tất cả phiên đang hoạt động
        var activeSessions = await _dbContext.UsageSessions
            .Where(s => s.UserId == nguoiDung && s.IsActive)
            .ToListAsync();

        foreach (var session in activeSessions)
        {
            session.EndTime = DateTime.Now;
            session.IsActive = false;
        }

        await _dbContext.SaveChangesAsync();
        
        if (_phienHienTai.ContainsKey(nguoiDung))
            _phienHienTai.Remove(nguoiDung);
        
        _logger.LogInformation("Đã reset thời gian sử dụng cho {NguoiDung}", nguoiDung);
    }

    /// <summary>
    /// Xuất báo cáo thời gian sử dụng
    /// </summary>
    public async Task<string> XuatBaoCaoAsync(string nguoiDung, DateTime tuNgay, DateTime denNgay, string dinhDang = "PDF")
    {
        // TODO: Implement xuất báo cáo PDF/Excel
        var baoCao = $"BaoCao_{nguoiDung}_{tuNgay:yyyyMMdd}_{denNgay:yyyyMMdd}.{dinhDang.ToLower()}";
        _logger.LogInformation("Xuất báo cáo {BaoCao}", baoCao);
        return baoCao;
    }

    /// <summary>
    /// Lấy danh sách cảnh báo vi phạm
    /// </summary>
    public async Task<IEnumerable<CanhBaoViPham>> LayDanhSachCanhBaoAsync(string nguoiDung, int soNgayGanDay = 7)
    {
        var tuNgay = DateTime.Now.AddDays(-soNgayGanDay);
        var canhBao = new List<CanhBaoViPham>();

        // Kiểm tra vi phạm giới hạn ngày
        for (int i = 0; i < soNgayGanDay; i++)
        {
            var ngay = DateTime.Now.AddDays(-i).Date;
            var thongKe = await LayThongKeTheoNgayAsync(nguoiDung, ngay);
            
            if (thongKe.VuotGioiHan)
            {
                canhBao.Add(new CanhBaoViPham
                {
                    Id = Guid.NewGuid(),
                    NguoiDung = nguoiDung,
                    ThoiDiemViPham = ngay,
                    LoaiViPham = LoaiViPham.VuotGioiHanNgay,
                    MoTa = $"Vượt giới hạn {thongKe.TongThoiGianSuDung - thongKe.GioiHanNgay.Value} phút",
                    ThoiGianVuot = thongKe.TongThoiGianSuDung - thongKe.GioiHanNgay.Value
                });
            }
        }

        return canhBao;
    }

    /// <summary>
    /// Thiết lập chế độ nghỉ ngơi bắt buộc
    /// </summary>
    public async Task ThietLapCheDoNghiNgoiAsync(string nguoiDung, TimeSpan thoiGianSuDung, TimeSpan thoiGianNghi)
    {
        var settings = await _dbContext.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == nguoiDung);

        if (settings == null)
        {
            settings = new UserSettings
            {
                Id = Guid.NewGuid(),
                UserId = nguoiDung,
                BreakTimeAfterMinutes = (int)thoiGianSuDung.TotalMinutes,
                BreakDurationMinutes = (int)thoiGianNghi.TotalMinutes,
                CreatedAt = DateTime.Now
            };
            _dbContext.UserSettings.Add(settings);
        }
        else
        {
            settings.BreakTimeAfterMinutes = (int)thoiGianSuDung.TotalMinutes;
            settings.BreakDurationMinutes = (int)thoiGianNghi.TotalMinutes;
            settings.UpdatedAt = DateTime.Now;
        }

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Kiểm tra phải nghỉ ngơi không
    /// </summary>
    public async Task<bool> KiemTraPhaiNghiNgoiAsync(string nguoiDung)
    {
        var settings = await _dbContext.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == nguoiDung);

        if (settings?.BreakTimeAfterMinutes == null) return false;

        // Kiểm tra phiên hiện tại
        if (_phienHienTai.ContainsKey(nguoiDung))
        {
            var phien = _phienHienTai[nguoiDung];
            var daSuDung = DateTime.Now - phien.ThoiGianBatDau;
            
            return daSuDung.TotalMinutes >= settings.BreakTimeAfterMinutes;
        }

        return false;
    }

    /// <summary>
    /// Kiểm tra định kỳ các điều kiện
    /// </summary>
    private async void KiemTraDinhKy(object? state)
    {
        try
        {
            foreach (var kvp in _phienHienTai)
            {
                var nguoiDung = kvp.Key;
                
                // Kiểm tra vượt giới hạn
                if (await KiemTraVuotGioiHanAsync(nguoiDung))
                {
                    await TamKhoaMayTinhAsync($"Đã hết thời gian sử dụng trong ngày");
                    await KetThucPhienSuDungAsync(kvp.Value.Id);
                    continue;
                }

                // Kiểm tra cần nghỉ ngơi
                if (await KiemTraPhaiNghiNgoiAsync(nguoiDung))
                {
                    await GuiCanhBaoSapHetThoiGianAsync(nguoiDung, TimeSpan.FromMinutes(5));
                }

                // Cảnh báo sắp hết thời gian
                var conLai = await LayThoiGianConLaiAsync(nguoiDung);
                if (conLai.TotalMinutes <= 10 && conLai.TotalMinutes > 0)
                {
                    await GuiCanhBaoSapHetThoiGianAsync(nguoiDung, conLai);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi kiểm tra định kỳ");
        }
    }

    public void Dispose()
    {
        _timerKiemTra?.Dispose();
        _khoaDongBo?.Dispose();
    }
}