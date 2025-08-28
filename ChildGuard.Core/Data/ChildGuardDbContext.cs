using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using ChildGuard.Core.Models;

namespace ChildGuard.Core.Data
{
    /// <summary>
    /// Database context cho ChildGuard sử dụng SQLite
    /// </summary>
    public class ChildGuardDbContext : DbContext
    {
        private readonly string _dbPath;
        
        public DbSet<EventLogEntity> EventLogs { get; set; }
        public DbSet<AttachmentEntity> Attachments { get; set; }
        public DbSet<ThreatEntity> Threats { get; set; }
        public DbSet<ProcessBlockEntity> ProcessBlocks { get; set; }
        public DbSet<KeystrokeLogEntity> KeystrokeLogs { get; set; }
        public DbSet<MouseActivityEntity> MouseActivities { get; set; }
        public DbSet<ApplicationUsageEntity> ApplicationUsages { get; set; }
        public DbSet<WebsiteVisitEntity> WebsiteVisits { get; set; }
        public DbSet<AlertEntity> Alerts { get; set; }
        public DbSet<SettingsEntity> Settings { get; set; }
        
        public ChildGuardDbContext()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbFolder = Path.Combine(appDataPath, "ChildGuard", "Database");
            
            if (!Directory.Exists(dbFolder))
                Directory.CreateDirectory(dbFolder);
            
            _dbPath = Path.Combine(dbFolder, "childguard.db");
        }
        
