using ChildGuard.UI.Localization;
using ChildGuard.UI.Theming;
using System.Diagnostics;
using System.Reflection;

namespace ChildGuard.UI;

public class AboutForm : Form
{
    private Label lblProduct = null!;
    private Label lblVersion = null!;
    private Label lblAuthor = null!;
    private LinkLabel lnkRepo = null!;
    private Button btnClose = null!;

    public AboutForm()
    {
        Text = UIStrings.Get("About.Title");
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(420, 220);
        BuildUI();
        Load += AboutForm_Load;
    }

    private void BuildUI()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 0, Padding = new Padding(12) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        Label MakeKV(string key, string value)
        {
            var panel = new Panel { Height = 28, Dock = DockStyle.Top };
            var k = new Label { Text = key, AutoSize = true, Font = new Font(Font, FontStyle.Bold), Location = new Point(6, 6) };
            var v = new Label { Text = value, AutoSize = true, Location = new Point(160, 6) };
            panel.Controls.Add(k);
            panel.Controls.Add(v);
            root.Controls.Add(panel);
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            return v;
        }

        lblProduct = MakeKV(UIStrings.Get("About.Product"), "");
        lblVersion = MakeKV(UIStrings.Get("About.Version"), "");
        lblAuthor = MakeKV(UIStrings.Get("About.Author"), "");

        // Repo link
        var repoPanel = new Panel { Height = 28, Dock = DockStyle.Top };
        var repoK = new Label { Text = UIStrings.Get("About.Repo"), AutoSize = true, Font = new Font(Font, FontStyle.Bold), Location = new Point(6, 6) };
        lnkRepo = new LinkLabel { Text = "https://github.com/quangminh1212/ChildGuard", AutoSize = true, Location = new Point(160, 6), LinkColor = Color.RoyalBlue };
        lnkRepo.LinkClicked += (s, e) => { try { Process.Start(new ProcessStartInfo(lnkRepo.Text) { UseShellExecute = true }); } catch { } };
        repoPanel.Controls.Add(repoK);
        repoPanel.Controls.Add(lnkRepo);
        root.Controls.Add(repoPanel);
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Close button
        var pnlBtn = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(8) };
        btnClose = new Button { Text = UIStrings.Get("Buttons.Close"), Width = 96, Height = 30 };
        btnClose.Click += (s, e) => Close();
        pnlBtn.Controls.Add(btnClose);
        root.Controls.Add(pnlBtn);
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        Controls.Add(root);
    }

    private void AboutForm_Load(object? sender, EventArgs e)
    {
        // Theme per config
        var cfg = ChildGuard.Core.Configuration.ConfigManager.Load(out _);
        UIStrings.SetLanguage(cfg.UILanguage);
        ModernStyle.Apply(this, (cfg.Theme?.ToLowerInvariant()) switch { "dark" => ThemeMode.Dark, "light" => ThemeMode.Light, _ => ThemeMode.System });
        Text = UIStrings.Get("About.Title");
        btnClose.Text = UIStrings.Get("Buttons.Close");

        // Fill values
        var asm = Assembly.GetExecutingAssembly();
        var productAttr = asm.GetCustomAttribute<AssemblyProductAttribute>();
        var product = productAttr?.Product ?? UIStrings.Get("General.AppName");
        var version = Application.ProductVersion;
        lblProduct.Text = product;
        lblVersion.Text = version;
        lblAuthor.Text = "quangminh1212";
    }
}

