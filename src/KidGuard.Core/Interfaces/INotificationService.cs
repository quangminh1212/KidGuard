using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KidGuard.Core.Interfaces;

/// <summary>
/// Dịch vụ gửi thông báo và cảnh báo
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Gửi thông báo tức thời
    /// </summary>
    Task GuiThongBaoAsync(ThongBao thongBao);
    
    /// <summary>
    /// Gửi cảnh báo
    /// </summary>
    Task GuiCanhBaoAsync(string tieuDe, string noiDung, MucDoCanhBao mucDo);
    
    /// <summary>
    /// Gửi thông báo qua email
    /// </summary>
    Task GuiEmailAsync(string emailNhan, string tieuDe, string noiDung, string? fileDinhKem = null);
    
    /// <summary>
    /// Hiển thị thông báo system tray
    /// </summary>
    Task HienThiSystemTrayAsync(string tieuDe, string noiDung, LoaiIcon loaiIcon);
    
    /// <summary>
    /// Hiển thị cảnh báo trên màn hình
    /// </summary>
    Task HienThiCanhBaoManHinhAsync(string noiDung, int thoiGianHienThi = 5000);
    
    /// <summary>
    /// Gửi thông báo SMS (nếu được cấu hình)
    /// </summary>
    Task GuiSmsAsync(string soDienThoai, string noiDung);
    
    /// <summary>
    /// Gửi thông báo push notification
    /// </summary>
    Task GuiPushNotificationAsync(string deviceId, string tieuDe, string noiDung);
    
    /// <summary>
    /// Gửi thông báo đến phụ huynh
    /// </summary>
    Task GuiThongBaoPhuHuynhAsync(ThongBaoPhuHuynh thongBao);
    
    /// <summary>
    /// Ghi log thông báo
    /// </summary>
    Task GhiLogThongBaoAsync(ThongBao thongBao);
    
    /// <summary>
    /// Lấy lịch sử thông báo
    /// </summary>
    Task<IEnumerable<LichSuThongBao>> LayLichSuThongBaoAsync(DateTime tuNgay, DateTime denNgay);
    
    /// <summary>
    /// Đánh dấu thông báo đã đọc
    /// </summary>
    Task DanhDauDaDocAsync(Guid idThongBao);
    
    /// <summary>
    /// Xóa thông báo cũ
    /// </summary>
    Task<int> XoaThongBaoCuAsync(int soNgayGiuLai = 30);
    
    /// <summary>
    /// Thiết lập cấu hình thông báo
    /// </summary>
    Task ThietLapCauHinhAsync(CauHinhThongBao cauHinh);
    
    /// <summary>
    /// Lấy cấu hình thông báo hiện tại
    /// </summary>
    Task<CauHinhThongBao> LayCauHinhAsync();
    
    /// <summary>
    /// Kiểm tra kết nối dịch vụ thông báo
    /// </summary>
    Task<bool> KiemTraKetNoiAsync();
    
    /// <summary>
    /// Đăng ký nhận thông báo
    /// </summary>
    Task DangKyNhanThongBaoAsync(string userId, LoaiThongBao[] cacLoai);
    
    /// <summary>
    /// Hủy đăng ký nhận thông báo
    /// </summary>
    Task HuyDangKyThongBaoAsync(string userId, LoaiThongBao[] cacLoai);
    
    /// <summary>
    /// Gửi báo cáo hàng ngày
    /// </summary>
    Task GuiBaoCaoHangNgayAsync(string emailNhan);
    
    /// <summary>
    /// Gửi báo cáo khẩn cấp
    /// </summary>
    Task GuiBaoCaoKhanCapAsync(string tieuDe, string noiDung, string[] emailNhan);
    
    /// <summary>
    /// Thực hiện hành động khi có vi phạm
    /// </summary>
    Task XuLyViPhamAsync(ViPham viPham);
}

