using System.Security.Cryptography;
using System.Text;
using KidGuard.Core.Data;
using KidGuard.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KidGuard.Services.Implementation;

/// <summary>
/// Service xử lý xác thực và bảo mật
/// Quản lý đăng nhập, mật khẩu và session
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly KidGuardDbContext _dbContext;
    private DateTime _sessionExpiry;
    private bool _isAuthenticated;
    private readonly TimeSpan _sessionDuration = TimeSpan.FromMinutes(30);
    private string? _resetCode;
    private DateTime _resetCodeExpiry;

    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        KidGuardDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        _isAuthenticated = false;
    }

    /// <summary>
    /// Đăng nhập với mật khẩu
    /// </summary>
    public async Task<bool> LoginAsync(string password)
    {
        try
        {
            var settings = await _dbContext.UserSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                _logger.LogWarning("Chưa có cấu hình người dùng");
                return false;
            }

            if (VerifyPassword(password, settings.PasswordHash))
            {
                _isAuthenticated = true;
                _sessionExpiry = DateTime.Now.Add(_sessionDuration);
                _logger.LogInformation("Đăng nhập thành công");
                return true;
            }

            _logger.LogWarning("Mật khẩu không đúng");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng nhập");
            return false;
        }
    }

    /// <summary>
    /// Thay đổi mật khẩu
    /// </summary>
    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            if (!_isAuthenticated)
            {
                _logger.LogWarning("Chưa đăng nhập");
                return false;
            }

            var settings = await _dbContext.UserSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                return false;
            }

            if (!VerifyPassword(currentPassword, settings.PasswordHash))
            {
                _logger.LogWarning("Mật khẩu hiện tại không đúng");
                return false;
            }

            settings.PasswordHash = HashPassword(newPassword);
            settings.UpdatedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Đã thay đổi mật khẩu thành công");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thay đổi mật khẩu");
            return false;
        }
    }

    /// <summary>
    /// Kiểm tra lần chạy đầu tiên
    /// </summary>
    public async Task<bool> IsFirstRunAsync()
    {
        try
        {
            var settings = await _dbContext.UserSettings.FirstOrDefaultAsync();
            return settings == null || settings.IsFirstRun;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi kiểm tra first run");
            return true;
        }
    }

    /// <summary>
    /// Thiết lập mật khẩu ban đầu
    /// </summary>
    public async Task<bool> SetupInitialPasswordAsync(string password, string? email = null)
    {
        try
        {
            var settings = await _dbContext.UserSettings.FirstOrDefaultAsync();
            
            if (settings == null)
            {
                settings = new UserSettings
                {
                    PasswordHash = HashPassword(password),
                    Email = email,
                    IsFirstRun = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.UserSettings.Add(settings);
            }
            else
            {
                settings.PasswordHash = HashPassword(password);
                settings.Email = email ?? settings.Email;
                settings.IsFirstRun = false;
                settings.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            
            _isAuthenticated = true;
            _sessionExpiry = DateTime.Now.Add(_sessionDuration);
            
            _logger.LogInformation("Đã thiết lập mật khẩu ban đầu");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thiết lập mật khẩu ban đầu");
            return false;
        }
    }

    /// <summary>
    /// Reset mật khẩu với mã xác nhận
    /// </summary>
    public async Task<bool> ResetPasswordAsync(string resetCode, string newPassword)
    {
        try
        {
            if (string.IsNullOrEmpty(_resetCode) || 
                _resetCode != resetCode ||
                DateTime.Now > _resetCodeExpiry)
            {
                _logger.LogWarning("Mã reset không hợp lệ hoặc đã hết hạn");
                return false;
            }

            var settings = await _dbContext.UserSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                return false;
            }

            settings.PasswordHash = HashPassword(newPassword);
            settings.UpdatedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            
            _resetCode = null;
            _logger.LogInformation("Đã reset mật khẩu thành công");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi reset mật khẩu");
            return false;
        }
    }

    /// <summary>
    /// Gửi mã reset mật khẩu
    /// </summary>
    public async Task<bool> SendPasswordResetCodeAsync(string email)
    {
        try
        {
            var settings = await _dbContext.UserSettings.FirstOrDefaultAsync();
            if (settings == null || settings.Email != email)
            {
                _logger.LogWarning("Email không khớp");
                return false;
            }

            // Tạo mã reset 6 số
            _resetCode = new Random().Next(100000, 999999).ToString();
            _resetCodeExpiry = DateTime.Now.AddMinutes(15);

            // TODO: Implement email sending
            // Tạm thời log ra console
            _logger.LogWarning($"Reset code: {_resetCode} (expires in 15 minutes)");

            // Lưu log
            var notification = new NotificationLog
            {
                Type = "Email",
                Recipient = email,
                Subject = "KidGuard - Mã khôi phục mật khẩu",
                Message = $"Mã khôi phục của bạn là: {_resetCode}\nMã này sẽ hết hạn sau 15 phút.",
                IsSent = false, // TODO: Change to true when email is actually sent
                SentAt = DateTime.UtcNow
            };
            
            _dbContext.NotificationLogs.Add(notification);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi mã reset");
            return false;
        }
    }

    /// <summary>
    /// Kiểm tra trạng thái đăng nhập
    /// </summary>
    public bool IsAuthenticated
    {
        get
        {
            if (_isAuthenticated && DateTime.Now > _sessionExpiry)
            {
                _isAuthenticated = false;
                _logger.LogInformation("Session đã hết hạn");
            }
            return _isAuthenticated;
        }
    }

    /// <summary>
    /// Đăng xuất
    /// </summary>
    public void Logout()
    {
        _isAuthenticated = false;
        _sessionExpiry = DateTime.Now;
        _logger.LogInformation("Đã đăng xuất");
    }

    /// <summary>
    /// Lấy thời gian còn lại của session
    /// </summary>
    public TimeSpan GetSessionTimeRemaining()
    {
        if (!_isAuthenticated)
            return TimeSpan.Zero;

        var remaining = _sessionExpiry - DateTime.Now;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Gia hạn session
    /// </summary>
    public void ExtendSession()
    {
        if (_isAuthenticated)
        {
            _sessionExpiry = DateTime.Now.Add(_sessionDuration);
            _logger.LogDebug("Đã gia hạn session");
        }
    }

    /// <summary>
    /// Hash mật khẩu với SHA256
    /// </summary>
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "KidGuard2024"));
        return Convert.ToBase64String(hashedBytes);
    }

    /// <summary>
    /// Xác thực mật khẩu
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        var passwordHash = HashPassword(password);
        return passwordHash == hash;
    }
}