using KidGuard.Core.Interfaces;
using KidGuard.Core.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace KidGuard.Forms;

public partial class MainForm : Form
{
    private readonly IWebsiteBlockingService _websiteBlockingService;
    private readonly ILogger<MainForm> _logger;
    
    private TabControl tabControl = null!;
    private TabPage websiteBlockingTab = null!;
    private TabPage applicationMonitoringTab = null!;
    private TabPage dashboardTab = null!;
    
    // Website Blocking Controls
    private TextBox txtWebsite = null!;
    private ComboBox cmbCategory = null!;
    private Button btnBlock = null!;
    private Button btnUnblock = null!;
    private DataGridView dgvBlockedWebsites = null!;
    private ToolStripStatusLabel lblStatus = null!;
    
    public MainForm(IWebsiteBlockingService websiteBlockingService, ILogger<MainForm> logger)
    {
        _websiteBlockingService = websiteBlockingService;
        _logger = logger;
        
        InitializeComponent();
        _ = LoadBlockedWebsites();
    }
    
    private void InitializeComponent()
    {
        Text = "KidGuard - Parental Control";
        Size = new Size(1000, 700);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = SystemIcons.Shield;
        
        // Create TabControl
        tabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };
        
        // Dashboard Tab
        dashboardTab = new TabPage("Dashboard");
        CreateDashboardTab();
        
        // Website Blocking Tab
        websiteBlockingTab = new TabPage("Website Blocking");
        CreateWebsiteBlockingTab();
        
        // Application Monitoring Tab
        applicationMonitoringTab = new TabPage("Application Control");
        CreateApplicationMonitoringTab();
        
        tabControl.TabPages.Add(dashboardTab);
        tabControl.TabPages.Add(websiteBlockingTab);
        tabControl.TabPages.Add(applicationMonitoringTab);
        
        Controls.Add(tabControl);
        
