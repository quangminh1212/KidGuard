using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KidGuard.Core.Interfaces;

/// <summary>
/// Dịch vụ tạo báo cáo và thống kê
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Tạo báo cáo tổng quan hàng ngày
    /// </summary>
    Task<BaoCaoNgay> TaoBaoCaoNgayAsync(DateTime ngay);
    
    /// <summary>
    /// Tạo báo cáo tổng quan hàng tuần
    /// </summary>
    Task<BaoCaoTuan> TaoBaoCaoTuanAsync(DateTime tuanBatDau);
    
    /// <summary>
    /// Tạo báo cáo tổng quan hàng tháng
    /// </summary>
    Task<BaoCaoThang> TaoBaoCaoThangAsync(int thang, int nam);
    
    /// <summary>
    /// Tạo báo cáo hoạt động ứng dụng
    /// </summary>
    Task<BaoCaoUngDung> TaoBaoCaoUngDungAsync(DateTime tuNgay, DateTime denNgay);
    
    /// <summary>
    /// Tạo báo cáo website đã truy cập
    /// </summary>
    Task<BaoCaoWebsite> TaoBaoCaoWebsiteAsync(DateTime tuNgay, DateTime denNgay);
    
    /// <summary>
    /// Tạo báo cáo vi phạm và cảnh báo
    /// </summary>
    Task<BaoCaoViPham> TaoBaoCaoViPhamAsync(DateTime tuNgay, DateTime denNgay);
    
    /// <summary>
    /// Tạo báo cáo thời gian sử dụng chi tiết
    /// </summary>
    Task<BaoCaoThoiGianSuDung> TaoBaoCaoThoiGianAsync(string nguoiDung, DateTime tuNgay, DateTime denNgay);
    
    /// <summary>
    /// Xuất báo cáo ra file PDF
    /// </summary>
    Task<string> XuatBaoCaoPdfAsync(object baoCao, string tenFile);
    
    /// <summary>
    /// Xuất báo cáo ra file Excel
    /// </summary>
    Task<string> XuatBaoCaoExcelAsync(object baoCao, string tenFile);
    
    /// <summary>
    /// Xuất báo cáo ra file HTML
    /// </summary>
    Task<string> XuatBaoCaoHtmlAsync(object baoCao, string tenFile);
    
    /// <summary>
    /// Gửi báo cáo qua email
    /// </summary>
    Task GuiBaoCaoQuaEmailAsync(object baoCao, string emailNhan, string chuDe);
    
    /// <summary>
    /// Lập lịch gửi báo cáo tự động
    /// </summary>
    Task LapLichGuiBaoCaoAsync(LoaiBaoCao loaiBaoCao, TanSuatGui tanSuat, string emailNhan);
    
    /// <summary>
    /// Lấy danh sách báo cáo đã tạo
    /// </summary>
    Task<IEnumerable<ThongTinBaoCao>> LayDanhSachBaoCaoAsync(int soLuong = 20);
    
    /// <summary>
    /// Tạo biểu đồ thống kê
    /// </summary>
    Task<BieuDoThongKe> TaoBieuDoThongKeAsync(LoaiBieuDo loaiBieuDo, DateTime tuNgay, DateTime denNgay);
    
    /// <summary>
    /// So sánh báo cáo giữa các kỳ
    /// </summary>
    Task<SoSanhBaoCao> SoSanhBaoCaoAsync(DateTime ky1TuNgay, DateTime ky1DenNgay, DateTime ky2TuNgay, DateTime ky2DenNgay);
    
    /// <summary>
    /// Lưu trữ báo cáo
    /// </summary>
    Task<Guid> LuuTruBaoCaoAsync(object baoCao, string tenBaoCao);
    
    /// <summary>
    /// Xóa báo cáo cũ
    /// </summary>
    Task<int> XoaBaoCaoCuAsync(int soNgayGiuLai = 30);
}

/// <summary>
/// Báo cáo tổng quan hàng ngày
/// </summary>
public class BaoCaoNgay
{
    public DateTime Ngay { get; set; }
    public TimeSpan TongThoiGianSuDung { get; set; }
    public int SoUngDungDaSuDung { get; set; }
    public int SoWebsiteDaTruyCap { get; set; }
    public int SoLanViPham { get; set; }
    public List<HoatDongTheoGio> HoatDongTheoGio { get; set; } = new();
    public List<UngDungSuDungNhieuNhat> TopUngDung { get; set; } = new();
    public List<WebsiteTruyCapNhieuNhat> TopWebsite { get; set; } = new();
    public string? GhiChu { get; set; }
}