/// <summary>
/// Thông tin thông báo
/// </summary>
public class ThongBao
{
    public Guid Id { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public LoaiThongBao LoaiThongBao { get; set; }
    public MucDoUuTien MucDoUuTien { get; set; }
    public DateTime ThoiGianTao { get; set; }
    public string? NguoiGui { get; set; }
    public string? NguoiNhan { get; set; }
    public bool DaDoc { get; set; }
    public Dictionary<string, object>? MetaData { get; set; }
}

/// <summary>
/// Thông báo cho phụ huynh
/// </summary>
public class ThongBaoPhuHuynh
{
    public string TieuDe { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public string ConEm { get; set; } = string.Empty; // Tên con em
    public DateTime ThoiGian { get; set; }
    public LoaiSuKien LoaiSuKien { get; set; }
    public MucDoCanhBao MucDo { get; set; }
    public string? HanhDongDeNghi { get; set; }
    public List<string>? HinhAnhDinhKem { get; set; }
}

/// <summary>
/// Lịch sử thông báo
/// </summary>
public class LichSuThongBao
{
    public Guid Id { get; set; }
    public ThongBao ThongBao { get; set; } = new();
    public DateTime ThoiGianGui { get; set; }
    public KenhGui KenhGui { get; set; }
    public TrangThaiGui TrangThai { get; set; }
    public string? LyDoThatBai { get; set; }
    public DateTime? ThoiGianDoc { get; set; }
}

/// <summary>
/// Cấu hình thông báo
/// </summary>
public class CauHinhThongBao
{
    public bool BatThongBaoEmail { get; set; } = true;
    public bool BatThongBaoSystemTray { get; set; } = true;
    public bool BatThongBaoSms { get; set; } = false;
    public bool BatThongBaoPush { get; set; } = false;
    
    // Cấu hình Email
    public string? SmtpServer { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public bool SmtpUseSsl { get; set; } = true;
    public string? EmailGui { get; set; }
    public List<string> EmailNhanMacDinh { get; set; } = new();
    
    // Cấu hình SMS
    public string? SmsApiUrl { get; set; }
    public string? SmsApiKey { get; set; }
    public string? SmsSenderName { get; set; }
    
    // Cấu hình Push
    public string? PushApiUrl { get; set; }
    public string? PushApiKey { get; set; }
    
    // Cấu hình chung
    public int DoTreThongBao { get; set; } = 0; // Milliseconds
    public bool GuiThongBaoNgoaiGio { get; set; } = false;
    public TimeSpan GioBatDau { get; set; } = new(8, 0, 0);
    public TimeSpan GioKetThuc { get; set; } = new(22, 0, 0);
    public bool NhomThongBaoGiongNhau { get; set; } = true;
    public int SoLanThuLai { get; set; } = 3;
    
    // Mức độ thông báo
    public Dictionary<LoaiThongBao, List<KenhGui>> KenhTheoLoai { get; set; } = new();
    public Dictionary<MucDoCanhBao, List<KenhGui>> KenhTheoMucDo { get; set; } = new();
}

/// <summary>
/// Vi phạm cần xử lý
/// </summary>
public class ViPham
{
    public Guid Id { get; set; }
    public DateTime ThoiGian { get; set; }
    public LoaiViPham LoaiViPham { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public string NguoiDung { get; set; } = string.Empty;
    public MucDoCanhBao MucDo { get; set; }
    public HanhDongXuLy HanhDongCanThucHien { get; set; }
    public bool DaXuLy { get; set; }
    public string? KetQuaXuLy { get; set; }
}

/// <summary>
/// Các enum
/// </summary>
public enum LoaiThongBao
{
    ThongTin,
    CanhBao,
    LoiNhac,
    BaoCao,
    ViPham,
    HeThong,
    KhanCap
}

public enum MucDoUuTien
{
    Thap,
    BinhThuong,
    Cao,
    RatCao,
    KhanCap
}

public enum MucDoCanhBao
{
    ThongTin,
    CanhBao,
    NghiemTrong,
    RatNghiemTrong,
    KhanCap
}

public enum LoaiSuKien
{
    TruyCapWebNguyHiem,
    SuDungUngDungCam,
    VuotGioiHanThoiGian,
    SuDungNgoaiGioChoPhep,
    TruyCapNoiDungKhongPhuHop,
    PhatHienMalware,
    ThayDoiHeThong,
    Khac
}

public enum KenhGui
{
    Email,
    SystemTray,
    Sms,
    Push,
    InApp,
    Log
}

public enum TrangThaiGui
{
    DangCho,
    DaGui,
    ThatBai,
    DaDoc,
    DaXuLy
}

public enum LoaiIcon
{
    ThongTin,
    CanhBao,
    Loi,
    ThanhCong
}

public enum HanhDongXuLy
{
    ChiGhiNhan,
    GuiCanhBao,
    GuiEmailPhuHuynh,
    KhoaMayTinh,
    ChanUngDung,
    ChanWebsite,
    GioiHanThoiGian,
    TatMayTinh
}

public enum LoaiViPham
{
    TruyCapWebCam,
    SuDungUngDungCam,
    VuotThoiGian,
    SuDungNgoaiGio,
    NoiDungKhongPhuHop,
    BoQuaCanhBao,
    ThayDoiCauHinh,
    CoGangVuotRao
}