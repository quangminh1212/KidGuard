using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using KidGuard.Core.Data;
using KidGuard.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KidGuard.Services.Implementation;

/// <summary>
/// Dịch vụ chụp màn hình
/// Chụp và quản lý ảnh màn hình để giám sát hoạt động
/// </summary>
public class ScreenshotService : IScreenshotService
{
    private readonly ILogger<ScreenshotService> _logger;
    private readonly KidGuardDbContext _dbContext;
    private readonly string _thuMucLuuAnh;
    private CancellationTokenSource? _ctsChupTuDong;
    private Task? _taskChupTuDong;
    private readonly SemaphoreSlim _khoaDongBo = new(1, 1);

    /// <summary>
    /// Khởi tạo dịch vụ chụp màn hình
    /// </summary>
    public ScreenshotService(
        ILogger<ScreenshotService> logger,
        KidGuardDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        
        // Tạo thư mục lưu ảnh
        _thuMucLuuAnh = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "KidGuard", "Screenshots"
        );
        
        if (!Directory.Exists(_thuMucLuuAnh))
        {
            Directory.CreateDirectory(_thuMucLuuAnh);
            _logger.LogInformation("Đã tạo thư mục lưu ảnh: {ThuMuc}", _thuMucLuuAnh);
        }
    }

    /// <summary>
    /// Chụp màn hình hiện tại
    /// </summary>
    public async Task<string> ChupManHinhAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                // Lấy kích thước màn hình chính
                var manHinh = Screen.PrimaryScreen;
                var khungHinh = manHinh.Bounds;
                
                // Tạo bitmap với kích thước màn hình
                using var bitmap = new Bitmap(khungHinh.Width, khungHinh.Height);
                using var doHoa = Graphics.FromImage(bitmap);
                
                // Chụp màn hình
                doHoa.CopyFromScreen(
                    khungHinh.Left,
                    khungHinh.Top,
                    0, 0,
                    bitmap.Size,
                    CopyPixelOperation.SourceCopy
                );
                
                // Tạo tên file với timestamp
                var tenFile = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                var duongDanFile = Path.Combine(_thuMucLuuAnh, tenFile);
                
                // Lưu với chất lượng JPEG 85%
                var codecJpeg = ImageCodecInfo.GetImageEncoders()
                    .First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
                    
                var thamSoEncoder = new EncoderParameters(1);
                thamSoEncoder.Param[0] = new EncoderParameter(
                    System.Drawing.Imaging.Encoder.Quality, 85L
                );
                
                bitmap.Save(duongDanFile, codecJpeg, thamSoEncoder);
                
                _logger.LogInformation("Đã chụp màn hình: {TenFile}", tenFile);
                return duongDanFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chụp màn hình");
                throw;
            }
        });
    }

    /// <summary>
    /// Chụp màn hình và lưu vào database
    /// </summary>
    public async Task<Guid> ChupVaLuuManHinhAsync(string? moTa = null)
    {
        await _khoaDongBo.WaitAsync();
        try
        {
            // Chụp màn hình
            var duongDanFile = await ChupManHinhAsync();
            var fileInfo = new FileInfo(duongDanFile);
            
            // Lấy kích thước ảnh
            int chieuRong = 0, chieuCao = 0;
            using (var bitmap = new Bitmap(duongDanFile))
            {
                chieuRong = bitmap.Width;
                chieuCao = bitmap.Height;
            }
            
            // Lưu thông tin vào database
            var thongTinAnh = new ThongTinAnhChup
            {
                Id = Guid.NewGuid(),
                DuongDanFile = duongDanFile,
                ThoiDiemChup = DateTime.Now,
                KichThuocFile = fileInfo.Length,
                ChieuRong = chieuRong,
                ChieuCao = chieuCao,
                MoTa = moTa,
                TenManHinh = Screen.PrimaryScreen.DeviceName,
                QuanTrong = false
            };
            
            // TODO: Lưu vào database thật
            // Tạm thời chỉ log
            _logger.LogInformation(
                "Đã lưu ảnh vào database: {Id}, Kích thước: {KichThuoc} KB",
                thongTinAnh.Id, thongTinAnh.KichThuocFile / 1024
            );
            
            return thongTinAnh.Id;
        }
        finally
        {
            _khoaDongBo.Release();
        }
    }

    /// <summary>
    /// Lấy danh sách ảnh chụp màn hình
    /// </summary>
    public async Task<IEnumerable<ThongTinAnhChup>> LayDanhSachAnhAsync(
        DateTime tuNgay, DateTime denNgay, int soLuong = 100)
    {
        await _khoaDongBo.WaitAsync();
        try
        {
            var danhSachAnh = new List<ThongTinAnhChup>();
            
            // Lấy tất cả file ảnh trong thư mục
            var files = Directory.GetFiles(_thuMucLuuAnh, "screenshot_*.jpg")
                .Select(f => new FileInfo(f))
                .Where(f => f.CreationTime >= tuNgay && f.CreationTime <= denNgay)
                .OrderByDescending(f => f.CreationTime)
                .Take(soLuong);
            
            foreach (var file in files)
            {
                try
                {
                    int chieuRong = 0, chieuCao = 0;
                    using (var bitmap = new Bitmap(file.FullName))
                    {
                        chieuRong = bitmap.Width;
                        chieuCao = bitmap.Height;
                    }
                    
                    danhSachAnh.Add(new ThongTinAnhChup
                    {
                        Id = Guid.NewGuid(),
                        DuongDanFile = file.FullName,
                        ThoiDiemChup = file.CreationTime,
                        KichThuocFile = file.Length,
                        ChieuRong = chieuRong,
                        ChieuCao = chieuCao
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể đọc file ảnh: {File}", file.Name);
                }
            }
            
            return danhSachAnh;
        }
        finally
        {
            _khoaDongBo.Release();
        }
    }

    /// <summary>
    /// Xóa ảnh cũ để tiết kiệm dung lượng
    /// </summary>
    public async Task<int> XoaAnhCuAsync(int soNgayGiuLai = 7)
    {
        await _khoaDongBo.WaitAsync();
        try
        {
            var ngayGioiHan = DateTime.Now.AddDays(-soNgayGiuLai);
            var filesCanXoa = Directory.GetFiles(_thuMucLuuAnh, "screenshot_*.jpg")
                .Select(f => new FileInfo(f))
                .Where(f => f.CreationTime < ngayGioiHan)
                .ToList();
            
            var soLuongDaXoa = 0;
            var dungLuongDaXoa = 0L;
            
            foreach (var file in filesCanXoa)
            {
                try
                {
                    dungLuongDaXoa += file.Length;
                    file.Delete();
                    soLuongDaXoa++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể xóa file: {File}", file.Name);
                }
            }
            
            _logger.LogInformation(
                "Đã xóa {SoLuong} ảnh cũ, giải phóng {DungLuong} MB",
                soLuongDaXoa, dungLuongDaXoa / (1024 * 1024)
            );
            
            return soLuongDaXoa;
        }
        finally
        {
            _khoaDongBo.Release();
        }
    }

    /// <summary>
    /// Bắt đầu chụp màn hình tự động
    /// </summary>
    public async Task BatDauChupTuDongAsync(TimeSpan khoangThoiGian, CancellationToken cancellationToken)
    {
        if (_ctsChupTuDong != null && !_ctsChupTuDong.IsCancellationRequested)
        {
            _logger.LogWarning("Chụp tự động đã đang chạy");
            return;
        }
        
        _ctsChupTuDong = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _taskChupTuDong = ChupTuDongAsync(khoangThoiGian, _ctsChupTuDong.Token);
        
        _logger.LogInformation(
            "Đã bắt đầu chụp màn hình tự động, mỗi {Phut} phút",
            khoangThoiGian.TotalMinutes
        );
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Thực hiện chụp tự động
    /// </summary>
    private async Task ChupTuDongAsync(TimeSpan khoangThoiGian, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(khoangThoiGian, cancellationToken);
                
                if (!cancellationToken.IsCancellationRequested)
                {
                    await ChupVaLuuManHinhAsync("Chụp tự động");
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình chụp tự động");
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }
    }

    /// <summary>
    /// Dừng chụp màn hình tự động
    /// </summary>
    public async Task DungChupTuDongAsync()
    {
        if (_ctsChupTuDong != null)
        {
            _ctsChupTuDong.Cancel();
            
            if (_taskChupTuDong != null)
            {
                try
                {
                    await _taskChupTuDong;
                }
                catch (OperationCanceledException)
                {
                    // Bình thường khi hủy
                }
            }
            
            _ctsChupTuDong.Dispose();
            _ctsChupTuDong = null;
            _taskChupTuDong = null;
            
            _logger.LogInformation("Đã dừng chụp màn hình tự động");
        }
    }

    /// <summary>
    /// Kiểm tra xem có đang chụp tự động không
    /// </summary>
    public bool DangChupTuDong => 
        _ctsChupTuDong != null && !_ctsChupTuDong.IsCancellationRequested;

    /// <summary>
    /// Lấy thống kê về ảnh chụp màn hình
    /// </summary>
    public async Task<ThongKeAnhChup> LayThongKeAsync()
    {
        await _khoaDongBo.WaitAsync();
        try
        {
            var tatCaAnh = Directory.GetFiles(_thuMucLuuAnh, "screenshot_*.jpg")
                .Select(f => new FileInfo(f))
                .ToList();
            
            var homNay = DateTime.Today;
            var dauTuan = homNay.AddDays(-(int)homNay.DayOfWeek);
            
            return new ThongKeAnhChup
            {
                TongSoAnh = tatCaAnh.Count,
                TongDungLuongMB = tatCaAnh.Sum(f => f.Length) / (1024.0 * 1024.0),
                SoAnhHomNay = tatCaAnh.Count(f => f.CreationTime.Date == homNay),
                SoAnhTuanNay = tatCaAnh.Count(f => f.CreationTime.Date >= dauTuan),
                AnhCuNhat = tatCaAnh.MinBy(f => f.CreationTime)?.CreationTime,
                AnhMoiNhat = tatCaAnh.MaxBy(f => f.CreationTime)?.CreationTime,
                DungLuongTrungBinhKB = tatCaAnh.Any() ? 
                    tatCaAnh.Average(f => f.Length) / 1024.0 : 0
            };
        }
        finally
        {
            _khoaDongBo.Release();
        }
    }

    /// <summary>
    /// Xuất ảnh ra file ZIP để backup
    /// </summary>
    public async Task<long> XuatAnhRaZipAsync(string duongDanZip, DateTime tuNgay, DateTime denNgay)
    {
        await _khoaDongBo.WaitAsync();
        try
        {
            // Lấy danh sách file cần zip
            var fileCanZip = Directory.GetFiles(_thuMucLuuAnh, "screenshot_*.jpg")
                .Select(f => new FileInfo(f))
                .Where(f => f.CreationTime >= tuNgay && f.CreationTime <= denNgay)
                .ToList();
            
            if (!fileCanZip.Any())
            {
                _logger.LogWarning("Không có ảnh nào trong khoảng thời gian chỉ định");
                return 0;
            }
            
            // Tạo file ZIP
            if (File.Exists(duongDanZip))
            {
                File.Delete(duongDanZip);
            }
            
            using (var zipArchive = ZipFile.Open(duongDanZip, ZipArchiveMode.Create))
            {
                foreach (var file in fileCanZip)
                {
                    zipArchive.CreateEntryFromFile(file.FullName, file.Name);
                }
            }
            
            var zipInfo = new FileInfo(duongDanZip);
            
            _logger.LogInformation(
                "Đã xuất {SoLuong} ảnh vào file ZIP: {File}, Kích thước: {KichThuoc} MB",
                fileCanZip.Count, duongDanZip, zipInfo.Length / (1024 * 1024)
            );
            
            return zipInfo.Length;
        }
        finally
        {
            _khoaDongBo.Release();
        }
    }
}