using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace ChildGuard.Core.Data
{
    /// <summary>
    /// Class khởi tạo và cấu hình database SQLite
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly string _connectionString;
        private readonly string _databasePath;
        
        public string DatabasePath => _databasePath;
        public string ConnectionString => _connectionString;
        
        public DatabaseInitializer()
        {
            // Tạo đường dẫn database trong AppData
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ChildGuard"
            );
            
            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            
            _databasePath = Path.Combine(appDataPath, "childguard.db");
            _connectionString = $"Data Source={_databasePath}";
        }
        
        public DatabaseInitializer(string databasePath)
        {
            _databasePath = databasePath;
            _connectionString = $"Data Source={_databasePath}";
            
            var directory = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        
        /// <summary>
        /// Khởi tạo database với các bảng cần thiết
        /// </summary>
        public async Task InitializeAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            
            // Tạo bảng Events
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Events (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TimestampUtc TEXT NOT NULL,
                    Type INTEGER NOT NULL,
                    Severity INTEGER NOT NULL,
                    Source TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    Content TEXT,
                    MetaJson TEXT,
                    AttachmentPath TEXT,
                    IsRead INTEGER DEFAULT 0,
                    CreatedAt TEXT NOT NULL
                );
                
                CREATE INDEX IF NOT EXISTS idx_events_timestamp ON Events(TimestampUtc);
                CREATE INDEX IF NOT EXISTS idx_events_type ON Events(Type);
                CREATE INDEX IF NOT EXISTS idx_events_severity ON Events(Severity);
                CREATE INDEX IF NOT EXISTS idx_events_isread ON Events(IsRead);
            ";
            await command.ExecuteNonQueryAsync();
            
            // Tạo bảng Attachments
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Attachments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EventLogId INTEGER NOT NULL,
                    FileName TEXT NOT NULL,
                    FilePath TEXT NOT NULL,
                    FileType TEXT NOT NULL,
                    FileSize INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (EventLogId) REFERENCES Events(Id) ON DELETE CASCADE
                );
                
                CREATE INDEX IF NOT EXISTS idx_attachments_eventlogid ON Attachments(EventLogId);
            ";
            await command.ExecuteNonQueryAsync();
            
            // Tạo bảng Keystrokes
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Keystrokes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TimestampUtc TEXT NOT NULL,
                    WindowTitle TEXT NOT NULL,
                    ProcessName TEXT NOT NULL,
                    KeyData TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                );
                
                CREATE INDEX IF NOT EXISTS idx_keystrokes_timestamp ON Keystrokes(TimestampUtc);
                CREATE INDEX IF NOT EXISTS idx_keystrokes_processname ON Keystrokes(ProcessName);
            ";
            await command.ExecuteNonQueryAsync();
            
            // Tạo bảng Processes
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Processes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TimestampUtc TEXT NOT NULL,
                    ProcessName TEXT NOT NULL,
                    ProcessPath TEXT NOT NULL,
                    ProcessId INTEGER NOT NULL,
                    Action TEXT NOT NULL,
                    Reason TEXT,
                    CreatedAt TEXT NOT NULL
                );
                
                CREATE INDEX IF NOT EXISTS idx_processes_timestamp ON Processes(TimestampUtc);
                CREATE INDEX IF NOT EXISTS idx_processes_name ON Processes(ProcessName);
                CREATE INDEX IF NOT EXISTS idx_processes_action ON Processes(Action);
            ";
            await command.ExecuteNonQueryAsync();
            
            // Tạo bảng Screenshots
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Screenshots (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TimestampUtc TEXT NOT NULL,
                    FilePath TEXT NOT NULL,
                    FileSize INTEGER NOT NULL,
                    Width INTEGER NOT NULL,
                    Height INTEGER NOT NULL,
                    TriggerType TEXT NOT NULL,
                    TriggerEventId INTEGER,
                    CreatedAt TEXT NOT NULL
                );
                
                CREATE INDEX IF NOT EXISTS idx_screenshots_timestamp ON Screenshots(TimestampUtc);
                CREATE INDEX IF NOT EXISTS idx_screenshots_trigger ON Screenshots(TriggerType);
            ";
            await command.ExecuteNonQueryAsync();
            
            // Tạo bảng AudioClips
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS AudioClips (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TimestampUtc TEXT NOT NULL,
                    FilePath TEXT NOT NULL,
                    FileSize INTEGER NOT NULL,
                    Duration INTEGER NOT NULL,
                    TriggerType TEXT NOT NULL,
                    TriggerEventId INTEGER,
                    CreatedAt TEXT NOT NULL
                );
                
                CREATE INDEX IF NOT EXISTS idx_audioclips_timestamp ON AudioClips(TimestampUtc);
                CREATE INDEX IF NOT EXISTS idx_audioclips_trigger ON AudioClips(TriggerType);
            ";
            await command.ExecuteNonQueryAsync();
            
            // Tạo bảng Statistics để lưu thống kê
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Statistics (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT NOT NULL UNIQUE,
                    TotalEvents INTEGER DEFAULT 0,
                    TotalThreats INTEGER DEFAULT 0,
                    TotalKeystrokes INTEGER DEFAULT 0,
                    TotalProcessesBlocked INTEGER DEFAULT 0,
                    TotalScreenshots INTEGER DEFAULT 0,
                    TotalAudioClips INTEGER DEFAULT 0,
                    UpdatedAt TEXT NOT NULL
                );
                
                CREATE INDEX IF NOT EXISTS idx_statistics_date ON Statistics(Date);
            ";
            await command.ExecuteNonQueryAsync();
        }
        
        /// <summary>
        /// Xóa database (dùng cho testing hoặc reset)
        /// </summary>
        public void DeleteDatabase()
        {
            if (File.Exists(_databasePath))
            {
                File.Delete(_databasePath);
            }
        }
        
        /// <summary>
        /// Kiểm tra database có tồn tại không
        /// </summary>
        public bool DatabaseExists()
        {
            return File.Exists(_databasePath);
        }
        
        /// <summary>
        /// Lấy kích thước database
        /// </summary>
        public long GetDatabaseSize()
        {
            if (File.Exists(_databasePath))
            {
                var fileInfo = new FileInfo(_databasePath);
                return fileInfo.Length;
            }
            return 0;
        }
    }
}
