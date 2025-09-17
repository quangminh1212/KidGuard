using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KidGuard.Core.Interfaces;

/// <summary>
/// Dịch vụ quản lý thời gian sử dụng máy tính
/// </summary>
public interface ITimeManagementService
{
    /// <summary>
    /// Bắt đầu theo dõi phiên sử dụng mới
    /// </summary>
    Task<Guid> BatDauPhienSuDungAsync(string nguoiDung);
    
    /// <summary>
    /// Kết thúc phiên sử dụng hiện tại
    /// </summary>
    Task KetThucPhienSuDungAsync(Guid idPhien);
    
    /// <summary>
    /// Lấy thời gian sử dụng trong ngày
    /// </summary>
    Task<TimeSpan> LayThoiGianSuDungTrongNgayAsync(string nguoiDung, DateTime? ngay = null);
    
    /// <summary>
    /// Lấy thời gian sử dụng trong tuần
    /// </summary>
    Task<TimeSpan> LayThoiGianSuDungTrongTuanAsync(string nguoiDung);
    
    /// <summary>
    /// Lấy lịch sử phiên sử dụng
    /// </summary>
    Task<IEnumerable<PhienSuDung>> LayLichSuPhienAsync(string nguoiDung, DateTime tuNgay, DateTime denNgay);
    
    /// <summary>
    /// Thiết lập giới hạn thời gian hàng ngày
    /// </summary>
    Task ThietLapGioiHanNgayAsync(string nguoiDung, TimeSpan gioiHan);
    
    /// <summary>
    /// Lấy giới hạn thời gian hàng ngày
    /// </summary>
    Task<TimeSpan?> LayGioiHanNgayAsync(string nguoiDung);
    
    /// <summary>
    /// Kiểm tra có vượt quá giới hạn không
    /// </summary>
    Task<bool> KiemTraVuotGioiHanAsync(string nguoiDung);
    
    /// <summary>
    /// Lấy thời gian còn lại trong ngày
    /// </summary>
    Task<TimeSpan> LayThoiGianConLaiAsync(string nguoiDung);
    
    /// <summary>
    /// Thiết lập lịch sử dụng được phép
    /// </summary>
    Task ThietLapLichSuDungAsync(string nguoiDung, LichSuDung lichSuDung);
    
    /// <summary>
    /// Lấy lịch sử dụng hiện tại
    /// </summary>
    Task<LichSuDung?> LayLichSuDungAsync(string nguoiDung);
    
    /// <summary>
    /// Kiểm tra có được phép sử dụng vào thời điểm hiện tại
    /// </summary>
    Task<bool> KiemTraChoPhepSuDungAsync(string nguoiDung, DateTime? thoiDiem = null);
    
    /// <summary>
    /// Tạm khóa máy tính khi hết thời gian
    /// </summary>
    Task TamKhoaMayTinhAsync(string lyDo);
    
    /// <summary>
    /// Gửi cảnh báo sắp hết thời gian
    /// </summary>
    Task GuiCanhBaoSapHetThoiGianAsync(string nguoiDung, TimeSpan thoiGianConLai);
    
    /// <summary>
    /// Lấy thống kê sử dụng theo ngày
    /// </summary>
    Task<ThongKeSuDungTheoNgay> LayThongKeTheoNgayAsync(string nguoiDung, DateTime ngay);
    
    /// <summary>
    /// Lấy thống kê sử dụng theo tuần
    /// </summary>
    Task<ThongKeSuDungTheoTuan> LayThongKeTheoTuanAsync(string nguoiDung, DateTime tuanBatDau);
    
    /// <summary>
    /// Lấy thống kê sử dụng theo tháng
    /// </summary>
    Task<ThongKeSuDungTheoThang> LayThongKeTheoThangAsync(string nguoiDung, int thang, int nam);
    
    /// <summary>
    /// Thêm thời gian bonus (phần thưởng)
    /// </summary>
    Task ThemThoiGianBonusAsync(string nguoiDung, TimeSpan thoiGianBonus, string lyDo);
    
    /// <summary>
    /// Đặt lại (reset) thời gian sử dụng
    /// </summary>
    Task ResetThoiGianSuDungAsync(string nguoiDung);
    
    /// <summary>
    /// Xuất báo cáo thời gian sử dụng
    /// </summary>
    Task<string> XuatBaoCaoAsync(string nguoiDung, DateTime tuNgay, DateTime denNgay, string dinhDang = "PDF");
    
    /// <summary>
    /// Lấy danh sách cảnh báo vi phạm thời gian
    /// </summary>
    Task<IEnumerable<CanhBaoViPham>> LayDanhSachCanhBaoAsync(string nguoiDung, int soNgayGanDay = 7);
    
    /// <summary>
    /// Thiết lập chế độ nghỉ ngơi bắt buộc
    /// </summary>
    Task ThietLapCheDoNghiNgoiAsync(string nguoiDung, TimeSpan thoiGianSuDung, TimeSpan thoiGianNghi);
    
