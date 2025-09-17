using System;
using System.Threading.Tasks;

namespace KidGuard.Core.Services
{
    /// <summary>
    /// Interface for notification service
    /// </summary>
    public interface INotificationService
    {
        Task ShowNotificationAsync(string title, string message, NotificationType type);
        Task ShowWarningAsync(string message);
        Task ShowErrorAsync(string message);
        Task ShowSuccessAsync(string message);
        Task ShowInfoAsync(string message);
        Task<bool> ShowConfirmationAsync(string title, string message);
        Task SendEmailNotificationAsync(string to, string subject, string body);
        Task PlaySoundNotificationAsync(NotificationSound sound);
        bool EnableSoundNotifications { get; set; }
        bool EnableDesktopNotifications { get; set; }
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public enum NotificationSound
    {
        Default,
        Warning,
        Error,
        Success,
        Custom
    }
}