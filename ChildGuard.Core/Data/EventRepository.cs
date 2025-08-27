using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using ChildGuard.Core.Models;

namespace ChildGuard.Core.Data
{
    /// <summary>
    /// Implementation của IEventRepository sử dụng Dapper
    /// </summary>
    public class EventRepository : IEventRepository
    {
        private readonly string _connectionString;
        
        public EventRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        
        public EventRepository(DatabaseInitializer dbInitializer)
        {
            _connectionString = dbInitializer.ConnectionString;
        }
        
        // Event operations
        public async Task<long> AddEventAsync(EventLog eventLog)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"
                INSERT INTO Events (TimestampUtc, Type, Severity, Source, Title, Content, MetaJson, AttachmentPath, IsRead, CreatedAt)
                VALUES (@TimestampUtc, @Type, @Severity, @Source, @Title, @Content, @MetaJson, @AttachmentPath, @IsRead, @CreatedAt);
                SELECT last_insert_rowid();";
            
            var id = await connection.QuerySingleAsync<long>(sql, new
            {
                TimestampUtc = eventLog.TimestampUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                Type = (int)eventLog.Type,
                Severity = (int)eventLog.Severity,
                eventLog.Source,
                eventLog.Title,
                eventLog.Content,
                eventLog.MetaJson,
                eventLog.AttachmentPath,
                IsRead = eventLog.IsRead ? 1 : 0,
                CreatedAt = eventLog.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });
            
            eventLog.Id = id;
            return id;
        }
        
