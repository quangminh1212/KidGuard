namespace KidGuard.Core.Interfaces;

/// <summary>
/// Giao diện dịch vụ chụp màn hình
/// Chụp và lưu trữ ảnh màn hình để phụ huynh xem lại
/// </summary>
public interface IScreenshotService
{
    /// <summary>
    /// Chụp màn hình hiện tại
    /// </summary>
    /// <returns>Đường dẫn file ảnh đã lưu</returns>
    Task<string> ChupManHinhAsync();
    
    /// <summary>
    /// Chụp màn hình và lưu vào database
    /// </summary>
    /// <param name="moTa">Mô tả hoặc ghi chú về ảnh</param>
    /// <returns>ID của ảnh trong database</returns>
    Task<Guid> ChupVaLuuManHinhAsync(string? moTa = null);
    
    /// <summary>
    /// Lấy danh sách ảnh chụp màn hình
    /// </summary>
    /// <param name="tuNgay">Từ ngày</param>
    /// <param name="denNgay">Đến ngày</param>
    /// <param name="soLuong">Số lượng ảnh tối đa</param>
    /// <returns>Danh sách thông tin ảnh</returns>
    Task<IEnumerable<ThongTinAnhChup>> LayDanhSachAnhAsync(
        DateTime tuNgay, 
        DateTime denNgay, 
        int soLuong = 100);
    
    /// <summary>
    /// Xóa ảnh cũ để tiết kiệm dung lượng
    /// </summary>
    /// <param name="soNgayGiuLai">Số ngày giữ lại ảnh (mặc định 7)</param>
    /// <returns>Số lượng ảnh đã xóa</returns>
    Task<int> XoaAnhCuAsync(int soNgayGiuLai = 7);
    
    /// <summary>
    /// Bắt đầu chụp màn hình tự động
    /// </summary>
    /// <param name="khoangThoiGian">Khoảng thời gian giữa các lần chụp</param>
    /// <param name="cancellationToken">Token để hủy tác vụ</param>
    Task BatDauChupTuDongAsync(TimeSpan khoangThoiGian, CancellationToken cancellationToken);
    
    /// <summary>
    /// Dừng chụp màn hình tự động
    /// </summary>
    Task DungChupTuDongAsync();
    
    /// <summary>
    /// Kiểm tra xem có đang chụp tự động không
    /// </summary>
    bool DangChupTuDong { get; }
    
    /// <summary>
    /// Lấy thống kê về ảnh chụp màn hình
    /// </summary>
    /// <returns>Thống kê ảnh</returns>
    Task<ThongKeAnhChup> LayThongKeAsync();
    
    /// <summary>
    /// Xuất ảnh ra file ZIP để backup
    /// </summary>
    /// <param name="duongDanZip">Đường dẫn file ZIP đầu ra</param>
    /// <param name="tuNgay">Từ ngày</param>
    /// <param name="denNgay">Đến ngày</param>
    /// <returns>Kích thước file ZIP (bytes)</returns>
    Task<long> XuatAnhRaZipAsync(string duongDanZip, DateTime tuNgay, DateTime denNgay);
}

/// <summary>
/// Thông tin ảnh chụp màn hình
/// </summary>
public class ThongTinAnhChup
{
    /// <summary>
    /// ID ảnh trong database
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Đường dẫn file ảnh
    /// </summary>
    public string DuongDanFile { get; set; } = string.Empty;
    
    /// <summary>
    /// Thời điểm chụp
    /// </summary>
    public DateTime ThoiDiemChup { get; set; }
    
    /// <summary>
    /// Kích thước file (bytes)
    /// </summary>
    public long KichThuocFile { get; set; }
    
    /// <summary>
    /// Chiều rộng ảnh (pixels)
    /// </summary>
    public int ChieuRong { get; set; }
    
    /// <summary>
    /// Chiều cao ảnh (pixels)
    /// </summary>
    public int ChieuCao { get; set; }
    
    /// <summary>
    /// Mô tả hoặc ghi chú
    /// </summary>
    public string? MoTa { get; set; }
    
    /// <summary>
    /// Tên màn hình (nếu có nhiều màn hình)
    /// </summary>
    public string? TenManHinh { get; set; }
    
    /// <summary>
    /// Có phải ảnh quan trọng cần giữ lại không
    /// </summary>
    public bool QuanTrong { get; set; }
}

/// <summary>
/// Thống kê ảnh chụp màn hình
/// </summary>
public class ThongKeAnhChup
{
    /// <summary>
    /// Tổng số ảnh
    /// </summary>
    public int TongSoAnh { get; set; }
    
    /// <summary>
    /// Tổng dung lượng (MB)
    /// </summary>
    public double TongDungLuongMB { get; set; }
    
    /// <summary>
    /// Số ảnh hôm nay
    /// </summary>
    public int SoAnhHomNay { get; set; }
    
    /// <summary>
    /// Số ảnh tuần này
    /// </summary>
    public int SoAnhTuanNay { get; set; }
    
    /// <summary>
    /// Ảnh cũ nhất
    /// </summary>
    public DateTime? AnhCuNhat { get; set; }
    
    /// <summary>
    /// Ảnh mới nhất
    /// </summary>
    public DateTime? AnhMoiNhat { get; set; }
    
    /// <summary>
    /// Dung lượng trung bình mỗi ảnh (KB)
    /// </summary>
    public double DungLuongTrungBinhKB { get; set; }
}