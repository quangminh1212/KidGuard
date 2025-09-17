using KidGuard.Core.Interfaces;
using KidGuard.Forms;
using KidGuard.Services.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Security.Principal;

namespace KidGuard;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Check for administrator privileges
        if (!IsRunningAsAdministrator())
        {
            var result = MessageBox.Show(
                "KidGuard requires administrator privileges to function properly.\n\n" +
                "Would you like to restart the application with administrator privileges?",
                "Administrator Privileges Required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                RestartAsAdministrator();
            }
        }

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "KidGuard", "logs", "kidguard-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        try
        {
            Log.Information("Starting KidGuard application");

            ApplicationConfiguration.Initialize();
            
            var host = CreateHostBuilder().Build();
            ServiceProvider = host.Services;
            
            var mainForm = ServiceProvider.GetRequiredService<MainForm>();
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            MessageBox.Show($"Fatal error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register services
                services.AddSingleton<IWebsiteBlockingService, WebsiteBlockingService>();
                
                // Register forms
                services.AddSingleton<MainForm>();
                
                // Add logging
                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddSerilog();
                });
            });
    }

    static bool IsRunningAsAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    static void RestartAsAdministrator()
    {
        try
        {
            var exeName = Environment.ProcessPath;
            if (exeName != null)
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = exeName,
                    Verb = "runas"
                };
                
                System.Diagnostics.Process.Start(startInfo);
                Application.Exit();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to restart with administrator privileges: {ex.Message}", 
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}