        // Status bar
        var statusStrip = new StatusStrip();
        lblStatus = new ToolStripStatusLabel("Ready");
        statusStrip.Items.Add(lblStatus);
        Controls.Add(statusStrip);
    }
    
    private void CreateDashboardTab()
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        
        // Title
        var lblTitle = new Label
        {
            Text = "KidGuard Dashboard",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            Location = new Point(20, 20),
            Size = new Size(400, 40)
        };
        
        // Statistics Panel
        var statsPanel = new GroupBox
        {
            Text = "Protection Statistics",
            Location = new Point(20, 80),
            Size = new Size(400, 200)
        };
        
        var lblBlockedSites = new Label
        {
            Text = "Blocked Websites: 0",
            Location = new Point(20, 30),
            Size = new Size(350, 30),
            Font = new Font("Segoe UI", 10)
        };
        
        var lblBlockedApps = new Label
        {
            Text = "Monitored Applications: 0",
            Location = new Point(20, 60),
            Size = new Size(350, 30),
            Font = new Font("Segoe UI", 10)
        };
        
        var lblProtectionStatus = new Label
        {
            Text = "Protection Status: ACTIVE",
            Location = new Point(20, 90),
            Size = new Size(350, 30),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.Green
        };
        
        statsPanel.Controls.AddRange(new Control[] { lblBlockedSites, lblBlockedApps, lblProtectionStatus });
        
        // Quick Actions Panel
        var actionsPanel = new GroupBox
        {
            Text = "Quick Actions",
            Location = new Point(440, 80),
            Size = new Size(400, 200)
        };
        
        var btnQuickBlock = new Button
        {
            Text = "Quick Block Website",
            Location = new Point(20, 30),
            Size = new Size(360, 40)
        };
        btnQuickBlock.Click += (s, e) => tabControl.SelectedTab = websiteBlockingTab;
        
        var btnExportReport = new Button
        {
            Text = "Export Activity Report",
            Location = new Point(20, 80),
            Size = new Size(360, 40)
        };
        
        var btnSettings = new Button
        {
            Text = "Settings",
            Location = new Point(20, 130),
            Size = new Size(360, 40)
        };
        
        actionsPanel.Controls.AddRange(new Control[] { btnQuickBlock, btnExportReport, btnSettings });
        
        panel.Controls.AddRange(new Control[] { lblTitle, statsPanel, actionsPanel });
        dashboardTab.Controls.Add(panel);
    }
    
    private void CreateWebsiteBlockingTab()
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        
        // Input Panel
        var inputPanel = new GroupBox
        {
            Text = "Block Website",
            Location = new Point(20, 20),
            Size = new Size(940, 120)
        };
        
        var lblWebsite = new Label
        {
            Text = "Website URL:",
            Location = new Point(20, 30),
            Size = new Size(100, 25)
        };
        
        txtWebsite = new TextBox
        {
            Location = new Point(130, 27),
            Size = new Size(400, 25),
            PlaceholderText = "e.g., facebook.com or https://youtube.com"
        };
        
        var lblCategory = new Label
        {
            Text = "Category:",
            Location = new Point(20, 65),
            Size = new Size(100, 25)
        };
        
        cmbCategory = new ComboBox
        {
            Location = new Point(130, 62),
            Size = new Size(200, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbCategory.Items.AddRange(new[] { "Social Media", "Gaming", "Video", "Adult", "Shopping", "General" });
        cmbCategory.SelectedIndex = 0;
        
        btnBlock = new Button
        {
            Text = "Block Website",
            Location = new Point(550, 27),
            Size = new Size(120, 30),
            BackColor = Color.IndianRed,
            ForeColor = Color.White
        };
        btnBlock.Click += BtnBlock_Click;
        
        btnUnblock = new Button
        {
            Text = "Unblock Selected",
            Location = new Point(680, 27),
            Size = new Size(120, 30),
            BackColor = Color.MediumSeaGreen,
            ForeColor = Color.White
        };
        btnUnblock.Click += BtnUnblock_Click;
        
        var btnImport = new Button
        {
            Text = "Import List",
            Location = new Point(550, 62),
            Size = new Size(120, 30)
        };
        btnImport.Click += BtnImport_Click;
        
        var btnExport = new Button
        {
            Text = "Export List",
            Location = new Point(680, 62),
            Size = new Size(120, 30)
        };
        btnExport.Click += BtnExport_Click;
        
        inputPanel.Controls.AddRange(new Control[] { 
            lblWebsite, txtWebsite, lblCategory, cmbCategory, 
            btnBlock, btnUnblock, btnImport, btnExport 
        });
        
        // DataGridView for blocked websites
        dgvBlockedWebsites = new DataGridView
        {
            Location = new Point(20, 150),
            Size = new Size(940, 400),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        
        dgvBlockedWebsites.Columns.Add("Domain", "Domain");
        dgvBlockedWebsites.Columns.Add("Category", "Category");
        dgvBlockedWebsites.Columns.Add("BlockedAt", "Blocked Date");
        dgvBlockedWebsites.Columns.Add("Status", "Status");
        
        panel.Controls.AddRange(new Control[] { inputPanel, dgvBlockedWebsites });
        websiteBlockingTab.Controls.Add(panel);
    }
    
    private void CreateApplicationMonitoringTab()
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        
        var lblInfo = new Label
        {
            Text = "Application Monitoring - Coming Soon",
            Font = new Font("Segoe UI", 14),
            Location = new Point(20, 20),
            Size = new Size(500, 40)
        };
        
        var lblDescription = new Label
        {
            Text = "This feature will allow you to:\n" +
                   "• Monitor and block specific applications\n" +
                   "• Set time limits for applications\n" +
                   "• View application usage statistics\n" +
                   "• Schedule allowed usage times",
            Location = new Point(20, 80),
            Size = new Size(600, 200)
        };
        
        panel.Controls.AddRange(new Control[] { lblInfo, lblDescription });
        applicationMonitoringTab.Controls.Add(panel);
    }
    
    private async void BtnBlock_Click(object? sender, EventArgs e)
    {
        var website = txtWebsite.Text.Trim();
        if (string.IsNullOrEmpty(website))
        {
            MessageBox.Show("Please enter a website URL", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        try
        {
            var category = cmbCategory.SelectedItem?.ToString() ?? "General";
            var success = await _websiteBlockingService.BlockWebsiteAsync(website, category);
            
            if (success)
            {
                UpdateStatus($"Successfully blocked: {website}", Color.Green);
                txtWebsite.Clear();
                await LoadBlockedWebsites();
            }
            else
            {
                UpdateStatus($"Failed to block: {website}", Color.Red);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking website");
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async void BtnUnblock_Click(object? sender, EventArgs e)
    {
        if (dgvBlockedWebsites.SelectedRows.Count == 0)
        {
            MessageBox.Show("Please select a website to unblock", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        try
        {
            var domain = dgvBlockedWebsites.SelectedRows[0].Cells["Domain"].Value?.ToString();
            if (!string.IsNullOrEmpty(domain))
            {
                var success = await _websiteBlockingService.UnblockWebsiteAsync(domain);
                
                if (success)
                {
                    UpdateStatus($"Successfully unblocked: {domain}", Color.Green);
                    await LoadBlockedWebsites();
                }
                else
                {
                    UpdateStatus($"Failed to unblock: {domain}", Color.Red);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblocking website");
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async void BtnImport_Click(object? sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            Title = "Import Website List"
        };
        
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var lines = await File.ReadAllLinesAsync(openFileDialog.FileName);
                var category = cmbCategory.SelectedItem?.ToString() ?? "General";
                var count = await _websiteBlockingService.ImportBlockListAsync(lines, category);
                
                UpdateStatus($"Imported {count} websites", Color.Green);
                await LoadBlockedWebsites();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing website list");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
    private async void BtnExport_Click(object? sender, EventArgs e)
    {
        using var saveFileDialog = new SaveFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            Title = "Export Website List",
            FileName = $"blocked_websites_{DateTime.Now:yyyyMMdd}.txt"
        };
        
        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var websites = await _websiteBlockingService.ExportBlockListAsync();
                await File.WriteAllLinesAsync(saveFileDialog.FileName, websites);
                
                UpdateStatus($"Exported {websites.Count()} websites", Color.Green);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting website list");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
    private async Task LoadBlockedWebsites()
    {
        try
        {
            var websites = await _websiteBlockingService.GetBlockedWebsitesAsync();
            
            dgvBlockedWebsites.Rows.Clear();
            foreach (var website in websites)
            {
                dgvBlockedWebsites.Rows.Add(
                    website.Domain,
                    website.Category,
                    website.BlockedAt.ToString("yyyy-MM-dd HH:mm"),
                    website.IsActive ? "Active" : "Inactive"
                );
            }
            
            UpdateDashboardStats(websites.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading blocked websites");
        }
    }
    
    private void UpdateStatus(string message, Color color)
    {
        if (lblStatus != null)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }
    }
    
    private void UpdateDashboardStats(int blockedWebsitesCount)
    {
        // Update dashboard statistics
        var statsLabel = dashboardTab.Controls
            .OfType<Panel>().FirstOrDefault()?
            .Controls.OfType<GroupBox>()
            .FirstOrDefault(g => g.Text == "Protection Statistics")?
            .Controls.OfType<Label>()
            .FirstOrDefault(l => l.Text.StartsWith("Blocked Websites:"));
            
        if (statsLabel != null)
        {
            statsLabel.Text = $"Blocked Websites: {blockedWebsitesCount}";
        }
    }
}