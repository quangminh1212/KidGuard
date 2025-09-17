namespace KidGuard.Core.Interfaces;

/// <summary>
/// Interface cho dịch vụ xác thực và bảo mật
/// Quản lý đăng nhập, mật khẩu và phân quyền
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Đăng nhập với mật khẩu
    /// </summary>
    Task<bool> LoginAsync(string password);
    
    /// <summary>
    /// Thay đổi mật khẩu
    /// </summary>
    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
    
    /// <summary>
    /// Kiểm tra xem có phải lần chạy đầu tiên không
    /// </summary>
    Task<bool> IsFirstRunAsync();
    
    /// <summary>
    /// Thiết lập mật khẩu lần đầu
    /// </summary>
    Task<bool> SetupInitialPasswordAsync(string password, string email = null);
    
    /// <summary>
    /// Reset mật khẩu với mã xác nhận
    /// </summary>
    Task<bool> ResetPasswordAsync(string resetCode, string newPassword);
    
    /// <summary>
    /// Gửi mã reset mật khẩu qua email
    /// </summary>
    Task<bool> SendPasswordResetCodeAsync(string email);
    
    /// <summary>
    /// Kiểm tra session hiện tại có hợp lệ không
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Đăng xuất
    /// </summary>
    void Logout();
    
    /// <summary>
    /// Lấy thời gian còn lại của session
    /// </summary>
    TimeSpan GetSessionTimeRemaining();
    
    /// <summary>
    /// Gia hạn session
    /// </summary>
    void ExtendSession();
    
    /// <summary>
    /// Hash mật khẩu
    /// </summary>
    string HashPassword(string password);
    
    /// <summary>
    /// Xác thực hash mật khẩu
    /// </summary>
    bool VerifyPassword(string password, string hash);
}