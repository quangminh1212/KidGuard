using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using KidGuard.Core.Models;
using KidGuard.Core.Services;
using KidGuard.Infrastructure.Data;

namespace KidGuard.Infrastructure.Services
{
    /// <summary>
    /// Implementation of authentication service
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthenticationService> _logger;
        private User _currentUser;

        public AuthenticationService(AppDbContext context, ILogger<AuthenticationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool IsAuthenticated => _currentUser != null;
        public User CurrentUser => _currentUser;

        public async Task<AuthenticationResult> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

                if (user == null)
                {
                    return new AuthenticationResult 
                    { 
                        Success = false, 
                        ErrorMessage = "Invalid username or password" 
                    };
                }

                var passwordHash = HashPassword(password);
                if (user.PasswordHash != passwordHash)
                {
                    return new AuthenticationResult 
                    { 
                        Success = false, 
                        ErrorMessage = "Invalid username or password" 
                    };
                }

                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _currentUser = user;
                var token = GenerateToken(user);

                _logger.LogInformation($"User {username} logged in successfully");

                return new AuthenticationResult 
                { 
                    Success = true, 
                    User = user, 
                    Token = token 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return new AuthenticationResult 
                { 
                    Success = false, 
                    ErrorMessage = "An error occurred during login" 
                };
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                _currentUser = null;
                _logger.LogInformation("User logged out successfully");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return false;
            }
        }

        public async Task<User> GetCurrentUserAsync()
        {
            return await Task.FromResult(_currentUser);
        }

        public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword)
        {
            try
            {
                if (_currentUser == null)
                    return false;

                var oldPasswordHash = HashPassword(oldPassword);
                if (_currentUser.PasswordHash != oldPasswordHash)
                    return false;

                _currentUser.PasswordHash = HashPassword(newPassword);
                _context.Users.Update(_currentUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password changed for user {_currentUser.Username}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return false;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            // Simple token validation - in production use JWT
            return await Task.FromResult(!string.IsNullOrEmpty(token) && token.Length > 32);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private string GenerateToken(User user)
        {
            // Simple token generation - in production use JWT
            var tokenData = $"{user.Id}:{user.Username}:{DateTime.UtcNow.Ticks}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenData));
        }
    }
}