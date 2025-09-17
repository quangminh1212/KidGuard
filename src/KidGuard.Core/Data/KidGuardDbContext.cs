using Microsoft.EntityFrameworkCore;
using KidGuard.Core.Models;

namespace KidGuard.Core.Data;

/// <summary>
/// Database context cho ứng dụng KidGuard
/// Quản lý kết nối và tương tác với SQLite database
/// </summary>
public class KidGuardDbContext : DbContext
{
    // Bảng lưu trữ websites bị chặn
    public DbSet<BlockedWebsite> BlockedWebsites { get; set; }
    
    // Bảng lưu trữ ứng dụng được giám sát
    public DbSet<MonitoredApplication> MonitoredApplications { get; set; }
    
    // Bảng lưu trữ lịch sử hoạt động
    public DbSet<ActivityLogEntry> ActivityLogs { get; set; }
    
    // Bảng lưu trữ phiên sử dụng ứng dụng
    public DbSet<UsageSession> UsageSessions { get; set; }
    
    // Bảng lưu trữ cấu hình người dùng
    public DbSet<UserSettings> UserSettings { get; set; }
    
    // Bảng lưu trữ lịch trình hạn chế
    public DbSet<ScheduleRule> ScheduleRules { get; set; }
    
    // Bảng lưu trữ thông báo
    public DbSet<NotificationLog> NotificationLogs { get; set; }

    /// <summary>
    /// Constructor với options từ DI container
    /// </summary>
    public KidGuardDbContext(DbContextOptions<KidGuardDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Constructor mặc định cho design-time
    /// </summary>
    public KidGuardDbContext()
    {
    }

    /// <summary>
    /// Cấu hình database connection
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KidGuard", "Data", "kidguard.db"
            );
            
            // Đảm bảo thư mục tồn tại
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    /// <summary>
    /// Cấu hình các entity và relationships
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình BlockedWebsite
        modelBuilder.Entity<BlockedWebsite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Domain).IsUnique();
            entity.Property(e => e.Domain).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Reason).HasMaxLength(500);
        });

        // Cấu hình MonitoredApplication
        modelBuilder.Entity<MonitoredApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProcessName).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ProcessName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ExecutablePath).HasMaxLength(500);
        });

        // Cấu hình ActivityLogEntry
        modelBuilder.Entity<ActivityLogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Type);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Details).HasMaxLength(1000);
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.ProcessName).HasMaxLength(255);
            entity.Property(e => e.WebsiteUrl).HasMaxLength(500);
        });

        // Cấu hình UsageSession
        modelBuilder.Entity<UsageSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StartTime);
            entity.Property(e => e.ProcessName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.TerminationReason).HasMaxLength(500);
        });

        // Cấu hình UserSettings
        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
        });

        // Cấu hình ScheduleRule
        modelBuilder.Entity<ScheduleRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Cấu hình NotificationLog
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SentAt);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Recipient).HasMaxLength(255);
            entity.Property(e => e.Subject).HasMaxLength(255);
            entity.Property(e => e.Message).IsRequired();
        });

        // Seed data mặc định
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Thêm dữ liệu mặc định
    /// </summary>
    private void SeedData(ModelBuilder modelBuilder)
    {
        // Thêm user settings mặc định
        modelBuilder.Entity<UserSettings>().HasData(
            new UserSettings
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                PasswordHash = "admin123", // TODO: Hash this properly
                IsFirstRun = true,
                DailyScreenTimeLimit = TimeSpan.FromHours(8),
                BlockAdultContent = true,
                EnableMonitoring = true,
                EnableNotifications = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Thêm một số website nguy hiểm mặc định
        var dangerousSites = new[]
        {
            "pornhub.com", "xvideos.com", "xnxx.com", "redtube.com",
            "youporn.com", "xhamster.com", "livejasmin.com"
        };

        var blockedSites = dangerousSites.Select((site, index) => new BlockedWebsite
        {
            Id = Guid.Parse($"00000000-0000-0000-0000-{(index + 1):D12}"),
            Domain = site,
            Category = "Adult",
            BlockedAt = DateTime.UtcNow,
            IsActive = true,
            Reason = "Nội dung người lớn - tự động chặn"
        }).ToArray();

        modelBuilder.Entity<BlockedWebsite>().HasData(blockedSites);
    }
}

/// <summary>
/// Model cho cấu hình người dùng
/// </summary>
public class UserSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PasswordHash { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsFirstRun { get; set; } = true;
    public TimeSpan DailyScreenTimeLimit { get; set; } = TimeSpan.FromHours(8);
    public bool BlockAdultContent { get; set; } = true;
    public bool BlockSocialMedia { get; set; } = false;
    public bool BlockGaming { get; set; } = false;
    public bool EnableMonitoring { get; set; } = true;
    public bool EnableNotifications { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Model cho lịch trình hạn chế
/// </summary>
public class ScheduleRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DayOfWeek[]? DaysOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAllowed { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Model cho log thông báo
/// </summary>
public class NotificationLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty; // Email, SMS, Telegram
    public string? Recipient { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSent { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
}