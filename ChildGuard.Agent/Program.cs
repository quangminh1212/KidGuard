namespace ChildGuard.Agent;

static class Program
{
    [STAThread]
    static void Main()
    {
        // Explicitly enable Per-Monitor V2 DPI awareness for the agent UI
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Application.Run(new Form1());
    }
}