        public ChildGuardDbContext(string dbPath)
        {
            _dbPath = dbPath;
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_dbPath}");
            optionsBuilder.EnableSensitiveDataLogging(false);
            optionsBuilder.EnableServiceProviderCaching();
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // EventLog configuration
            modelBuilder.Entity<EventLogEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Content);
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedAt).IsRequired();
                
                entity.HasIndex(e => e.EventType);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Severity);
                
                // Relationship với Attachments
                entity.HasMany(e => e.Attachments)
                      .WithOne(a => a.EventLog)
                      .HasForeignKey(a => a.EventLogId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Attachment configuration
            modelBuilder.Entity<AttachmentEntity>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.FileName).IsRequired().HasMaxLength(255);
                entity.Property(a => a.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(a => a.FileType).IsRequired().HasMaxLength(50);
                entity.Property(a => a.FileSize).IsRequired();
                entity.Property(a => a.Hash).HasMaxLength(64); // SHA256 hash
                entity.Property(a => a.CreatedAt).IsRequired();
                
                entity.HasIndex(a => a.FileType);
                entity.HasIndex(a => a.CreatedAt);
                entity.HasIndex(a => a.Hash);
            });
            
            // Threat configuration
            modelBuilder.Entity<ThreatEntity>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.ThreatType).IsRequired().HasMaxLength(50);
                entity.Property(t => t.ThreatLevel).IsRequired();
                entity.Property(t => t.Description).HasMaxLength(500);
                entity.Property(t => t.Source).HasMaxLength(255);
                entity.Property(t => t.Content).HasMaxLength(1000);
                entity.Property(t => t.ActionTaken).HasMaxLength(100);
                entity.Property(t => t.DetectedAt).IsRequired();
                entity.Property(t => t.IsResolved).IsRequired();
                
                entity.HasIndex(t => t.ThreatType);
                entity.HasIndex(t => t.DetectedAt);
                entity.HasIndex(t => t.ThreatLevel);
                entity.HasIndex(t => t.IsResolved);
            });
            
            // ProcessBlock configuration
            modelBuilder.Entity<ProcessBlockEntity>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.ProcessName).IsRequired().HasMaxLength(255);
                entity.Property(p => p.ProcessPath).HasMaxLength(500);
                entity.Property(p => p.Reason).HasMaxLength(500);
                entity.Property(p => p.BlockedAt).IsRequired();
                
                entity.HasIndex(p => p.ProcessName);
                entity.HasIndex(p => p.BlockedAt);
            });
            
            // KeystrokeLog configuration
            modelBuilder.Entity<KeystrokeLogEntity>(entity =>
            {
                entity.HasKey(k => k.Id);
                entity.Property(k => k.WindowTitle).HasMaxLength(255);
                entity.Property(k => k.ProcessName).HasMaxLength(255);
                entity.Property(k => k.Text).IsRequired();
                entity.Property(k => k.KeyCount).IsRequired();
                entity.Property(k => k.CapturedAt).IsRequired();
                entity.Property(k => k.HasThreat).IsRequired();
                
                entity.HasIndex(k => k.CapturedAt);
                entity.HasIndex(k => k.HasThreat);
                entity.HasIndex(k => k.ProcessName);
            });
            
            // MouseActivity configuration
            modelBuilder.Entity<MouseActivityEntity>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Action).IsRequired().HasMaxLength(50);
                entity.Property(m => m.Button).HasMaxLength(20);
                entity.Property(m => m.X).IsRequired();
                entity.Property(m => m.Y).IsRequired();
                entity.Property(m => m.WindowTitle).HasMaxLength(255);
                entity.Property(m => m.ProcessName).HasMaxLength(255);
                entity.Property(m => m.CapturedAt).IsRequired();
                
                entity.HasIndex(m => m.CapturedAt);
                entity.HasIndex(m => m.Action);
            });
            
            // ApplicationUsage configuration
            modelBuilder.Entity<ApplicationUsageEntity>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.ApplicationName).IsRequired().HasMaxLength(255);
                entity.Property(a => a.ProcessName).IsRequired().HasMaxLength(255);
                entity.Property(a => a.WindowTitle).HasMaxLength(500);
                entity.Property(a => a.StartTime).IsRequired();
                entity.Property(a => a.EndTime);
                entity.Property(a => a.Duration);
                entity.Property(a => a.IsActive).IsRequired();
                
                entity.HasIndex(a => a.ApplicationName);
                entity.HasIndex(a => a.StartTime);
                entity.HasIndex(a => a.IsActive);
            });
            
            // WebsiteVisit configuration
            modelBuilder.Entity<WebsiteVisitEntity>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.Url).IsRequired().HasMaxLength(1000);
                entity.Property(w => w.Domain).IsRequired().HasMaxLength(255);
                entity.Property(w => w.PageTitle).HasMaxLength(500);
                entity.Property(w => w.Browser).HasMaxLength(100);
                entity.Property(w => w.VisitedAt).IsRequired();
                entity.Property(w => w.Duration);
                entity.Property(w => w.IsSafe).IsRequired();
                entity.Property(w => w.ThreatReason).HasMaxLength(500);
                
                entity.HasIndex(w => w.Domain);
                entity.HasIndex(w => w.VisitedAt);
                entity.HasIndex(w => w.IsSafe);
            });
            
            // Alert configuration
            modelBuilder.Entity<AlertEntity>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.AlertType).IsRequired().HasMaxLength(50);
                entity.Property(a => a.Title).IsRequired().HasMaxLength(255);
                entity.Property(a => a.Message).IsRequired().HasMaxLength(1000);
                entity.Property(a => a.Severity).IsRequired().HasMaxLength(20);
                entity.Property(a => a.CreatedAt).IsRequired();
                entity.Property(a => a.IsRead).IsRequired();
                entity.Property(a => a.IsResolved).IsRequired();
                
                entity.HasIndex(a => a.AlertType);
                entity.HasIndex(a => a.CreatedAt);
                entity.HasIndex(a => a.IsRead);
                entity.HasIndex(a => a.IsResolved);
            });
            
            // Settings configuration
            modelBuilder.Entity<SettingsEntity>(entity =>
            {
                entity.HasKey(s => s.Key);
                entity.Property(s => s.Key).HasMaxLength(100);
                entity.Property(s => s.Value).IsRequired();
                entity.Property(s => s.Category).HasMaxLength(50);
                entity.Property(s => s.Description).HasMaxLength(500);
                entity.Property(s => s.UpdatedAt).IsRequired();
                
                entity.HasIndex(s => s.Category);
            });
            
            // Seed default settings
            modelBuilder.Entity<SettingsEntity>().HasData(
                new SettingsEntity { Key = "EnableMonitoring", Value = "true", Category = "General", Description = "Enable monitoring", UpdatedAt = DateTime.UtcNow },
                new SettingsEntity { Key = "EnableScreenshots", Value = "true", Category = "Screenshots", Description = "Enable screenshot capture", UpdatedAt = DateTime.UtcNow },
                new SettingsEntity { Key = "ScreenshotInterval", Value = "300", Category = "Screenshots", Description = "Screenshot interval in seconds", UpdatedAt = DateTime.UtcNow },
                new SettingsEntity { Key = "EnableAudioRecording", Value = "false", Category = "Audio", Description = "Enable audio recording", UpdatedAt = DateTime.UtcNow },
                new SettingsEntity { Key = "EnableKeyLogging", Value = "true", Category = "Monitoring", Description = "Enable keystroke logging", UpdatedAt = DateTime.UtcNow },
                new SettingsEntity { Key = "EnableUrlChecking", Value = "true", Category = "Web", Description = "Enable URL safety checking", UpdatedAt = DateTime.UtcNow },
                new SettingsEntity { Key = "RetentionDays", Value = "30", Category = "Storage", Description = "Data retention period in days", UpdatedAt = DateTime.UtcNow }
            );
        }
        
        public async Task InitializeDatabaseAsync()
        {
            await Database.EnsureCreatedAsync();
            await Database.MigrateAsync();
        }
    }
    
    // Entity Models
    public class EventLogEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EventType { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Content { get; set; }
        public string Severity { get; set; } = "Info";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<AttachmentEntity> Attachments { get; set; } = new List<AttachmentEntity>();
    }
    
    public class AttachmentEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Hash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key
        public Guid? EventLogId { get; set; }
        public virtual EventLogEntity? EventLog { get; set; }
    }
    
    public class ThreatEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ThreatType { get; set; } = string.Empty;
        public int ThreatLevel { get; set; } // 1-10
        public string? Description { get; set; }
        public string? Source { get; set; }
        public string? Content { get; set; }
        public string? ActionTaken { get; set; }
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
    }
    
    public class ProcessBlockEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ProcessName { get; set; } = string.Empty;
        public string? ProcessPath { get; set; }
        public string? Reason { get; set; }
        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
    }
    
    public class KeystrokeLogEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? WindowTitle { get; set; }
        public string? ProcessName { get; set; }
        public string Text { get; set; } = string.Empty;
        public int KeyCount { get; set; }
        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
        public bool HasThreat { get; set; } = false;
    }
    
    public class MouseActivityEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Action { get; set; } = string.Empty; // Click, DoubleClick, Move, Scroll
        public string? Button { get; set; } // Left, Right, Middle
        public int X { get; set; }
        public int Y { get; set; }
        public string? WindowTitle { get; set; }
        public string? ProcessName { get; set; }
        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    }
    
    public class ApplicationUsageEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ApplicationName { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string? WindowTitle { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public bool IsActive { get; set; } = true;
    }
    
    public class WebsiteVisitEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Url { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string? PageTitle { get; set; }
        public string? Browser { get; set; }
        public DateTime VisitedAt { get; set; } = DateTime.UtcNow;
        public TimeSpan? Duration { get; set; }
        public bool IsSafe { get; set; } = true;
        public string? ThreatReason { get; set; }
    }
    
    public class AlertEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string AlertType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Info"; // Info, Warning, Error, Critical
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public bool IsResolved { get; set; } = false;
    }
    
    public class SettingsEntity
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Description { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