    /// <summary>
    /// Kiểm tra phải nghỉ ngơi không
    /// </summary>
    Task<bool> KiemTraPhaiNghiNgoiAsync(string nguoiDung);
}

/// <summary>
/// Thông tin phiên sử dụng
/// </summary>
public class PhienSuDung
{
    public Guid Id { get; set; }
    public string NguoiDung { get; set; } = string.Empty;
    public DateTime ThoiGianBatDau { get; set; }
    public DateTime? ThoiGianKetThuc { get; set; }
    public TimeSpan ThoiLuongSuDung => ThoiGianKetThuc.HasValue 
        ? ThoiGianKetThuc.Value - ThoiGianBatDau 
        : DateTime.Now - ThoiGianBatDau;
    public bool DangHoatDong => !ThoiGianKetThuc.HasValue;
    public string? GhiChu { get; set; }
}

/// <summary>
/// Lịch sử dụng được phép
/// </summary>
public class LichSuDung
{
    public Guid Id { get; set; }
    public string NguoiDung { get; set; } = string.Empty;
    public List<KhungGioChoPhep> CacKhungGio { get; set; } = new();
    public bool ApDungCuoiTuan { get; set; } = false;
    public bool KichHoat { get; set; } = true;
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

/// <summary>
/// Khung giờ được phép sử dụng
/// </summary>
public class KhungGioChoPhep
{
    public DayOfWeek NgayTrongTuan { get; set; }
    public TimeSpan GioBatDau { get; set; }
    public TimeSpan GioKetThuc { get; set; }
    public TimeSpan? GioiHanThoiGian { get; set; }
    public bool ChoPhepSuDung { get; set; } = true;
    public string? GhiChu { get; set; }
}

/// <summary>
/// Thống kê sử dụng theo ngày
/// </summary>
public class ThongKeSuDungTheoNgay
{
    public DateTime Ngay { get; set; }
    public string NguoiDung { get; set; } = string.Empty;
    public TimeSpan TongThoiGianSuDung { get; set; }
    public TimeSpan? GioiHanNgay { get; set; }
    public bool VuotGioiHan => GioiHanNgay.HasValue && TongThoiGianSuDung > GioiHanNgay.Value;
    public int SoPhienSuDung { get; set; }
    public TimeSpan ThoiGianBatDauSom { get; set; }
    public TimeSpan ThoiGianKetThucMuon { get; set; }
    public List<PhienSuDung> ChiTietPhien { get; set; } = new();
    public Dictionary<int, TimeSpan> TheoGio { get; set; } = new(); // Key: giờ (0-23), Value: thời gian sử dụng
}

/// <summary>
/// Thống kê sử dụng theo tuần
/// </summary>
public class ThongKeSuDungTheoTuan
{
    public DateTime TuanBatDau { get; set; }
    public DateTime TuanKetThuc { get; set; }
    public string NguoiDung { get; set; } = string.Empty;
    public TimeSpan TongThoiGianSuDung { get; set; }
    public TimeSpan TrungBinhMoiNgay => TimeSpan.FromMinutes(TongThoiGianSuDung.TotalMinutes / 7);
    public Dictionary<DayOfWeek, TimeSpan> TheoNgay { get; set; } = new();
    public int SoNgayVuotGioiHan { get; set; }
    public DateTime? NgaySuDungNhieuNhat { get; set; }
    public DateTime? NgaySuDungItNhat { get; set; }
}

/// <summary>
/// Thống kê sử dụng theo tháng
/// </summary>
public class ThongKeSuDungTheoThang
{
    public int Thang { get; set; }
    public int Nam { get; set; }
    public string NguoiDung { get; set; } = string.Empty;
    public TimeSpan TongThoiGianSuDung { get; set; }
    public TimeSpan TrungBinhMoiNgay => TimeSpan.FromMinutes(TongThoiGianSuDung.TotalMinutes / SoNgayTrongThang);
    public int SoNgayTrongThang { get; set; }
    public int SoNgayVuotGioiHan { get; set; }
    public Dictionary<int, TimeSpan> TheoNgay { get; set; } = new(); // Key: ngày trong tháng, Value: thời gian
    public TimeSpan XuHuong { get; set; } // Tăng/giảm so với tháng trước
}

/// <summary>
/// Cảnh báo vi phạm thời gian
/// </summary>
public class CanhBaoViPham
{
    public Guid Id { get; set; }
    public string NguoiDung { get; set; } = string.Empty;
    public DateTime ThoiDiemViPham { get; set; }
    public LoaiViPham LoaiViPham { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public TimeSpan? ThoiGianVuot { get; set; }
    public bool DaXuLy { get; set; }
    public string? CachXuLy { get; set; }
}

/// <summary>
/// Loại vi phạm thời gian
/// </summary>
public enum LoaiViPham
{
    VuotGioiHanNgay,
    SuDungNgoaiGioChoPhep,
    KhongNghiNgoiTheQuyDinh,
    ViPhamLichSuDung,
    SuDungQuaKhuya,
    SuDungLienTucQuaLau
}