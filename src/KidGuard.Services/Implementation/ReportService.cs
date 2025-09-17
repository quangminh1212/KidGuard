using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KidGuard.Core.Data;
using KidGuard.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KidGuard.Services.Implementation;

/// <summary>
/// Dịch vụ tạo báo cáo và thống kê
/// </summary>
public class ReportService : IReportService
{
    private readonly ILogger<ReportService> _logger;
    private readonly KidGuardDbContext _dbContext;
    private readonly ITimeManagementService _timeManagementService;
    private readonly IApplicationMonitoringService _applicationMonitoringService;
    private readonly IActivityLoggerService _activityLoggerService;
    private readonly string _thuMucBaoCao;

    public ReportService(
        ILogger<ReportService> logger,
        KidGuardDbContext dbContext,
        ITimeManagementService timeManagementService,
        IApplicationMonitoringService applicationMonitoringService,
        IActivityLoggerService activityLoggerService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _timeManagementService = timeManagementService;
        _applicationMonitoringService = applicationMonitoringService;
        _activityLoggerService = activityLoggerService;
        
        // Tạo thư mục lưu báo cáo
        _thuMucBaoCao = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "KidGuard", "Reports"
        );
        
        if (!Directory.Exists(_thuMucBaoCao))
        {
            Directory.CreateDirectory(_thuMucBaoCao);
        }
    }

    /// <summary>
    /// Tạo báo cáo tổng quan hàng ngày
    /// </summary>
    public async Task<BaoCaoNgay> TaoBaoCaoNgayAsync(DateTime ngay)
    {
        var ngayBaoCao = ngay.Date;
        var nguoiDung = Environment.UserName;
        
        // Lấy thông tin thời gian sử dụng
        var thongKeNgay = await _timeManagementService.LayThongKeTheoNgayAsync(nguoiDung, ngayBaoCao);
        
        // Lấy danh sách ứng dụng đã sử dụng
        var ungDung = await _dbContext.MonitoredApplications
            .Where(a => a.LastActive.Date == ngayBaoCao)
            .ToListAsync();
        
        // Lấy danh sách website đã truy cập (từ ActivityLog)
        var websites = await _dbContext.ActivityLogEntries
            .Where(a => a.Timestamp.Date == ngayBaoCao && a.ActivityType == "Website")
            .Select(a => a.Details)
            .Distinct()
            .ToListAsync();
        
        // Lấy số lần vi phạm
        var viPham = await _dbContext.NotificationLogs
            .Where(n => n.CreatedAt.Date == ngayBaoCao && 
                       (n.Type == "Warning" || n.Type == "Violation"))
            .CountAsync();
        
        // Tính hoạt động theo giờ
        var hoatDongTheoGio = new List<HoatDongTheoGio>();
        for (int gio = 0; gio < 24; gio++)
        {
            var thoiGian = thongKeNgay.TheoGio.ContainsKey(gio) 
                ? thongKeNgay.TheoGio[gio] 
                : TimeSpan.Zero;
                
            hoatDongTheoGio.Add(new HoatDongTheoGio
            {
                Gio = gio,
                ThoiGianSuDung = thoiGian,
                SoUngDungDaMo = ungDung.Count(a => a.LastActive.Hour == gio)
            });
        }
        
        // Top ứng dụng sử dụng nhiều nhất
        var topUngDung = ungDung
            .GroupBy(a => a.ApplicationName)
            .Select(g => new UngDungSuDungNhieuNhat
            {
                TenUngDung = g.Key,
                ThoiGianSuDung = TimeSpan.FromMinutes(g.Sum(a => a.TotalUsageMinutes)),
                PhanTram = g.Sum(a => a.TotalUsageMinutes) * 100.0 / 
                          Math.Max(1, ungDung.Sum(a => a.TotalUsageMinutes))
            })
            .OrderByDescending(a => a.ThoiGianSuDung)
            .Take(5)
            .ToList();
        
        return new BaoCaoNgay
        {
            Ngay = ngayBaoCao,
            TongThoiGianSuDung = thongKeNgay.TongThoiGianSuDung,
            SoUngDungDaSuDung = ungDung.Count,
            SoWebsiteDaTruyCap = websites.Count,
            SoLanViPham = viPham,
            HoatDongTheoGio = hoatDongTheoGio,
            TopUngDung = topUngDung,
            TopWebsite = new List<WebsiteTruyCapNhieuNhat>(), // TODO: Implement website tracking
            GhiChu = thongKeNgay.VuotGioiHan ? "Đã vượt giới hạn thời gian sử dụng" : null
        };
    }

    /// <summary>
    /// Tạo báo cáo tổng quan hàng tuần
    /// </summary>
    public async Task<BaoCaoTuan> TaoBaoCaoTuanAsync(DateTime tuanBatDau)
    {
        var batDau = tuanBatDau.Date;
        var ketThuc = batDau.AddDays(6);
        var nguoiDung = Environment.UserName;
        
        var thongKeTuan = await _timeManagementService.LayThongKeTheoTuanAsync(nguoiDung, batDau);
        
        // Đếm vi phạm trong tuần
        var viPham = await _dbContext.NotificationLogs
            .Where(n => n.CreatedAt >= batDau && n.CreatedAt <= ketThuc &&
                       (n.Type == "Warning" || n.Type == "Violation"))
            .CountAsync();
        
        // Tìm ngày hoạt động cao nhất
        var ngayHoatDongCao = new List<NgayHoatDongNhieuNhat>();
        if (thongKeTuan.NgaySuDungNhieuNhat.HasValue)
        {
            var thongKeNgay = await _timeManagementService.LayThongKeTheoNgayAsync(
                nguoiDung, thongKeTuan.NgaySuDungNhieuNhat.Value);
            
            ngayHoatDongCao.Add(new NgayHoatDongNhieuNhat
            {
                Ngay = thongKeTuan.NgaySuDungNhieuNhat.Value,
                ThoiGianSuDung = thongKeNgay.TongThoiGianSuDung,
                LyDo = "Ngày sử dụng nhiều nhất trong tuần"
            });
        }
        
        // Phân tích xu hướng
        var xuHuong = new TrendAnalysis();
        if (thongKeTuan.TongThoiGianSuDung.TotalMinutes > 0)
        {
            // So sánh với tuần trước
            var tuanTruoc = await _timeManagementService.LayThongKeTheoTuanAsync(
                nguoiDung, batDau.AddDays(-7));
            
            var chenhLech = thongKeTuan.TongThoiGianSuDung - tuanTruoc.TongThoiGianSuDung;
            var phanTram = tuanTruoc.TongThoiGianSuDung.TotalMinutes > 0 
                ? chenhLech.TotalMinutes * 100 / tuanTruoc.TongThoiGianSuDung.TotalMinutes
                : 0;
            
            xuHuong.XuHuong = chenhLech.TotalMinutes > 0 ? "Tăng" : 
                             chenhLech.TotalMinutes < 0 ? "Giảm" : "Ổn định";
            xuHuong.PhanTramThayDoi = Math.Abs(phanTram);
            xuHuong.DuBao = phanTram > 20 ? "Cần chú ý kiểm soát thời gian sử dụng" :
                           phanTram < -20 ? "Đang cải thiện tốt" : "Trong mức bình thường";
        }
        
        return new BaoCaoTuan
        {
            TuanBatDau = batDau,
            TuanKetThuc = ketThuc,
            TongThoiGianSuDung = thongKeTuan.TongThoiGianSuDung,
            TrungBinhMoiNgay = thongKeTuan.TrungBinhMoiNgay,
            ThoiGianTheoNgay = thongKeTuan.TheoNgay,
            TongSoViPham = viPham,
            NgayHoatDongCao = ngayHoatDongCao,
            XuHuongSuDung = xuHuong
        };
    }

    /// <summary>
    /// Tạo báo cáo tổng quan hàng tháng
    /// </summary>
    public async Task<BaoCaoThang> TaoBaoCaoThangAsync(int thang, int nam)
    {
        var nguoiDung = Environment.UserName;
        var thongKeThang = await _timeManagementService.LayThongKeTheoThangAsync(nguoiDung, thang, nam);
        
        var batDau = new DateTime(nam, thang, 1);
        var ketThuc = batDau.AddMonths(1).AddDays(-1);
        
        // Lấy top ứng dụng trong tháng
        var ungDung = await _dbContext.MonitoredApplications
            .Where(a => a.LastActive >= batDau && a.LastActive <= ketThuc)
            .GroupBy(a => a.ApplicationName)
            .Select(g => new UngDungSuDungNhieuNhat
            {
                TenUngDung = g.Key,
                ThoiGianSuDung = TimeSpan.FromMinutes(g.Sum(a => a.TotalUsageMinutes))
            })
            .OrderByDescending(a => a.ThoiGianSuDung)
            .Take(10)
            .ToListAsync();
        
        // Đếm vi phạm
        var viPham = await _dbContext.NotificationLogs
            .Where(n => n.CreatedAt >= batDau && n.CreatedAt <= ketThuc &&
                       (n.Type == "Warning" || n.Type == "Violation"))
            .CountAsync();
        
        // Tính phần trăm tăng giảm
        var phanTramTangGiam = 0.0;
        if (thongKeThang.XuHuong.TotalMinutes != 0)
        {
            phanTramTangGiam = thongKeThang.XuHuong.TotalMinutes * 100 / 
                              Math.Max(1, thongKeThang.TongThoiGianSuDung.TotalMinutes);
        }
        
        return new BaoCaoThang
        {
            Thang = thang,
            Nam = nam,
            TongThoiGianSuDung = thongKeThang.TongThoiGianSuDung,
            ThoiGianTheoNgay = thongKeThang.TheoNgay,
            Top10UngDung = ungDung,
            Top10Website = new List<WebsiteTruyCapNhieuNhat>(), // TODO
            TongSoViPham = viPham,
            PhanTramTangGiam = phanTramTangGiam
        };
    }

    /// <summary>
    /// Tạo báo cáo hoạt động ứng dụng
    /// </summary>
    public async Task<BaoCaoUngDung> TaoBaoCaoUngDungAsync(DateTime tuNgay, DateTime denNgay)
    {
        var ungDung = await _dbContext.MonitoredApplications
            .Where(a => a.LastActive >= tuNgay && a.LastActive <= denNgay)
            .ToListAsync();
        
        var chiTiet = ungDung
            .GroupBy(a => a.ApplicationName)
            .Select(g => new ChiTietUngDung
            {
                TenUngDung = g.Key,
                LoaiUngDung = g.First().Category ?? "Khác",
                TongThoiGianSuDung = TimeSpan.FromMinutes(g.Sum(a => a.TotalUsageMinutes)),
                SoLanMo = g.Sum(a => a.LaunchCount),
                LanCuoiSuDung = g.Max(a => a.LastActive),
                LaUngDungChan = g.Any(a => a.IsBlocked)
            })
            .ToList();
        
        var theoLoai = chiTiet
            .GroupBy(c => c.LoaiUngDung)
            .ToDictionary(
                g => g.Key,
                g => TimeSpan.FromMinutes(g.Sum(c => c.TongThoiGianSuDung.TotalMinutes))
            );
        
        var ungDungBiChan = ungDung
            .Where(a => a.IsBlocked)
            .GroupBy(a => a.ApplicationName)
            .Select(g => new UngDungCamSuDung
            {
                TenUngDung = g.Key,
                SoLanChan = g.Count(),
                LanCuoiChan = g.Max(a => a.LastActive)
            })
            .ToList();
        
        return new BaoCaoUngDung
        {
            TuNgay = tuNgay,
            DenNgay = denNgay,
            TongSoUngDung = chiTiet.Count,
            DanhSachUngDung = chiTiet,
            ThoiGianTheoLoaiUngDung = theoLoai,
            UngDungBiChan = ungDungBiChan
        };
    }

    /// <summary>
    /// Tạo báo cáo website đã truy cập
    /// </summary>
    public async Task<BaoCaoWebsite> TaoBaoCaoWebsiteAsync(DateTime tuNgay, DateTime denNgay)
    {
        // Lấy từ ActivityLog
        var activities = await _dbContext.ActivityLogEntries
            .Where(a => a.Timestamp >= tuNgay && 
                       a.Timestamp <= denNgay &&
                       a.ActivityType == "Website")
            .ToListAsync();
        
        var websites = activities
            .GroupBy(a => a.Details ?? "Unknown")
            .Select(g => new ChiTietWebsite
            {
                Url = g.Key,
                TenMien = ExtractDomain(g.Key),
                SoLanTruyCap = g.Count(),
                ThoiGianXem = TimeSpan.FromMinutes(5 * g.Count()), // Ước tính
                LanCuoiTruyCap = g.Max(a => a.Timestamp),
                TheLoai = "General",
                LaWebsiteNguyHiem = false
            })
            .ToList();
        
        var websiteBiChan = await _dbContext.BlockedWebsites
            .Where(w => w.CreatedAt >= tuNgay && w.CreatedAt <= denNgay)
            .Select(w => new WebsiteBiChan
            {
                Url = w.Url,
                SoLanChan = 0, // TODO: Track block count
                LyDoChan = w.Reason ?? "Nội dung không phù hợp"
            })
            .ToListAsync();
        
        return new BaoCaoWebsite
        {
            TuNgay = tuNgay,
            DenNgay = denNgay,
            TongSoWebsite = websites.Count,
            DanhSachWebsite = websites,
            TheLoaiWebsite = websites.GroupBy(w => w.TheLoai)
                .ToDictionary(g => g.Key, g => g.Count()),
            WebsiteDaChan = websiteBiChan
        };
    }

    /// <summary>
    /// Tạo báo cáo vi phạm và cảnh báo
    /// </summary>
    public async Task<BaoCaoViPham> TaoBaoCaoViPhamAsync(DateTime tuNgay, DateTime denNgay)
    {
        var thongBao = await _dbContext.NotificationLogs
            .Where(n => n.CreatedAt >= tuNgay && 
                       n.CreatedAt <= denNgay &&
                       (n.Type == "Warning" || n.Type == "Violation"))
            .ToListAsync();
        
        var viPham = thongBao.Select(n => new ChiTietViPham
        {
            ThoiDiem = n.CreatedAt,
            LoaiViPham = n.Type,
            MoTa = n.Message,
            MucDoNghiemTrong = n.Priority ?? "Normal",
            HanhDongDaThucHien = n.ActionTaken ?? "Ghi nhận"
        }).ToList();
        
        var theoLoai = viPham
            .GroupBy(v => v.LoaiViPham)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var khuyenNghi = new List<KhuyenNghi>();
        
        // Phân tích và đưa ra khuyến nghị
        if (viPham.Count > 10)
        {
            khuyenNghi.Add(new KhuyenNghi
            {
                NoiDung = "Số lượng vi phạm cao, cần tăng cường giám sát",
                MucDoUuTien = "Cao",
                HanhDongDeNghi = "Thiết lập giới hạn thời gian chặt chẽ hơn"
            });
        }
        
        if (viPham.Any(v => v.MucDoNghiemTrong == "High"))
        {
            khuyenNghi.Add(new KhuyenNghi
            {
                NoiDung = "Có vi phạm nghiêm trọng cần xử lý",
                MucDoUuTien = "Rất cao",
                HanhDongDeNghi = "Xem xét chi tiết và có biện pháp ngay"
            });
        }
        
        return new BaoCaoViPham
        {
            TuNgay = tuNgay,
            DenNgay = denNgay,
            TongSoViPham = viPham.Count,
            ViPhamTheoLoai = theoLoai,
            DanhSachViPham = viPham,
            CacKhuyenNghi = khuyenNghi
        };
    }

    /// <summary>
    /// Tạo báo cáo thời gian sử dụng chi tiết
    /// </summary>
    public async Task<BaoCaoThoiGianSuDung> TaoBaoCaoThoiGianAsync(string nguoiDung, DateTime tuNgay, DateTime denNgay)
    {
        var soNgay = (denNgay - tuNgay).Days + 1;
        var tongThoiGian = TimeSpan.Zero;
        var thoiGianTheoNgay = new Dictionary<DateTime, TimeSpan>();
        var phanBoTheoGio = new Dictionary<int, TimeSpan>();
        
        // Khởi tạo phân bố theo giờ
        for (int i = 0; i < 24; i++)
        {
            phanBoTheoGio[i] = TimeSpan.Zero;
        }
        
        // Lấy thống kê từng ngày
        for (var ngay = tuNgay.Date; ngay <= denNgay.Date; ngay = ngay.AddDays(1))
        {
            var thongKeNgay = await _timeManagementService.LayThongKeTheoNgayAsync(nguoiDung, ngay);
            tongThoiGian += thongKeNgay.TongThoiGianSuDung;
            thoiGianTheoNgay[ngay] = thongKeNgay.TongThoiGianSuDung;
            
            // Cộng dồn phân bố theo giờ
            foreach (var kvp in thongKeNgay.TheoGio)
            {
                phanBoTheoGio[kvp.Key] += kvp.Value;
            }
        }
        
        // Tính thời gian năng suất vs giải trí
        var ungDungNangSuat = await _dbContext.MonitoredApplications
            .Where(a => a.LastActive >= tuNgay && 
                       a.LastActive <= denNgay &&
                       (a.Category == "Office" || a.Category == "Development"))
            .SumAsync(a => a.TotalUsageMinutes);
        
        var ungDungGiaiTri = await _dbContext.MonitoredApplications
            .Where(a => a.LastActive >= tuNgay && 
                       a.LastActive <= denNgay &&
                       (a.Category == "Games" || a.Category == "Entertainment"))
            .SumAsync(a => a.TotalUsageMinutes);
        
        return new BaoCaoThoiGianSuDung
        {
            NguoiDung = nguoiDung,
            TuNgay = tuNgay,
            DenNgay = denNgay,
            TongThoiGian = tongThoiGian,
            TrungBinhMoiNgay = TimeSpan.FromMinutes(tongThoiGian.TotalMinutes / soNgay),
            ThoiGianTheoNgay = thoiGianTheoNgay,
            PhanBoTheoGio = phanBoTheoGio,
            ThoiGianNangSuat = TimeSpan.FromMinutes(ungDungNangSuat),
            ThoiGianGiaiTri = TimeSpan.FromMinutes(ungDungGiaiTri)
        };
    }

    /// <summary>
    /// Xuất báo cáo ra file PDF
    /// </summary>
    public async Task<string> XuatBaoCaoPdfAsync(object baoCao, string tenFile)
    {
        var duongDan = Path.Combine(_thuMucBaoCao, $"{tenFile}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        
        // TODO: Implement PDF generation using a library like iTextSharp or PdfSharpCore
        // Tạm thời xuất JSON
        var json = JsonConvert.SerializeObject(baoCao, Formatting.Indented);
        var html = TaoHtmlTuBaoCao(baoCao, json);
        
        await File.WriteAllTextAsync(duongDan.Replace(".pdf", ".html"), html);
        
        _logger.LogInformation("Đã xuất báo cáo PDF: {DuongDan}", duongDan);
        return duongDan.Replace(".pdf", ".html");
    }

    /// <summary>
    /// Xuất báo cáo ra file Excel
    /// </summary>
    public async Task<string> XuatBaoCaoExcelAsync(object baoCao, string tenFile)
    {
        var duongDan = Path.Combine(_thuMucBaoCao, $"{tenFile}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        
        // TODO: Implement Excel generation using EPPlus or similar
        // Tạm thời xuất CSV
        var csv = TaoCsvTuBaoCao(baoCao);
        await File.WriteAllTextAsync(duongDan, csv);
        
        _logger.LogInformation("Đã xuất báo cáo Excel: {DuongDan}", duongDan);
        return duongDan;
    }

    /// <summary>
    /// Xuất báo cáo ra file HTML
    /// </summary>
    public async Task<string> XuatBaoCaoHtmlAsync(object baoCao, string tenFile)
    {
        var duongDan = Path.Combine(_thuMucBaoCao, $"{tenFile}_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        
        var json = JsonConvert.SerializeObject(baoCao, Formatting.Indented);
        var html = TaoHtmlTuBaoCao(baoCao, json);
        
        await File.WriteAllTextAsync(duongDan, html);
        
        _logger.LogInformation("Đã xuất báo cáo HTML: {DuongDan}", duongDan);
        return duongDan;
    }

    /// <summary>
    /// Gửi báo cáo qua email
    /// </summary>
    public async Task GuiBaoCaoQuaEmailAsync(object baoCao, string emailNhan, string chuDe)
    {
        // TODO: Implement email sending using SMTP
        _logger.LogInformation("Gửi báo cáo qua email tới {Email}: {ChuDe}", emailNhan, chuDe);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Lập lịch gửi báo cáo tự động
    /// </summary>
    public async Task LapLichGuiBaoCaoAsync(LoaiBaoCao loaiBaoCao, TanSuatGui tanSuat, string emailNhan)
    {
        // TODO: Implement scheduling using Windows Task Scheduler or Quartz.NET
        _logger.LogInformation("Lập lịch gửi báo cáo {LoaiBaoCao} với tần suất {TanSuat} tới {Email}", 
            loaiBaoCao, tanSuat, emailNhan);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Lấy danh sách báo cáo đã tạo
    /// </summary>
    public async Task<IEnumerable<ThongTinBaoCao>> LayDanhSachBaoCaoAsync(int soLuong = 20)
    {
        return await Task.Run(() =>
        {
            var files = Directory.GetFiles(_thuMucBaoCao)
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .Take(soLuong)
                .Select(f => new ThongTinBaoCao
                {
                    Id = Guid.NewGuid(),
                    TenBaoCao = Path.GetFileNameWithoutExtension(f.Name),
                    LoaiBaoCao = XacDinhLoaiBaoCao(f.Name),
                    NgayTao = f.CreationTime,
                    DuongDanFile = f.FullName,
                    KichThuocFile = f.Length,
                    NguoiTao = Environment.UserName
                });
            
            return files;
        });
    }

    /// <summary>
    /// Tạo biểu đồ thống kê
    /// </summary>
    public async Task<BieuDoThongKe> TaoBieuDoThongKeAsync(LoaiBieuDo loaiBieuDo, DateTime tuNgay, DateTime denNgay)
    {
        var nguoiDung = Environment.UserName;
        var duLieu = new List<DiemDuLieu>();
        
        // Lấy dữ liệu theo ngày
        for (var ngay = tuNgay.Date; ngay <= denNgay.Date; ngay = ngay.AddDays(1))
        {
            var thongKe = await _timeManagementService.LayThongKeTheoNgayAsync(nguoiDung, ngay);
            duLieu.Add(new DiemDuLieu
            {
                Nhan = ngay.ToString("dd/MM"),
                GiaTri = thongKe.TongThoiGianSuDung.TotalHours,
                MauSac = thongKe.VuotGioiHan ? "#FF0000" : "#00FF00"
            });
        }
        
        return new BieuDoThongKe
        {
            LoaiBieuDo = loaiBieuDo,
            TieuDe = $"Thống kê từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}",
            DuLieu = duLieu,
            CauHinh = new Dictionary<string, object>
            {
                ["showLegend"] = true,
                ["showGrid"] = true,
                ["animate"] = true
            }
        };
    }

    /// <summary>
    /// So sánh báo cáo giữa các kỳ
    /// </summary>
    public async Task<SoSanhBaoCao> SoSanhBaoCaoAsync(DateTime ky1TuNgay, DateTime ky1DenNgay, 
        DateTime ky2TuNgay, DateTime ky2DenNgay)
    {
        var nguoiDung = Environment.UserName;
        
        // Tính các chỉ số kỳ 1
        var ky1 = new KyBaoCao
        {
            TuNgay = ky1TuNgay,
            DenNgay = ky1DenNgay,
            CacChiSo = new Dictionary<string, double>()
        };
        
        var baoCaoThoiGianKy1 = await TaoBaoCaoThoiGianAsync(nguoiDung, ky1TuNgay, ky1DenNgay);
        ky1.CacChiSo["TongGioSuDung"] = baoCaoThoiGianKy1.TongThoiGian.TotalHours;
        ky1.CacChiSo["TrungBinhNgay"] = baoCaoThoiGianKy1.TrungBinhMoiNgay.TotalHours;
        
        // Tính các chỉ số kỳ 2
        var ky2 = new KyBaoCao
        {
            TuNgay = ky2TuNgay,
            DenNgay = ky2DenNgay,
            CacChiSo = new Dictionary<string, double>()
        };
        
        var baoCaoThoiGianKy2 = await TaoBaoCaoThoiGianAsync(nguoiDung, ky2TuNgay, ky2DenNgay);
        ky2.CacChiSo["TongGioSuDung"] = baoCaoThoiGianKy2.TongThoiGian.TotalHours;
        ky2.CacChiSo["TrungBinhNgay"] = baoCaoThoiGianKy2.TrungBinhMoiNgay.TotalHours;
        
        // Tính chênh lệch
        var chenhLech = new Dictionary<string, ChenhLech>();
        foreach (var chiSo in ky1.CacChiSo.Keys)
        {
            var giaTri1 = ky1.CacChiSo[chiSo];
            var giaTri2 = ky2.CacChiSo[chiSo];
            
            chenhLech[chiSo] = new ChenhLech
            {
                GiaTriKy1 = giaTri1,
                GiaTriKy2 = giaTri2,
                ChenhLechTuyetDoi = giaTri2 - giaTri1,
                ChenhLechPhanTram = giaTri1 > 0 ? (giaTri2 - giaTri1) * 100 / giaTri1 : 0,
                XuHuong = giaTri2 > giaTri1 ? "Tăng" : giaTri2 < giaTri1 ? "Giảm" : "Không đổi"
            };
        }
        
        // Nhận xét
        var nhanXet = new List<string>();
        if (chenhLech["TongGioSuDung"].ChenhLechPhanTram > 20)
        {
            nhanXet.Add("Thời gian sử dụng tăng đáng kể so với kỳ trước");
        }
        else if (chenhLech["TongGioSuDung"].ChenhLechPhanTram < -20)
        {
            nhanXet.Add("Thời gian sử dụng giảm tốt so với kỳ trước");
        }
        
        return new SoSanhBaoCao
        {
            Ky1 = ky1,
            Ky2 = ky2,
            CacChenhLech = chenhLech,
            NhanXet = nhanXet
        };
    }

    /// <summary>
    /// Lưu trữ báo cáo
    /// </summary>
    public async Task<Guid> LuuTruBaoCaoAsync(object baoCao, string tenBaoCao)
    {
        var id = Guid.NewGuid();
        var duongDan = Path.Combine(_thuMucBaoCao, $"{tenBaoCao}_{id}.json");
        
        var json = JsonConvert.SerializeObject(baoCao, Formatting.Indented);
        await File.WriteAllTextAsync(duongDan, json);
        
        _logger.LogInformation("Đã lưu báo cáo {TenBaoCao} với ID {Id}", tenBaoCao, id);
        return id;
    }

    /// <summary>
    /// Xóa báo cáo cũ
    /// </summary>
    public async Task<int> XoaBaoCaoCuAsync(int soNgayGiuLai = 30)
    {
        return await Task.Run(() =>
        {
            var ngayGioiHan = DateTime.Now.AddDays(-soNgayGiuLai);
            var files = Directory.GetFiles(_thuMucBaoCao)
                .Select(f => new FileInfo(f))
                .Where(f => f.CreationTime < ngayGioiHan)
                .ToList();
            
            var soLuong = 0;
            foreach (var file in files)
            {
                try
                {
                    file.Delete();
                    soLuong++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể xóa file {File}", file.Name);
                }
            }
            
            _logger.LogInformation("Đã xóa {SoLuong} báo cáo cũ", soLuong);
            return soLuong;
        });
    }

    // Các phương thức hỗ trợ
    private string ExtractDomain(string url)
    {
        try
        {
            var uri = new Uri(url);
            return uri.Host;
        }
        catch
        {
            return url;
        }
    }

    private LoaiBaoCao XacDinhLoaiBaoCao(string tenFile)
    {
        if (tenFile.Contains("Ngay")) return LoaiBaoCao.BaoCaoNgay;
        if (tenFile.Contains("Tuan")) return LoaiBaoCao.BaoCaoTuan;
        if (tenFile.Contains("Thang")) return LoaiBaoCao.BaoCaoThang;
        if (tenFile.Contains("UngDung")) return LoaiBaoCao.BaoCaoUngDung;
        if (tenFile.Contains("Website")) return LoaiBaoCao.BaoCaoWebsite;
        if (tenFile.Contains("ViPham")) return LoaiBaoCao.BaoCaoViPham;
        if (tenFile.Contains("ThoiGian")) return LoaiBaoCao.BaoCaoThoiGian;
        return LoaiBaoCao.BaoCaoTongHop;
    }

    private string TaoHtmlTuBaoCao(object baoCao, string json)
    {
        var html = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <title>Báo cáo KidGuard</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        h1 {{ color: #333; }}
        pre {{ background: #f4f4f4; padding: 15px; border-radius: 5px; }}
        .info {{ background: #e8f4f8; padding: 10px; margin: 10px 0; border-left: 4px solid #0066cc; }}
    </style>
</head>
<body>
    <h1>Báo cáo KidGuard</h1>
    <div class='info'>
        <strong>Ngày tạo:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}<br>
        <strong>Loại báo cáo:</strong> {baoCao.GetType().Name}
    </div>
    <h2>Chi tiết báo cáo</h2>
    <pre>{json}</pre>
</body>
</html>";
        return html;
    }

    private string TaoCsvTuBaoCao(object baoCao)
    {
        var csv = new StringBuilder();
        csv.AppendLine($"Báo cáo KidGuard - {DateTime.Now:dd/MM/yyyy HH:mm}");
        csv.AppendLine($"Loại báo cáo: {baoCao.GetType().Name}");
        csv.AppendLine();
        
        // Serialize object to CSV format
        var json = JsonConvert.SerializeObject(baoCao);
        csv.AppendLine(json);
        
        return csv.ToString();
    }
}