/// <summary>
/// Báo cáo tổng quan hàng tuần
/// </summary>
public class BaoCaoTuan
{
    public DateTime TuanBatDau { get; set; }
    public DateTime TuanKetThuc { get; set; }
    public TimeSpan TongThoiGianSuDung { get; set; }
    public TimeSpan TrungBinhMoiNgay { get; set; }
    public Dictionary<DayOfWeek, TimeSpan> ThoiGianTheoNgay { get; set; } = new();
    public int TongSoViPham { get; set; }
    public List<NgayHoatDongNhieuNhat> NgayHoatDongCao { get; set; } = new();
    public TrendAnalysis XuHuongSuDung { get; set; } = new();
}

/// <summary>
/// Báo cáo tổng quan hàng tháng
/// </summary>
public class BaoCaoThang
{
    public int Thang { get; set; }
    public int Nam { get; set; }
    public TimeSpan TongThoiGianSuDung { get; set; }
    public Dictionary<int, TimeSpan> ThoiGianTheoNgay { get; set; } = new();
    public List<UngDungSuDungNhieuNhat> Top10UngDung { get; set; } = new();
    public List<WebsiteTruyCapNhieuNhat> Top10Website { get; set; } = new();
    public int TongSoViPham { get; set; }
    public double PhanTramTangGiam { get; set; } // So với tháng trước
}

/// <summary>
/// Báo cáo hoạt động ứng dụng
/// </summary>
public class BaoCaoUngDung
{
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public int TongSoUngDung { get; set; }
    public List<ChiTietUngDung> DanhSachUngDung { get; set; } = new();
    public Dictionary<string, TimeSpan> ThoiGianTheoLoaiUngDung { get; set; } = new();
    public List<UngDungCamSuDung> UngDungBiChan { get; set; } = new();
}

/// <summary>
/// Chi tiết ứng dụng trong báo cáo
/// </summary>
public class ChiTietUngDung
{
    public string TenUngDung { get; set; } = string.Empty;
    public string LoaiUngDung { get; set; } = string.Empty;
    public TimeSpan TongThoiGianSuDung { get; set; }
    public int SoLanMo { get; set; }
    public DateTime LanCuoiSuDung { get; set; }
    public bool LaUngDungChan { get; set; }
}

/// <summary>
/// Báo cáo website đã truy cập
/// </summary>
public class BaoCaoWebsite
{
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public int TongSoWebsite { get; set; }
    public List<ChiTietWebsite> DanhSachWebsite { get; set; } = new();
    public Dictionary<string, int> TheLoaiWebsite { get; set; } = new();
    public List<WebsiteBiChan> WebsiteDaChan { get; set; } = new();
}

/// <summary>
/// Chi tiết website trong báo cáo
/// </summary>
public class ChiTietWebsite
{
    public string Url { get; set; } = string.Empty;
    public string TenMien { get; set; } = string.Empty;
    public int SoLanTruyCap { get; set; }
    public TimeSpan ThoiGianXem { get; set; }
    public DateTime LanCuoiTruyCap { get; set; }
    public string TheLoai { get; set; } = string.Empty;
    public bool LaWebsiteNguyHiem { get; set; }
}

/// <summary>
/// Báo cáo vi phạm và cảnh báo
/// </summary>
public class BaoCaoViPham
{
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public int TongSoViPham { get; set; }
    public Dictionary<string, int> ViPhamTheoLoai { get; set; } = new();
    public List<ChiTietViPham> DanhSachViPham { get; set; } = new();
    public List<KhuyenNghi> CacKhuyenNghi { get; set; } = new();
}

