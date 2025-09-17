using System;

namespace KidGuard.Core.Interfaces;

/// <summary>
/// Thông tin chi tiết về một ảnh chụp màn hình
/// </summary>
public class ThongTinAnhChup
{
    /// <summary>
    /// ID định danh duy nhất của ảnh
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Đường dẫn đầy đủ tới file ảnh
    /// </summary>
    public string DuongDanFile { get; set; } = string.Empty;
    
    /// <summary>
    /// Thời điểm chụp ảnh
    /// </summary>
    public DateTime ThoiDiemChup { get; set; }
    
    /// <summary>
    /// Kích thước file tính bằng byte
    /// </summary>
    public long KichThuocFile { get; set; }
    
    /// <summary>
    /// Chiều rộng ảnh (pixel)
    /// </summary>
    public int ChieuRong { get; set; }
    
    /// <summary>
    /// Chiều cao ảnh (pixel)
    /// </summary>
    public int ChieuCao { get; set; }
    
    /// <summary>
    /// Mô tả hoặc ghi chú về ảnh
    /// </summary>
    public string? MoTa { get; set; }
    
    /// <summary>
    /// Tên màn hình được chụp
    /// </summary>
    public string? TenManHinh { get; set; }
    
    /// <summary>
    /// Đánh dấu ảnh quan trọng cần giữ lại
    /// </summary>
    public bool QuanTrong { get; set; }
    
    /// <summary>
    /// Lấy kích thước file theo KB
    /// </summary>
    public double KichThuocKB => KichThuocFile / 1024.0;
    
    /// <summary>
    /// Lấy kích thước file theo MB
    /// </summary>
    public double KichThuocMB => KichThuocFile / (1024.0 * 1024.0);
    
    /// <summary>
    /// Lấy tên file từ đường dẫn
    /// </summary>
    public string TenFile => System.IO.Path.GetFileName(DuongDanFile);
    
    /// <summary>
    /// Lấy độ phân giải dạng chuỗi
    /// </summary>
    public string DoPhanGiai => $"{ChieuRong} x {ChieuCao}";
}

/// <summary>
/// Thống kê tổng quan về ảnh chụp màn hình
/// </summary>
public class ThongKeAnhChup
{
    /// <summary>
    /// Tổng số ảnh đã chụp
    /// </summary>
    public int TongSoAnh { get; set; }
    
    /// <summary>
    /// Tổng dung lượng tất cả ảnh (MB)
    /// </summary>
    public double TongDungLuongMB { get; set; }
    
    /// <summary>
    /// Số ảnh chụp trong ngày hôm nay
    /// </summary>
    public int SoAnhHomNay { get; set; }
    
    /// <summary>
    /// Số ảnh chụp trong tuần này
    /// </summary>
    public int SoAnhTuanNay { get; set; }
    
    /// <summary>
    /// Thời điểm chụp ảnh cũ nhất
    /// </summary>
    public DateTime? AnhCuNhat { get; set; }
    
    /// <summary>
    /// Thời điểm chụp ảnh mới nhất
    /// </summary>
    public DateTime? AnhMoiNhat { get; set; }
    
    /// <summary>
    /// Dung lượng trung bình mỗi ảnh (KB)
    /// </summary>
    public double DungLuongTrungBinhKB { get; set; }
    
    /// <summary>
    /// Kiểm tra có ảnh nào không
    /// </summary>
    public bool CoAnh => TongSoAnh > 0;
    
    /// <summary>
    /// Tính số ngày đã chụp
    /// </summary>
    public int SoNgayDaChup => 
        (AnhCuNhat.HasValue && AnhMoiNhat.HasValue) 
            ? (int)(AnhMoiNhat.Value - AnhCuNhat.Value).TotalDays + 1
            : 0;
    
    /// <summary>
    /// Trung bình ảnh mỗi ngày
    /// </summary>
    public double TrungBinhAnhMoiNgay => 
        SoNgayDaChup > 0 ? (double)TongSoAnh / SoNgayDaChup : 0;
}

