namespace ChildGuard.Core.Configuration;

using System.Text.Json;

public static class ConfigManager
{
    public static string GetProgramDataDir() => "C:/ProgramData/ChildGuard";
    public static string GetLocalAppDataDir() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChildGuard");
    public static string GetProgramDataConfigPath() => Path.Combine(GetProgramDataDir(), "config.json");
    public static string GetLocalAppDataConfigPath() => Path.Combine(GetLocalAppDataDir(), "config.json");

    private static bool IsDirectoryWritable(string dir)
    {
        try
        {
            Directory.CreateDirectory(dir);
            var test = Path.Combine(dir, ".write_test");
            File.WriteAllText(test, "ok");
            File.Delete(test);
            return true;
        }
        catch { return false; }
    }

    public static AppConfig Load(out string pathUsed)
    {
        // Try ProgramData first
        var programPath = GetProgramDataConfigPath();
        if (File.Exists(programPath))
        {
            var cfg = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(programPath)) ?? new AppConfig();
            pathUsed = programPath;
            return cfg;
        }

        // Try LocalAppData
        var localPath = GetLocalAppDataConfigPath();
        if (File.Exists(localPath))
        {
            var cfg = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(localPath)) ?? new AppConfig();
            pathUsed = localPath;
            return cfg;
        }

        // None found: choose preferred writeable location
        var preferProgram = IsDirectoryWritable(GetProgramDataDir());
        pathUsed = preferProgram ? programPath : localPath;
        var defaultCfg = new AppConfig();
        if (!preferProgram)
        {
            // If ProgramData not writable, default data dir to user's local app data
            defaultCfg.DataDirectory = GetLocalAppDataDir();
        }
        return defaultCfg;
    }

    public static void Save(AppConfig cfg, out string pathUsed)
    {
        var programDir = GetProgramDataDir();
        var localDir = GetLocalAppDataDir();
        var preferProgram = IsDirectoryWritable(programDir);
        pathUsed = preferProgram ? GetProgramDataConfigPath() : GetLocalAppDataConfigPath();
        var dir = Path.GetDirectoryName(pathUsed)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(pathUsed, json);
    }
}

