using System;
using System.Threading.Tasks;
using KidGuard.Core.DTOs;
using KidGuard.Core.Models;

namespace KidGuard.Core.Services
{
    /// <summary>
    /// Interface for authentication service
    /// </summary>
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> LoginAsync(string username, string password);
        Task<bool> LogoutAsync();
        Task<User> GetCurrentUserAsync();
        Task<bool> ChangePasswordAsync(string oldPassword, string newPassword);
        Task<bool> ValidateTokenAsync(string token);
        bool IsAuthenticated { get; }
        User CurrentUser { get; }
    }

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public User User { get; set; }
        public string ErrorMessage { get; set; }
    }
}