/// <summary>
/// Cấu hình cho chức năng chụp màn hình
/// </summary>
public class CauHinhChupManHinh
{
    /// <summary>
    /// Cho phép chụp màn hình tự động
    /// </summary>
    public bool ChupTuDong { get; set; } = true;
    
    /// <summary>
    /// Khoảng thời gian giữa các lần chụp (phút)
    /// </summary>
    public int KhoangThoiGianPhut { get; set; } = 5;
    
    /// <summary>
    /// Chất lượng ảnh JPEG (0-100)
    /// </summary>
    public int ChatLuongJpeg { get; set; } = 85;
    
    /// <summary>
    /// Số ngày giữ lại ảnh trước khi tự động xóa
    /// </summary>
    public int SoNgayGiuAnh { get; set; } = 7;
    
    /// <summary>
    /// Dung lượng tối đa cho phép (MB)
    /// </summary>
    public int DungLuongToiDaMB { get; set; } = 1000;
    
    /// <summary>
    /// Chụp khi phát hiện hoạt động đáng ngờ
    /// </summary>
    public bool ChupKhiPhatHienDangNgo { get; set; } = true;
    
    /// <summary>
    /// Làm mờ thông tin nhạy cảm trong ảnh
    /// </summary>
    public bool LamMoThongTinNhayCam { get; set; } = false;
    
    /// <summary>
    /// Thư mục lưu ảnh tùy chỉnh
    /// </summary>
    public string? ThuMucLuuAnh { get; set; }
    
    /// <summary>
    /// Chỉ chụp màn hình chính
    /// </summary>
    public bool ChiChupManHinhChinh { get; set; } = true;
    
    /// <summary>
    /// Tạo thumbnail cho ảnh
    /// </summary>
    public bool TaoThumbnail { get; set; } = true;
    
    /// <summary>
    /// Kích thước thumbnail (pixel)
    /// </summary>
    public int KichThuocThumbnail { get; set; } = 320;
    
    /// <summary>
    /// Kiểm tra cấu hình hợp lệ
    /// </summary>
    public bool HopLe =>
        KhoangThoiGianPhut >= 1 &&
        ChatLuongJpeg >= 10 && ChatLuongJpeg <= 100 &&
        SoNgayGiuAnh >= 1 &&
        DungLuongToiDaMB >= 100;
}

/// <summary>
/// Kết quả phân tích ảnh chụp màn hình
/// </summary>
public class KetQuaPhanTichAnh
{
    /// <summary>
    /// ID ảnh được phân tích
    /// </summary>
    public Guid IdAnh { get; set; }
    
    /// <summary>
    /// Thời điểm phân tích
    /// </summary>
    public DateTime ThoiDiemPhanTich { get; set; }
    
    /// <summary>
    /// Có phát hiện nội dung không phù hợp
    /// </summary>
    public bool CoNoiDungKhongPhuHop { get; set; }
    
    /// <summary>
    /// Danh sách ứng dụng được phát hiện
    /// </summary>
    public List<string> UngDungPhatHien { get; set; } = new();
    
    /// <summary>
    /// Danh sách website được phát hiện
    /// </summary>
    public List<string> WebsitePhatHien { get; set; } = new();
    
    /// <summary>
    /// Text được trích xuất từ ảnh (OCR)
    /// </summary>
    public string? TextTrichXuat { get; set; }
    
    /// <summary>
    /// Độ tin cậy của phân tích (0-100)
    /// </summary>
    public double DoTinCay { get; set; }
    
    /// <summary>
    /// Ghi chú từ phân tích
    /// </summary>
    public string? GhiChu { get; set; }
    
    /// <summary>
    /// Cần xem xét thủ công
    /// </summary>
    public bool CanXemXet => CoNoiDungKhongPhuHop && DoTinCay < 80;
}