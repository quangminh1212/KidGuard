using System;
using System.IO;
using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Events;

namespace KidGuard.Infrastructure.Logging
{
    /// <summary>
    /// Centralized logging service with detailed step-by-step logging
    /// </summary>
    public static class LoggingService
    {
        private static ILogger _logger;
        private static readonly object _lock = new object();

        static LoggingService()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize Serilog with file and console sinks
        /// </summary>
        public static void Initialize()
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "KidGuard",
                "Logs",
                $"kidguard_{DateTime.Now:yyyyMMdd}.log"
            );

            // Ensure log directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

            lock (_lock)
            {
                _logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .Enrich.WithThreadId()
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentUserName()
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
                    )
                    .WriteTo.File(
                        logPath,
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] [{ThreadId}] {Message:lj}{NewLine}{Exception}",
                        fileSizeLimitBytes: 10_485_760, // 10MB
                        retainedFileCountLimit: 30,
                        shared: true
                    )
                    .CreateLogger();
            }

            LogInfo("==============================================");
            LogInfo("KidGuard Application Started");
            LogInfo($"Log file: {logPath}");
            LogInfo($"Machine: {Environment.MachineName}");
            LogInfo($"User: {Environment.UserName}");
            LogInfo($"OS: {Environment.OSVersion}");
            LogInfo($".NET Version: {Environment.Version}");
            LogInfo("==============================================");
        }

        /// <summary>
        /// Log debug information with method context
        /// </summary>
        public static void LogDebug(string message, 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var className = Path.GetFileNameWithoutExtension(sourceFilePath);
            _logger?.ForContext("SourceContext", className)
                   .Debug("[{MemberName}:{LineNumber}] {Message}", memberName, sourceLineNumber, message);
        }

        /// <summary>
        /// Log information message
        /// </summary>
        public static void LogInfo(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            var className = Path.GetFileNameWithoutExtension(sourceFilePath);
            _logger?.ForContext("SourceContext", className)
                   .Information("[{MemberName}] {Message}", memberName, message);
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public static void LogWarning(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            var className = Path.GetFileNameWithoutExtension(sourceFilePath);
            _logger?.ForContext("SourceContext", className)
                   .Warning("[{MemberName}] ⚠️ {Message}", memberName, message);
        }

        /// <summary>
        /// Log error with exception details
        /// </summary>
        public static void LogError(Exception ex, string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            var className = Path.GetFileNameWithoutExtension(sourceFilePath);
            _logger?.ForContext("SourceContext", className)
                   .Error(ex, "[{MemberName}] ❌ {Message}", memberName, message);
        }

        /// <summary>
        /// Log critical error
        /// </summary>
        public static void LogCritical(Exception ex, string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            var className = Path.GetFileNameWithoutExtension(sourceFilePath);
            _logger?.ForContext("SourceContext", className)
                   .Fatal(ex, "[{MemberName}] 🔥 CRITICAL: {Message}", memberName, message);
        }

        /// <summary>
        /// Log method entry
        /// </summary>
        public static void LogMethodEntry(object parameters = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            var className = Path.GetFileNameWithoutExtension(sourceFilePath);
            if (parameters != null)
            {
                _logger?.ForContext("SourceContext", className)
                       .Debug("➡️ Entering {MemberName} with parameters: {@Parameters}", memberName, parameters);
            }
            else
            {
                _logger?.ForContext("SourceContext", className)
                       .Debug("➡️ Entering {MemberName}", memberName);
            }
        }

        /// <summary>
        /// Log method exit
        /// </summary>
        public static void LogMethodExit(object returnValue = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            var className = Path.GetFileNameWithoutExtension(sourceFilePath);
            if (returnValue != null)
            {
                _logger?.ForContext("SourceContext", className)
                       .Debug("⬅️ Exiting {MemberName} with return value: {@ReturnValue}", memberName, returnValue);
            }
            else
            {
                _logger?.ForContext("SourceContext", className)
                       .Debug("⬅️ Exiting {MemberName}", memberName);
            }
        }

        /// <summary>
        /// Log performance metrics
        /// </summary>
        public static void LogPerformance(string operation, TimeSpan duration,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            var className = Path.GetFileNameWithoutExtension(sourceFilePath);
            _logger?.ForContext("SourceContext", className)
                   .Information("⏱️ [{MemberName}] {Operation} completed in {Duration:c}", 
                       memberName, operation, duration);
        }

        /// <summary>
        /// Log security event
        /// </summary>
        public static void LogSecurity(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            var className = Path.GetFileNameWithoutExtension(sourceFilePath);
            _logger?.ForContext("SourceContext", className)
                   .Warning("🔒 [{MemberName}] SECURITY: {Message}", memberName, message);
        }

        /// <summary>
        /// Create a scoped logger for a specific context
        /// </summary>
        public static ILogger ForContext<T>()
        {
            return _logger?.ForContext<T>() ?? Log.Logger;
        }

        /// <summary>
        /// Create a scoped logger with property
        /// </summary>
        public static ILogger ForContext(string propertyName, object value)
        {
            return _logger?.ForContext(propertyName, value) ?? Log.Logger;
        }

        /// <summary>
        /// Flush and close logs
        /// </summary>
        public static void CloseAndFlush()
        {
            LogInfo("==============================================");
            LogInfo("KidGuard Application Shutting Down");
            LogInfo("==============================================");
            Log.CloseAndFlush();
        }
    }
}