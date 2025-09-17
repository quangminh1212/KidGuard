using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KidGuard.Core.Services;
using KidGuard.Infrastructure.Data;
using KidGuard.Infrastructure.Logging;
using KidGuard.Infrastructure.Services;

namespace KidGuard.WPF
{
    /// <summary>
    /// Main application class with dependency injection and logging
    /// </summary>
    public partial class App : Application
    {
        private IHost _host;
        private readonly Stopwatch _startupStopwatch = new Stopwatch();

        protected override void OnStartup(StartupEventArgs e)
        {
            _startupStopwatch.Start();
            
            // Initialize logging first
            LoggingService.Initialize();
            LoggingService.LogInfo("=== KidGuard WPF Application Starting ===");
            LoggingService.LogDebug($"Command line args: {string.Join(" ", e.Args)}");

            try
            {
                // Setup exception handlers
                SetupExceptionHandling();
                
                // Build and configure host
                LoggingService.LogDebug("Building application host...");
                _host = CreateHostBuilder(e.Args).Build();
                
                LoggingService.LogDebug("Initializing database...");
                InitializeDatabase();
                
                // Start services
                LoggingService.LogDebug("Starting background services...");
                _host.Start();
                
                // Create and show main window
                LoggingService.LogDebug("Creating main window...");
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
                
                _startupStopwatch.Stop();
                LoggingService.LogPerformance("Application startup", _startupStopwatch.Elapsed);
                LoggingService.LogInfo("=== KidGuard WPF Application Started Successfully ===");
                
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                LoggingService.LogCritical(ex, "Failed to start application");
                MessageBox.Show(
                    $"Failed to start KidGuard:\n\n{ex.Message}\n\nCheck logs for details.",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Shutdown(1);
            }
        }

        private void SetupExceptionHandling()
        {
            LoggingService.LogDebug("Setting up global exception handlers...");
            
            // Handle UI thread exceptions
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            
            // Handle non-UI thread exceptions
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            
            // Handle task exceptions
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            
            LoggingService.LogDebug("Exception handlers configured");
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LoggingService.LogError(e.Exception, "Unhandled UI thread exception");
            
            var result = MessageBox.Show(
                $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nDo you want to continue?",
                "Application Error",
                MessageBoxButton.YesNo,
                MessageBoxImage.Error
            );
            
            e.Handled = result == MessageBoxResult.Yes;
            
            if (!e.Handled)
            {
                LoggingService.LogCritical(e.Exception, "Application terminating due to unhandled exception");
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            LoggingService.LogCritical(exception, $"Unhandled application exception. IsTerminating: {e.IsTerminating}");
            
            if (e.IsTerminating)
            {
                MessageBox.Show(
                    "A critical error has occurred. The application will now close.\n\nCheck the logs for more information.",
                    "Critical Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LoggingService.LogError(e.Exception, "Unobserved task exception");
            e.SetObserved(); // Prevent application crash
        }

        private IHostBuilder CreateHostBuilder(string[] args)
        {
            LoggingService.LogDebug("Configuring host builder...");
            
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    LoggingService.LogDebug("Registering services...");
                    
                    // Register DbContext
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        var dbPath = System.IO.Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "KidGuard",
                            "kidguard.db"
                        );
                        
                        LoggingService.LogDebug($"Database path: {dbPath}");
                        options.UseSqlite($"Data Source={dbPath}");
                        options.EnableSensitiveDataLogging();
                        options.EnableDetailedErrors();
                    });
                    
                    // Register Core Services
                    services.AddSingleton<IAuthenticationService, AuthenticationService>();
                    services.AddSingleton<IApplicationMonitoringService, ApplicationMonitoringService>();
                    services.AddSingleton<IActivityLoggerService, ActivityLoggerService>();
                    services.AddSingleton<IScreenshotService, ScreenshotService>();
                    services.AddSingleton<ITimeManagementService, TimeManagementService>();
                    services.AddSingleton<INotificationService, NotificationService>();
                    services.AddSingleton<IReportService, ReportService>();
                    services.AddSingleton<IWebFilteringService, WebFilteringService>();
                    
                    // Register WPF Windows and Pages
                    services.AddSingleton<MainWindow>();
                    
                    LoggingService.LogDebug($"Registered {services.Count} services");
                });
        }

        private void InitializeDatabase()
        {
            try
            {
                LoggingService.LogDebug("Ensuring database directory exists...");
                var dbDirectory = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "KidGuard"
                );
                System.IO.Directory.CreateDirectory(dbDirectory);
                
                LoggingService.LogDebug("Running database migrations...");
                using (var scope = _host.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Database.EnsureCreated();
                    LoggingService.LogInfo("Database initialized successfully");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "Failed to initialize database");
                throw;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LoggingService.LogInfo("=== KidGuard WPF Application Shutting Down ===");
            LoggingService.LogDebug($"Exit code: {e.ApplicationExitCode}");
            
            try
            {
                LoggingService.LogDebug("Stopping host services...");
                _host?.StopAsync(TimeSpan.FromSeconds(5)).Wait();
                _host?.Dispose();
                LoggingService.LogDebug("Host services stopped");
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "Error during application shutdown");
            }
            finally
            {
                LoggingService.LogInfo("=== KidGuard WPF Application Shutdown Complete ===");
                LoggingService.CloseAndFlush();
            }
            
            base.OnExit(e);
        }
    }
}