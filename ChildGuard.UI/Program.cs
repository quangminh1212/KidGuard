using System.Diagnostics;

namespace ChildGuard.UI;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Enable high DPI support (Per-Monitor V2)
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        // Setup global exception handlers
        Application.ThreadException += Application_ThreadException;
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        
        // Parse command line arguments
        bool openSettings = false, openReports = false;
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--open", StringComparison.OrdinalIgnoreCase) && i+1 < args.Length)
            {
                var v = args[i+1].ToLowerInvariant();
                if (v == "settings") openSettings = true;
                if (v == "reports") openReports = true;
                i++;
            }
        }
        
        // Use ModernMainForm as the main application shell
        var mainForm = new ModernMainForm();
        
        // Handle command line navigation after form is shown
        mainForm.Shown += (s, e) =>
        {
            if (openSettings)
            {
                try 
                { 
                    // Navigate to Settings panel in ModernMainForm
                    // The form already has navigation, so we can trigger it
                    // This would need a public method in ModernMainForm to navigate programmatically
                    // For now, we'll open the standalone form as a fallback
                    new SettingsForm().Show(mainForm); 
                } 
                catch (Exception ex) 
                { 
                    Debug.WriteLine($"Error opening settings: {ex.Message}");
                }
            }
            if (openReports)
            {
                try 
                { 
                    // Navigate to Reports panel in ModernMainForm  
                    // For now, we'll open the standalone form as a fallback
                    new ReportsForm().Show(mainForm); 
                } 
                catch (Exception ex) 
                { 
                    Debug.WriteLine($"Error opening reports: {ex.Message}");
                }
            }
        };
        
        // Run the application
        Application.Run(mainForm);
    }
    
    private static void Application_ThreadException(object? sender, System.Threading.ThreadExceptionEventArgs e)
    {
        LogException("Thread Exception", e.Exception);
    }
    
    private static void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogException("Unhandled Domain Exception", ex);
        }
    }
    
    private static void LogException(string source, Exception exception)
    {
        try
        {
            // Log to debug output
            Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}: {exception}");
            
            // TODO: In production, log to file or event log
            // For now, we'll just write to debug output
            // string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChildGuard", "error.log");
            // File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}: {exception}\n\n");
            
            // Optionally show error dialog in debug builds
            #if DEBUG
            MessageBox.Show(
                $"An error occurred:\n\n{exception.Message}\n\nSource: {source}", 
                "ChildGuard - Error", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error
            );
            #endif
        }
        catch
        {
            // If we can't log, at least don't crash the error handler
        }
    }
}
