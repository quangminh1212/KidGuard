using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ChildGuard.UI.Theming;
using Newtonsoft.Json;

namespace ChildGuard.UI.Controls
{
    public class SettingsControl : UserControl
    {
        private TabControl _tabControl;
        private Dictionary<string, object> _settings;
        private string _settingsFilePath;
        private ModernButton _saveButton;
        private ModernButton _resetButton;
        private Label _statusLabel;
        
        // Event for settings changes
        public event Action<Dictionary<string, object>>? OnSettingsChanged;

        public event EventHandler? SettingsChanged;

        public SettingsControl()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            Size = new Size(1000, 700);
            BackColor = ColorScheme.Modern.BackgroundPrimary;

            // Header
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(20, 15, 20, 15)
            };

            var titleLabel = new Label
            {
                Text = "Settings",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 15)
            };

            headerPanel.Controls.Add(titleLabel);

            // Footer with buttons
            var footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(20, 10, 20, 10)
            };

            _saveButton = new ModernButton
            {
                Text = "Save Settings",
                Size = new Size(120, 35),
                Location = new Point(Width - 280, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _saveButton.Click += SaveButton_Click;

            _resetButton = new ModernButton
            {
                Text = "Reset to Default",
                Size = new Size(120, 35),
                Location = new Point(Width - 150, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(220, 53, 69)
            };
            _resetButton.Click += ResetButton_Click;

            _statusLabel = new Label
            {
                Text = "Ready",
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorScheme.Modern.TextSecondary,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            footerPanel.Controls.AddRange(new Control[] { _statusLabel, _saveButton, _resetButton });

            // Tab Control
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BackColor = ColorScheme.Modern.BackgroundPrimary
            };

            // Create tabs
            CreateGeneralTab();
            CreateProtectionTab();
            CreateMonitoringTab();
            CreateNotificationsTab();
            CreateAdvancedTab();

            // Add controls to form
            Controls.Add(_tabControl);
            Controls.Add(footerPanel);
            Controls.Add(headerPanel);
        }

        private void CreateGeneralTab()
        {
            var tabPage = new TabPage("General")
            {
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(20)
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false
            };

            // Application Settings
            panel.Controls.Add(CreateSectionHeader("Application Settings"));
            
            var startWithWindowsCheck = CreateCheckBox("Start with Windows", "StartWithWindows");
            panel.Controls.Add(startWithWindowsCheck);

            var minimizeToTrayCheck = CreateCheckBox("Minimize to system tray", "MinimizeToTray");
            panel.Controls.Add(minimizeToTrayCheck);

            var autoUpdateCheck = CreateCheckBox("Enable automatic updates", "AutoUpdate");
            panel.Controls.Add(autoUpdateCheck);

            // Data Settings
            panel.Controls.Add(CreateSectionHeader("Data Settings"));

            var dataRetentionCombo = CreateComboBox("Data retention period:", "DataRetention",
                new[] { "7 days", "14 days", "30 days", "60 days", "90 days", "Forever" });
            panel.Controls.Add(dataRetentionCombo);

            var autoCleanupCheck = CreateCheckBox("Automatic cleanup old data", "AutoCleanup");
            panel.Controls.Add(autoCleanupCheck);

            var maxStorageInput = CreateNumberInput("Maximum storage (MB):", "MaxStorage", 1000, 1, 10000);
            panel.Controls.Add(maxStorageInput);

            tabPage.Controls.Add(panel);
            _tabControl.TabPages.Add(tabPage);
        }

        private void CreateProtectionTab()
        {
            var tabPage = new TabPage("Protection")
            {
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(20)
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false
            };

            // Content Filtering
            panel.Controls.Add(CreateSectionHeader("Content Filtering"));

            var enableBadWordsCheck = CreateCheckBox("Enable bad words detection", "EnableBadWords");
            panel.Controls.Add(enableBadWordsCheck);

            var badWordsSensitivitySlider = CreateSlider("Detection sensitivity:", "BadWordsSensitivity", 1, 5, 3);
            panel.Controls.Add(badWordsSensitivitySlider);

            var badWordsListButton = CreateButton("Manage Bad Words List", () => ShowBadWordsList());
            panel.Controls.Add(badWordsListButton);

            // URL Protection
            panel.Controls.Add(CreateSectionHeader("URL Protection"));

            var enableUrlCheckCheck = CreateCheckBox("Enable URL safety checking", "EnableUrlCheck");
            panel.Controls.Add(enableUrlCheckCheck);

            var blockUnsafeUrlsCheck = CreateCheckBox("Automatically block unsafe URLs", "BlockUnsafeUrls");
            panel.Controls.Add(blockUnsafeUrlsCheck);

            var safeBrowsingApiKeyInput = CreateTextInput("Google Safe Browsing API Key:", "SafeBrowsingApiKey", true);
            panel.Controls.Add(safeBrowsingApiKeyInput);

            var blockedDomainsButton = CreateButton("Manage Blocked Domains", () => ShowBlockedDomainsList());
            panel.Controls.Add(blockedDomainsButton);

            // Process Protection
            panel.Controls.Add(CreateSectionHeader("Process Protection"));

            var enableProcessMonitorCheck = CreateCheckBox("Enable process monitoring", "EnableProcessMonitor");
            panel.Controls.Add(enableProcessMonitorCheck);

            var autoKillBlockedCheck = CreateCheckBox("Automatically terminate blocked processes", "AutoKillBlocked");
            panel.Controls.Add(autoKillBlockedCheck);

            var blockedProcessesButton = CreateButton("Manage Blocked Processes", () => ShowBlockedProcessesList());
            panel.Controls.Add(blockedProcessesButton);

            tabPage.Controls.Add(panel);
            _tabControl.TabPages.Add(tabPage);
        }

        private void CreateMonitoringTab()
        {
            var tabPage = new TabPage("Monitoring")
            {
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(20)
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false
            };

            // Keyboard & Mouse
            panel.Controls.Add(CreateSectionHeader("Keyboard & Mouse"));

            var enableKeyboardCheck = CreateCheckBox("Monitor keyboard input", "EnableKeyboard");
            panel.Controls.Add(enableKeyboardCheck);

            var enableMouseCheck = CreateCheckBox("Monitor mouse activity", "EnableMouse");
            panel.Controls.Add(enableMouseCheck);

            var logKeystrokesCheck = CreateCheckBox("Log keystrokes (privacy warning)", "LogKeystrokes");
            logKeystrokesCheck.ForeColor = Color.FromArgb(255, 152, 0);
            panel.Controls.Add(logKeystrokesCheck);

            // Audio Monitoring
            panel.Controls.Add(CreateSectionHeader("Audio Monitoring"));

            var enableAudioCheck = CreateCheckBox("Enable audio monitoring", "EnableAudio");
            panel.Controls.Add(enableAudioCheck);

            var audioOnThreatCheck = CreateCheckBox("Record audio on threat detection", "AudioOnThreat");
            panel.Controls.Add(audioOnThreatCheck);

            var audioDurationInput = CreateNumberInput("Recording duration (seconds):", "AudioDuration", 10, 5, 60);
            panel.Controls.Add(audioDurationInput);

            // Screenshot Capture
            panel.Controls.Add(CreateSectionHeader("Screenshot Capture"));

            var enableScreenshotCheck = CreateCheckBox("Enable screenshot capture", "EnableScreenshot");
            panel.Controls.Add(enableScreenshotCheck);

            var screenshotOnThreatCheck = CreateCheckBox("Capture screenshot on threat", "ScreenshotOnThreat");
            panel.Controls.Add(screenshotOnThreatCheck);

            var periodicScreenshotCheck = CreateCheckBox("Enable periodic screenshots", "PeriodicScreenshot");
            panel.Controls.Add(periodicScreenshotCheck);

            var screenshotIntervalInput = CreateNumberInput("Screenshot interval (minutes):", "ScreenshotInterval", 5, 1, 60);
            panel.Controls.Add(screenshotIntervalInput);

            tabPage.Controls.Add(panel);
            _tabControl.TabPages.Add(tabPage);
        }

        private void CreateNotificationsTab()
        {
            var tabPage = new TabPage("Notifications")
            {
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(20)
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false
            };

            // Notification Settings
            panel.Controls.Add(CreateSectionHeader("Notification Settings"));

            var enableNotificationsCheck = CreateCheckBox("Enable notifications", "EnableNotifications");
            panel.Controls.Add(enableNotificationsCheck);

            var soundAlertsCheck = CreateCheckBox("Play sound alerts", "SoundAlerts");
            panel.Controls.Add(soundAlertsCheck);

            var desktopNotificationsCheck = CreateCheckBox("Show desktop notifications", "DesktopNotifications");
            panel.Controls.Add(desktopNotificationsCheck);

            // Threat Levels
            panel.Controls.Add(CreateSectionHeader("Alert Levels"));

            var notifyHighCheck = CreateCheckBox("Alert on high severity threats", "NotifyHigh");
            notifyHighCheck.Checked = true;
            panel.Controls.Add(notifyHighCheck);

            var notifyMediumCheck = CreateCheckBox("Alert on medium severity threats", "NotifyMedium");
            panel.Controls.Add(notifyMediumCheck);

            var notifyLowCheck = CreateCheckBox("Alert on low severity threats", "NotifyLow");
            panel.Controls.Add(notifyLowCheck);

            // Quiet Hours
            panel.Controls.Add(CreateSectionHeader("Quiet Hours"));

            var enableQuietHoursCheck = CreateCheckBox("Enable quiet hours", "EnableQuietHours");
            panel.Controls.Add(enableQuietHoursCheck);

            var quietStartInput = CreateTimeInput("Quiet hours start:", "QuietStart", "22:00");
            panel.Controls.Add(quietStartInput);

            var quietEndInput = CreateTimeInput("Quiet hours end:", "QuietEnd", "07:00");
            panel.Controls.Add(quietEndInput);

            tabPage.Controls.Add(panel);
            _tabControl.TabPages.Add(tabPage);
        }

        private void CreateAdvancedTab()
        {
            var tabPage = new TabPage("Advanced")
            {
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(20)
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false
            };

            // Performance
            panel.Controls.Add(CreateSectionHeader("Performance"));

            var cpuLimitInput = CreateNumberInput("CPU usage limit (%):", "CpuLimit", 50, 10, 100);
            panel.Controls.Add(cpuLimitInput);

            var memoryLimitInput = CreateNumberInput("Memory limit (MB):", "MemoryLimit", 500, 100, 2000);
            panel.Controls.Add(memoryLimitInput);

            var lowPriorityCheck = CreateCheckBox("Run in low priority mode", "LowPriority");
            panel.Controls.Add(lowPriorityCheck);

            // Logging
            panel.Controls.Add(CreateSectionHeader("Logging"));

            var enableDebugLogCheck = CreateCheckBox("Enable debug logging", "EnableDebugLog");
            panel.Controls.Add(enableDebugLogCheck);

            var logLevelCombo = CreateComboBox("Log level:", "LogLevel",
                new[] { "Error", "Warning", "Info", "Debug", "Verbose" });
            panel.Controls.Add(logLevelCombo);

            var maxLogSizeInput = CreateNumberInput("Max log file size (MB):", "MaxLogSize", 100, 10, 1000);
            panel.Controls.Add(maxLogSizeInput);

            var viewLogsButton = CreateButton("View Logs", () => ViewLogs());
            panel.Controls.Add(viewLogsButton);

            // Database
            panel.Controls.Add(CreateSectionHeader("Database"));

            var compactDbButton = CreateButton("Compact Database", () => CompactDatabase());
            panel.Controls.Add(compactDbButton);

            var exportDataButton = CreateButton("Export Data", () => ExportData());
            panel.Controls.Add(exportDataButton);

            var importDataButton = CreateButton("Import Data", () => ImportData());
            panel.Controls.Add(importDataButton);

            tabPage.Controls.Add(panel);
            _tabControl.TabPages.Add(tabPage);
        }

        // Helper methods to create controls
        private Label CreateSectionHeader(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 5)
            };
        }

        private CheckBox CreateCheckBox(string text, string settingKey)
        {
            var checkBox = new CheckBox
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Padding = new Padding(20, 5, 0, 5),
                Checked = GetSetting<bool>(settingKey)
            };
            
            checkBox.CheckedChanged += (s, e) => SetSetting(settingKey, checkBox.Checked);
            
            return checkBox;
        }

        private Panel CreateTextInput(string label, string settingKey, bool isPassword = false)
        {
            var panel = new Panel
            {
                Height = 60,
                Width = 400,
                Padding = new Padding(20, 5, 0, 5)
            };

            var labelControl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var textBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Width = 350,
                Location = new Point(0, 25),
                Text = GetSetting<string>(settingKey) ?? "",
                UseSystemPasswordChar = isPassword
            };
            
            textBox.TextChanged += (s, e) => SetSetting(settingKey, textBox.Text);

            panel.Controls.Add(labelControl);
            panel.Controls.Add(textBox);

            return panel;
        }

        private Panel CreateNumberInput(string label, string settingKey, int defaultValue, int min, int max)
        {
            var panel = new Panel
            {
                Height = 60,
                Width = 400,
                Padding = new Padding(20, 5, 0, 5)
            };

            var labelControl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var numericUpDown = new NumericUpDown
            {
                Font = new Font("Segoe UI", 10),
                Width = 100,
                Location = new Point(0, 25),
                Minimum = min,
                Maximum = max,
                Value = GetSetting<int>(settingKey, defaultValue)
            };
            
            numericUpDown.ValueChanged += (s, e) => SetSetting(settingKey, (int)numericUpDown.Value);

            panel.Controls.Add(labelControl);
            panel.Controls.Add(numericUpDown);

            return panel;
        }

        private Panel CreateComboBox(string label, string settingKey, string[] items)
        {
            var panel = new Panel
            {
                Height = 60,
                Width = 400,
                Padding = new Padding(20, 5, 0, 5)
            };

            var labelControl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var comboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Width = 200,
                Location = new Point(0, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboBox.Items.AddRange(items);
            
            var currentValue = GetSetting<string>(settingKey);
            if (currentValue != null && comboBox.Items.Contains(currentValue))
            {
                comboBox.SelectedItem = currentValue;
            }
            else if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
            
            comboBox.SelectedIndexChanged += (s, e) => SetSetting(settingKey, comboBox.SelectedItem?.ToString());

            panel.Controls.Add(labelControl);
            panel.Controls.Add(comboBox);

            return panel;
        }

        private Panel CreateSlider(string label, string settingKey, int min, int max, int defaultValue)
        {
            var panel = new Panel
            {
                Height = 80,
                Width = 400,
                Padding = new Padding(20, 5, 0, 5)
            };

            var value = GetSetting<int>(settingKey, defaultValue);
            
            var labelControl = new Label
            {
                Text = $"{label} {value}",
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var trackBar = new TrackBar
            {
                Width = 350,
                Location = new Point(0, 25),
                Minimum = min,
                Maximum = max,
                Value = value,
                TickStyle = TickStyle.None
            };
            
            trackBar.ValueChanged += (s, e) =>
            {
                SetSetting(settingKey, trackBar.Value);
                labelControl.Text = $"{label} {trackBar.Value}";
            };

            panel.Controls.Add(labelControl);
            panel.Controls.Add(trackBar);

            return panel;
        }

        private Panel CreateTimeInput(string label, string settingKey, string defaultValue)
        {
            var panel = new Panel
            {
                Height = 60,
                Width = 400,
                Padding = new Padding(20, 5, 0, 5)
            };

            var labelControl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var dateTimePicker = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10),
                Width = 100,
                Location = new Point(0, 25),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true
            };
            
            var currentValue = GetSetting<string>(settingKey, defaultValue);
            if (DateTime.TryParse(currentValue, out var time))
            {
                dateTimePicker.Value = time;
            }
            
            dateTimePicker.ValueChanged += (s, e) => 
                SetSetting(settingKey, dateTimePicker.Value.ToString("HH:mm"));

            panel.Controls.Add(labelControl);
            panel.Controls.Add(dateTimePicker);

            return panel;
        }

        private Button CreateButton(string text, Action onClick)
        {
            var button = new ModernButton
            {
                Text = text,
                Size = new Size(200, 35),
                Margin = new Padding(20, 5, 0, 5)
            };
            button.Click += (s, e) => onClick();
            return button;
        }

        // Settings management
        private void LoadSettings()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingsDir = Path.Combine(appDataPath, "ChildGuard");
            Directory.CreateDirectory(settingsDir);
            _settingsFilePath = Path.Combine(settingsDir, "settings.json");

            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    _settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json) 
                        ?? new Dictionary<string, object>();
                }
                catch
                {
                    _settings = new Dictionary<string, object>();
                }
            }
            else
            {
                _settings = GetDefaultSettings();
                SaveSettings();
            }
        }

        private Dictionary<string, object> GetDefaultSettings()
        {
            return new Dictionary<string, object>
            {
                ["StartWithWindows"] = false,
                ["MinimizeToTray"] = true,
                ["AutoUpdate"] = true,
                ["DataRetention"] = "30 days",
                ["AutoCleanup"] = true,
                ["MaxStorage"] = 1000,
                ["EnableBadWords"] = true,
                ["BadWordsSensitivity"] = 3,
                ["EnableUrlCheck"] = true,
                ["BlockUnsafeUrls"] = true,
                ["EnableProcessMonitor"] = true,
                ["AutoKillBlocked"] = true,
                ["EnableKeyboard"] = true,
                ["EnableMouse"] = true,
                ["LogKeystrokes"] = false,
                ["EnableAudio"] = true,
                ["AudioOnThreat"] = true,
                ["AudioDuration"] = 10,
                ["EnableScreenshot"] = true,
                ["ScreenshotOnThreat"] = true,
                ["PeriodicScreenshot"] = false,
                ["ScreenshotInterval"] = 5,
                ["EnableNotifications"] = true,
                ["SoundAlerts"] = true,
                ["DesktopNotifications"] = true,
                ["NotifyHigh"] = true,
                ["NotifyMedium"] = true,
                ["NotifyLow"] = false,
                ["EnableQuietHours"] = false,
                ["QuietStart"] = "22:00",
                ["QuietEnd"] = "07:00",
                ["CpuLimit"] = 50,
                ["MemoryLimit"] = 500,
                ["LowPriority"] = false,
                ["EnableDebugLog"] = false,
                ["LogLevel"] = "Info",
                ["MaxLogSize"] = 100
            };
        }

        private T GetSetting<T>(string key, T defaultValue = default!)
        {
            if (_settings.ContainsKey(key))
            {
                var value = _settings[key];
                if (value is T typedValue)
                    return typedValue;
                
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        private void SetSetting(string key, object value)
        {
            _settings[key] = value;
            _statusLabel.Text = "Settings modified (unsaved)";
            _statusLabel.ForeColor = Color.FromArgb(255, 152, 0);
        }

        private void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_settingsFilePath, json);
                _statusLabel.Text = "Settings saved successfully";
                _statusLabel.ForeColor = Color.FromArgb(76, 175, 80);
                
                // Trigger both events
                SettingsChanged?.Invoke(this, EventArgs.Empty);
                OnSettingsChanged?.Invoke(_settings);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            SaveSettings();
        }

        private void ResetButton_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to reset all settings to default?",
                "Reset Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            
            if (result == DialogResult.Yes)
            {
                _settings = GetDefaultSettings();
                SaveSettings();
                
                // Reload UI
                Controls.Clear();
                InitializeComponent();
                
                _statusLabel.Text = "Settings reset to default";
                _statusLabel.ForeColor = Color.FromArgb(76, 175, 80);
            }
        }

        // Dialog methods
        private void ShowBadWordsList()
        {
            // TODO: Implement bad words list dialog
            MessageBox.Show("Bad Words List Manager - Coming Soon", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowBlockedDomainsList()
        {
            // TODO: Implement blocked domains list dialog
            MessageBox.Show("Blocked Domains Manager - Coming Soon", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowBlockedProcessesList()
        {
            // TODO: Implement blocked processes list dialog
            MessageBox.Show("Blocked Processes Manager - Coming Soon", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ViewLogs()
        {
            // TODO: Implement log viewer
            MessageBox.Show("Log Viewer - Coming Soon", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CompactDatabase()
        {
            // TODO: Implement database compaction
            MessageBox.Show("Database compaction started", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportData()
        {
            // TODO: Implement data export
            MessageBox.Show("Data Export - Coming Soon", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ImportData()
        {
            // TODO: Implement data import
            MessageBox.Show("Data Import - Coming Soon", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public Dictionary<string, object> GetAllSettings()
        {
            return new Dictionary<string, object>(_settings);
        }
    }
}
