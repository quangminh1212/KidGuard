using System;
using System.ComponentModel.DataAnnotations;

namespace KidGuard.Core.Models
{
    /// <summary>
    /// User entity model
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string FullName { get; set; }

        public UserRole Role { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public string ProfilePicture { get; set; }

        public string PreferencesJson { get; set; }
    }

    public enum UserRole
    {
        Child,
        Parent,
        Administrator
    }
}