        public async Task<EventLog?> GetEventByIdAsync(long id)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT * FROM Events WHERE Id = @Id";
            
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id });
            if (result == null) return null;
            
            return MapToEventLog(result);
        }
        
        public async Task<IEnumerable<EventLog>> GetEventsAsync(DateTime? fromDate = null, DateTime? toDate = null, EventType? type = null, EventSeverity? severity = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT * FROM Events WHERE 1=1";
            var parameters = new DynamicParameters();
            
            if (fromDate.HasValue)
            {
                sql += " AND TimestampUtc >= @FromDate";
                parameters.Add("FromDate", fromDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            
            if (toDate.HasValue)
            {
                sql += " AND TimestampUtc <= @ToDate";
                parameters.Add("ToDate", toDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            
            if (type.HasValue)
            {
                sql += " AND Type = @Type";
                parameters.Add("Type", (int)type.Value);
            }
            
            if (severity.HasValue)
            {
                sql += " AND Severity = @Severity";
                parameters.Add("Severity", (int)severity.Value);
            }
            
            sql += " ORDER BY TimestampUtc DESC";
            
            var results = await connection.QueryAsync<dynamic>(sql, parameters);
            return results.Select(MapToEventLog);
        }
        
        public async Task<IEnumerable<EventLog>> GetRecentEventsAsync(int count = 100)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT * FROM Events ORDER BY TimestampUtc DESC LIMIT @Count";
            
            var results = await connection.QueryAsync<dynamic>(sql, new { Count = count });
            return results.Select(MapToEventLog);
        }
        
        public async Task<IEnumerable<EventLog>> GetUnreadEventsAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT * FROM Events WHERE IsRead = 0 ORDER BY TimestampUtc DESC";
            
            var results = await connection.QueryAsync<dynamic>(sql);
            return results.Select(MapToEventLog);
        }
        
        public async Task<bool> MarkEventAsReadAsync(long id)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "UPDATE Events SET IsRead = 1 WHERE Id = @Id";
            
            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
        
        public async Task<bool> MarkAllEventsAsReadAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "UPDATE Events SET IsRead = 1 WHERE IsRead = 0";
            
            var affected = await connection.ExecuteAsync(sql);
            return affected > 0;
        }
        
        public async Task<bool> DeleteEventAsync(long id)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "DELETE FROM Events WHERE Id = @Id";
            
            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
        
        public async Task<int> DeleteOldEventsAsync(DateTime olderThan)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "DELETE FROM Events WHERE TimestampUtc < @OlderThan";
            
            return await connection.ExecuteAsync(sql, new { OlderThan = olderThan.ToString("yyyy-MM-dd HH:mm:ss") });
        }
        
        // Attachment operations
        public async Task<long> AddAttachmentAsync(EventAttachment attachment)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"
                INSERT INTO Attachments (EventLogId, FileName, FilePath, FileType, FileSize, CreatedAt)
                VALUES (@EventLogId, @FileName, @FilePath, @FileType, @FileSize, @CreatedAt);
                SELECT last_insert_rowid();";
            
            var id = await connection.QuerySingleAsync<long>(sql, new
            {
                attachment.EventLogId,
                attachment.FileName,
                attachment.FilePath,
                attachment.FileType,
                attachment.FileSize,
                CreatedAt = attachment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });
            
            attachment.Id = id;
            return id;
        }
        
        public async Task<IEnumerable<EventAttachment>> GetAttachmentsByEventIdAsync(long eventId)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT * FROM Attachments WHERE EventLogId = @EventId";
            
            return await connection.QueryAsync<EventAttachment>(sql, new { EventId = eventId });
        }
        
        public async Task<bool> DeleteAttachmentAsync(long id)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "DELETE FROM Attachments WHERE Id = @Id";
            
            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
        
        // Keystroke operations
        public async Task<long> AddKeystrokeAsync(KeystrokeLog keystroke)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"
                INSERT INTO Keystrokes (TimestampUtc, WindowTitle, ProcessName, KeyData, CreatedAt)
                VALUES (@TimestampUtc, @WindowTitle, @ProcessName, @KeyData, @CreatedAt);
                SELECT last_insert_rowid();";
            
            var id = await connection.QuerySingleAsync<long>(sql, new
            {
                TimestampUtc = keystroke.TimestampUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                keystroke.WindowTitle,
                keystroke.ProcessName,
                keystroke.KeyData,
                CreatedAt = keystroke.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });
            
            keystroke.Id = id;
            return id;
        }
        
        public async Task<IEnumerable<KeystrokeLog>> GetKeystrokesAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT * FROM Keystrokes WHERE 1=1";
            var parameters = new DynamicParameters();
            
            if (fromDate.HasValue)
            {
                sql += " AND TimestampUtc >= @FromDate";
                parameters.Add("FromDate", fromDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            
            if (toDate.HasValue)
            {
                sql += " AND TimestampUtc <= @ToDate";
                parameters.Add("ToDate", toDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            
            sql += " ORDER BY TimestampUtc DESC";
            
            return await connection.QueryAsync<KeystrokeLog>(sql, parameters);
        }
        
        public async Task<int> DeleteOldKeystrokesAsync(DateTime olderThan)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "DELETE FROM Keystrokes WHERE TimestampUtc < @OlderThan";
            
            return await connection.ExecuteAsync(sql, new { OlderThan = olderThan.ToString("yyyy-MM-dd HH:mm:ss") });
        }
        
        // Process operations
        public async Task<long> AddProcessLogAsync(ProcessLog processLog)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"
                INSERT INTO Processes (TimestampUtc, ProcessName, ProcessPath, ProcessId, Action, Reason, CreatedAt)
                VALUES (@TimestampUtc, @ProcessName, @ProcessPath, @ProcessId, @Action, @Reason, @CreatedAt);
                SELECT last_insert_rowid();";
            
            var id = await connection.QuerySingleAsync<long>(sql, new
            {
                TimestampUtc = processLog.TimestampUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                processLog.ProcessName,
                processLog.ProcessPath,
                processLog.ProcessId,
                processLog.Action,
                processLog.Reason,
                CreatedAt = processLog.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });
            
            processLog.Id = id;
            return id;
        }
        
        public async Task<IEnumerable<ProcessLog>> GetProcessLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? action = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT * FROM Processes WHERE 1=1";
            var parameters = new DynamicParameters();
            
            if (fromDate.HasValue)
            {
                sql += " AND TimestampUtc >= @FromDate";
                parameters.Add("FromDate", fromDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            
            if (toDate.HasValue)
            {
                sql += " AND TimestampUtc <= @ToDate";
                parameters.Add("ToDate", toDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            
            if (!string.IsNullOrEmpty(action))
            {
                sql += " AND Action = @Action";
                parameters.Add("Action", action);
            }
            
            sql += " ORDER BY TimestampUtc DESC";
            
            return await connection.QueryAsync<ProcessLog>(sql, parameters);
        }
        
        public async Task<IEnumerable<ProcessLog>> GetBlockedProcessesAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await GetProcessLogsAsync(fromDate, toDate, "Blocked");
        }
        
        // Statistics operations
        public async Task<Dictionary<EventType, int>> GetEventCountsByTypeAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT Type, COUNT(*) as Count FROM Events WHERE 1=1";
            var parameters = new DynamicParameters();
            
            if (fromDate.HasValue)
            {
                sql += " AND TimestampUtc >= @FromDate";
                parameters.Add("FromDate", fromDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            
            if (toDate.HasValue)
            {
                sql += " AND TimestampUtc <= @ToDate";
                parameters.Add("ToDate", toDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            
            sql += " GROUP BY Type";
            
            var results = await connection.QueryAsync<dynamic>(sql, parameters);
            return results.ToDictionary(r => (EventType)r.Type, r => (int)r.Count);
        }
        
        public async Task<Dictionary<EventSeverity, int>> GetEventCountsBySeverityAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT Severity, COUNT(*) as Count FROM Events WHERE 1=1";
            var parameters = new DynamicParameters();
            
            if (fromDate.HasValue)
            {
                sql += " AND TimestampUtc >= @FromDate";
                parameters.Add("FromDate", fromDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            
            if (toDate.HasValue)
            {
                sql += " AND TimestampUtc <= @ToDate";
                parameters.Add("ToDate", toDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            
            sql += " GROUP BY Severity";
            
            var results = await connection.QueryAsync<dynamic>(sql, parameters);
            return results.ToDictionary(r => (EventSeverity)r.Severity, r => (int)r.Count);
        }
        
        public async Task<int> GetTotalEventCountAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT COUNT(*) FROM Events WHERE 1=1";
            var parameters = new DynamicParameters();
            
            if (fromDate.HasValue)
            {
                sql += " AND TimestampUtc >= @FromDate";
                parameters.Add("FromDate", fromDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            
            if (toDate.HasValue)
            {
                sql += " AND TimestampUtc <= @ToDate";
                parameters.Add("ToDate", toDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            
            return await connection.QuerySingleAsync<int>(sql, parameters);
        }
        
        public async Task<int> GetThreatCountAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT COUNT(*) FROM Events WHERE Severity >= @MinSeverity";
            var parameters = new DynamicParameters();
            parameters.Add("MinSeverity", (int)EventSeverity.High);
            
            if (fromDate.HasValue)
            {
                sql += " AND TimestampUtc >= @FromDate";
                parameters.Add("FromDate", fromDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            
            if (toDate.HasValue)
            {
                sql += " AND TimestampUtc <= @ToDate";
                parameters.Add("ToDate", toDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            
            return await connection.QuerySingleAsync<int>(sql, parameters);
        }
        
        // Search operations
        public async Task<IEnumerable<EventLog>> SearchEventsAsync(string searchTerm, int maxResults = 100)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"
                SELECT * FROM Events 
                WHERE Title LIKE @SearchTerm OR Content LIKE @SearchTerm OR Source LIKE @SearchTerm
                ORDER BY TimestampUtc DESC
                LIMIT @MaxResults";
            
            var results = await connection.QueryAsync<dynamic>(sql, new 
            { 
                SearchTerm = $"%{searchTerm}%",
                MaxResults = maxResults
            });
            
            return results.Select(MapToEventLog);
        }
        
        // Cleanup operations
        public async Task<long> GetDatabaseSizeAsync()
        {
            var dbInit = new DatabaseInitializer(_connectionString.Replace("Data Source=", ""));
            return await Task.FromResult(dbInit.GetDatabaseSize());
        }
        
        public async Task<int> CleanupOldDataAsync(int daysToKeep)
        {
            var olderThan = DateTime.UtcNow.AddDays(-daysToKeep);
            var totalDeleted = 0;
            
            totalDeleted += await DeleteOldEventsAsync(olderThan);
            totalDeleted += await DeleteOldKeystrokesAsync(olderThan);
            
            // Xóa process logs cũ
            using var connection = new SqliteConnection(_connectionString);
            var sql = "DELETE FROM Processes WHERE TimestampUtc < @OlderThan";
            totalDeleted += await connection.ExecuteAsync(sql, new { OlderThan = olderThan.ToString("yyyy-MM-dd HH:mm:ss") });
            
            return totalDeleted;
        }
        
        // Helper methods
        private EventLog MapToEventLog(dynamic row)
        {
            return new EventLog
            {
                Id = row.Id,
                TimestampUtc = DateTime.Parse(row.TimestampUtc),
                Type = (EventType)row.Type,
                Severity = (EventSeverity)row.Severity,
                Source = row.Source,
                Title = row.Title,
                Content = row.Content,
                MetaJson = row.MetaJson,
                AttachmentPath = row.AttachmentPath,
                IsRead = row.IsRead == 1,
                CreatedAt = DateTime.Parse(row.CreatedAt)
            };
        }
    }
}
