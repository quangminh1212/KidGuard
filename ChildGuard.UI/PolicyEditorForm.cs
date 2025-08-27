using System.Text.Json;
using ChildGuard.Core.Configuration;
using ChildGuard.UI.Localization;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI;

public partial class PolicyEditorForm : Form
{
    private string _configPath = string.Empty;
    private AppConfig _config = new();

public PolicyEditorForm()
    {
        InitializeComponent();
        // Theme applied after loading config
    }

    private void PolicyEditorForm_Load(object? sender, EventArgs e)
    {
        _config = ConfigManager.Load(out _configPath);
        UIStrings.SetLanguage(_config.UILanguage);
        ApplyLocalization();
        ModernStyle.Apply(this, ParseTheme(_config.Theme));
        try { txtJson.Font = new Font("Consolas", 10f); } catch { }
        var opts = new JsonSerializerOptions { WriteIndented = true };
        txtJson.Text = JsonSerializer.Serialize(_config.PolicyRules, opts);
        lblPath.Text = string.Format(UIStrings.Get("Policy.ConfigPath"), _configPath);
    }

    private void ApplyLocalization()
    {
        this.Text = UIStrings.Get("Policy.Title");
        btnSave.Text = UIStrings.Get("Buttons.Save");
        btnCancel.Text = UIStrings.Get("Buttons.Cancel");
    }

    private static ThemeMode ParseTheme(string? s)
    {
        return (s?.ToLowerInvariant()) switch
        {
            "dark" => ThemeMode.Dark,
            "light" => ThemeMode.Light,
            _ => ThemeMode.System
        };
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            var rules = JsonSerializer.Deserialize<PolicyRule[]>(txtJson.Text) ?? Array.Empty<PolicyRule>();
            _config.PolicyRules = rules;
            ConfigManager.Save(_config, out var saved);
            lblPath.Text = string.Format(UIStrings.Get("Policy.ConfigPath"), saved);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, UIStrings.Format("Policy.InvalidJson", ex.Message), UIStrings.Get("General.AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