/// <summary>
/// Chi tiết vi phạm
/// </summary>
public class ChiTietViPham
{
    public DateTime ThoiDiem { get; set; }
    public string LoaiViPham { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
    public string MucDoNghiemTrong { get; set; } = string.Empty;
    public string HanhDongDaThucHien { get; set; } = string.Empty;
}

/// <summary>
/// Báo cáo thời gian sử dụng chi tiết
/// </summary>
public class BaoCaoThoiGianSuDung
{
    public string NguoiDung { get; set; } = string.Empty;
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public TimeSpan TongThoiGian { get; set; }
    public TimeSpan TrungBinhMoiNgay { get; set; }
    public Dictionary<DateTime, TimeSpan> ThoiGianTheoNgay { get; set; } = new();
    public Dictionary<int, TimeSpan> PhanBoTheoGio { get; set; } = new(); // 0-23
    public TimeSpan ThoiGianNangSuat { get; set; } // Thời gian dùng app năng suất
    public TimeSpan ThoiGianGiaiTri { get; set; } // Thời gian dùng app giải trí
}

/// <summary>
/// Thông tin báo cáo đã lưu
/// </summary>
public class ThongTinBaoCao
{
    public Guid Id { get; set; }
    public string TenBaoCao { get; set; } = string.Empty;
    public LoaiBaoCao LoaiBaoCao { get; set; }
    public DateTime NgayTao { get; set; }
    public string DuongDanFile { get; set; } = string.Empty;
    public long KichThuocFile { get; set; }
    public string NguoiTao { get; set; } = string.Empty;
}

/// <summary>
/// Biểu đồ thống kê
/// </summary>
public class BieuDoThongKe
{
    public LoaiBieuDo LoaiBieuDo { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public List<DiemDuLieu> DuLieu { get; set; } = new();
    public Dictionary<string, object> CauHinh { get; set; } = new();
}

/// <summary>
/// Điểm dữ liệu trong biểu đồ
/// </summary>
public class DiemDuLieu
{
    public string Nhan { get; set; } = string.Empty;
    public double GiaTri { get; set; }
    public string? MauSac { get; set; }
    public Dictionary<string, object>? MetaData { get; set; }
}

/// <summary>
/// So sánh báo cáo giữa các kỳ
/// </summary>
public class SoSanhBaoCao
{
    public KyBaoCao Ky1 { get; set; } = new();
    public KyBaoCao Ky2 { get; set; } = new();
    public Dictionary<string, ChenhLech> CacChenhLech { get; set; } = new();
    public List<string> NhanXet { get; set; } = new();
}

/// <summary>
/// Thông tin kỳ báo cáo
/// </summary>
public class KyBaoCao
{
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public Dictionary<string, double> CacChiSo { get; set; } = new();
}

/// <summary>
/// Chênh lệch giữa hai kỳ
/// </summary>
public class ChenhLech
{
    public double GiaTriKy1 { get; set; }
    public double GiaTriKy2 { get; set; }
    public double ChenhLechTuyetDoi { get; set; }
    public double ChenhLechPhanTram { get; set; }
    public string XuHuong { get; set; } = string.Empty; // "Tăng", "Giảm", "Không đổi"
}

/// <summary>
/// Các lớp hỗ trợ
/// </summary>
public class HoatDongTheoGio
{
    public int Gio { get; set; }
    public TimeSpan ThoiGianSuDung { get; set; }
    public int SoUngDungDaMo { get; set; }
}

public class UngDungSuDungNhieuNhat
{
    public string TenUngDung { get; set; } = string.Empty;
    public TimeSpan ThoiGianSuDung { get; set; }
    public double PhanTram { get; set; }
}

public class WebsiteTruyCapNhieuNhat
{
    public string Url { get; set; } = string.Empty;
    public int SoLanTruyCap { get; set; }
    public TimeSpan ThoiGianXem { get; set; }
}

public class NgayHoatDongNhieuNhat
{
    public DateTime Ngay { get; set; }
    public TimeSpan ThoiGianSuDung { get; set; }
    public string LyDo { get; set; } = string.Empty;
}

public class TrendAnalysis
{
    public string XuHuong { get; set; } = string.Empty; // "Tăng", "Giảm", "Ổn định"
    public double PhanTramThayDoi { get; set; }
    public string DuBao { get; set; } = string.Empty;
}

public class UngDungCamSuDung
{
    public string TenUngDung { get; set; } = string.Empty;
    public int SoLanChan { get; set; }
    public DateTime LanCuoiChan { get; set; }
}

public class WebsiteBiChan
{
    public string Url { get; set; } = string.Empty;
    public int SoLanChan { get; set; }
    public string LyDoChan { get; set; } = string.Empty;
}

public class KhuyenNghi
{
    public string NoiDung { get; set; } = string.Empty;
    public string MucDoUuTien { get; set; } = string.Empty;
    public string HanhDongDeNghi { get; set; } = string.Empty;
}

/// <summary>
/// Enum các loại
/// </summary>
public enum LoaiBaoCao
{
    BaoCaoNgay,
    BaoCaoTuan,
    BaoCaoThang,
    BaoCaoUngDung,
    BaoCaoWebsite,
    BaoCaoViPham,
    BaoCaoThoiGian,
    BaoCaoTongHop
}

public enum TanSuatGui
{
    HangNgay,
    HangTuan,
    HangThang,
    TuyChon
}

public enum LoaiBieuDo
{
    Cot,
    Duong,
    Tron,
    Vung,
    Thanh,
    KetHop
}