using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using ChildGuard.Core.Models;
using ChildGuard.Core.Data;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI.Controls
{
    public class AttachmentsManager : UserControl
    {
        private readonly IEventRepository? _eventRepository;
        private readonly string _attachmentsPath;
        
        // UI Components - với proper initialization
        private Panel _headerPanel = null!;
        private Panel _filterPanel = null!;
        private ListView _fileListView = null!;
        private Panel _previewPanel = null!;
        private PictureBox _imagePreview = null!;
        private Panel _audioPlayer = null!;
        private Label _fileInfoLabel = null!;
        private Panel _actionPanel = null!;
        
        // Filter controls - với proper initialization
        private ComboBox _typeFilter = null!;
        private DateTimePicker _dateFromPicker = null!;
        private DateTimePicker _dateToPicker = null!;
        private TextBox _searchBox = null!;
        private ModernButton _searchButton = null!;
        
        // Action buttons - với proper initialization
        private ModernButton _openButton = null!;
        private ModernButton _deleteButton = null!;
        private ModernButton _exportButton = null!;
        private ModernButton _deleteAllButton = null!;
        
        // Data
        private readonly List<AttachmentInfo> _attachments = new List<AttachmentInfo>();
        private AttachmentInfo? _selectedAttachment;
        
        // Dispose tracking
        private bool _disposed = false;
        
        // File watcher cho real-time updates
        private FileSystemWatcher? _fileWatcher;
        
        // Cache cho thumbnails
        private readonly Dictionary<string, Image> _thumbnailCache = new Dictionary<string, Image>();
        
        public AttachmentsManager(IEventRepository? eventRepository = null)
        {
            _eventRepository = eventRepository;
            
            // Xác định thư mục lưu attachments
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _attachmentsPath = Path.Combine(appDataPath, "ChildGuard", "Attachments");
            
            InitializeComponent();
            LoadAttachments();
        }
        
        private void InitializeComponent()
        {
            Size = new Size(1000, 700);
            BackColor = ColorScheme.Modern.BackgroundPrimary;
            
            // Header Panel
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(20, 15, 20, 15)
            };
            
            var titleLabel = new Label
            {
                Text = "File Attachments Manager",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 15)
            };
            
            var statsLabel = new Label
            {
                Text = "Loading...",
                Name = "statsLabel",
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorScheme.Modern.TextSecondary,
                AutoSize = true,
                Location = new Point(Width - 200, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            
            _headerPanel.Controls.AddRange(new Control[] { titleLabel, statsLabel });
            
            // Filter Panel
            _filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = ColorScheme.Modern.BackgroundSecondary,
                Padding = new Padding(20, 10, 20, 10)
            };
            
            // Type filter
            var typeLabel = new Label
            {
                Text = "Type:",
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Location = new Point(20, 15),
                AutoSize = true
            };
            
            _typeFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Width = 120,
                Location = new Point(60, 12)
            };
            _typeFilter.Items.AddRange(new[] { "All", "Screenshots", "Audio", "Documents" });
            _typeFilter.SelectedIndex = 0;
            _typeFilter.SelectedIndexChanged += (s, e) => ApplyFilters();
            
            // Date range
            var fromLabel = new Label
            {
                Text = "From:",
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Location = new Point(200, 15),
                AutoSize = true
            };
            
            _dateFromPicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10),
                Width = 100,
                Location = new Point(245, 12),
                Value = DateTime.Now.AddDays(-7)
            };
            _dateFromPicker.ValueChanged += (s, e) => ApplyFilters();
            
            var toLabel = new Label
            {
                Text = "To:",
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Location = new Point(360, 15),
                AutoSize = true
            };
            
            _dateToPicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10),
                Width = 100,
                Location = new Point(385, 12),
                Value = DateTime.Now
            };
            _dateToPicker.ValueChanged += (s, e) => ApplyFilters();
            
            // Search box
            _searchBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Width = 200,
                Location = new Point(500, 12)
            };
            _searchBox.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) ApplyFilters(); };
            
            _searchButton = new ModernButton
            {
                Text = "Search",
                Size = new Size(80, 30),
                Location = new Point(710, 10)
            };
            _searchButton.Click += (s, e) => ApplyFilters();
            
            // Clear filters button
            var clearButton = new ModernButton
            {
                Text = "Clear",
                Size = new Size(60, 30),
                Location = new Point(800, 10),
                Style = ModernButton.ButtonStyle.Secondary
            };
            clearButton.Click += (s, e) => ClearFilters();
            
            _filterPanel.Controls.AddRange(new Control[] { 
                typeLabel, _typeFilter, fromLabel, _dateFromPicker, 
                toLabel, _dateToPicker, _searchBox, _searchButton, clearButton 
            });
            
            // Main content - Splitter container
            var splitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 500
            };
            
            // Left panel - File list
            var listPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            
            _fileListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                ForeColor = ColorScheme.Modern.TextPrimary,
                Font = new Font("Segoe UI", 10)
            };
            
            _fileListView.Columns.AddRange(new[] {
                new ColumnHeader { Text = "Name", Width = 200 },
                new ColumnHeader { Text = "Type", Width = 80 },
                new ColumnHeader { Text = "Size", Width = 80 },
                new ColumnHeader { Text = "Date", Width = 120 },
                new ColumnHeader { Text = "Event", Width = 150 }
            });
            
            _fileListView.SelectedIndexChanged += OnFileSelected;
            _fileListView.DoubleClick += (s, e) => OpenSelectedFile();
            
            listPanel.Controls.Add(_fileListView);
            splitter.Panel1.Controls.Add(listPanel);
            
            // Right panel - Preview and actions
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            
            // File info
            _fileInfoLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 60,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Text = "Select a file to preview",
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = ColorScheme.Modern.BackgroundSecondary,
                Padding = new Padding(10)
            };
            
            // Preview panel
            _previewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorScheme.Modern.BackgroundSecondary,
                Padding = new Padding(10)
            };
            
            _imagePreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Visible = false
            };
            
            _audioPlayer = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false
            };
            
            var audioLabel = new Label
            {
                Text = "🔊 Audio File",
                Font = new Font("Segoe UI Emoji", 48),
                ForeColor = ColorScheme.Modern.TextSecondary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _audioPlayer.Controls.Add(audioLabel);
            
            _previewPanel.Controls.AddRange(new Control[] { _imagePreview, _audioPlayer });
            
            // Action panel
            _actionPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(10)
            };
            
            _openButton = new ModernButton
            {
                Text = "Open",
                Size = new Size(80, 35),
                Location = new Point(10, 12),
                Enabled = false
            };
            _openButton.Click += (s, e) => OpenSelectedFile();
            
            _deleteButton = new ModernButton
            {
                Text = "Delete",
                Size = new Size(80, 35),
                Location = new Point(100, 12),
                Style = ModernButton.ButtonStyle.Danger,
                Enabled = false
            };
            _deleteButton.Click += (s, e) => DeleteSelectedFile();
            
            _exportButton = new ModernButton
            {
                Text = "Export",
                Size = new Size(80, 35),
                Location = new Point(190, 12),
                Style = ModernButton.ButtonStyle.Secondary,
                Enabled = false
            };
            _exportButton.Click += (s, e) => ExportSelectedFile();
            
            _deleteAllButton = new ModernButton
            {
                Text = "Delete All",
                Size = new Size(100, 35),
                Location = new Point(Width - 120, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Style = ModernButton.ButtonStyle.Danger
            };
            _deleteAllButton.Click += (s, e) => DeleteAllFiles();
            
            _actionPanel.Controls.AddRange(new Control[] { 
                _openButton, _deleteButton, _exportButton, _deleteAllButton 
            });
            
            rightPanel.Controls.AddRange(new Control[] { _fileInfoLabel, _previewPanel, _actionPanel });
            splitter.Panel2.Controls.Add(rightPanel);
            
            // Add all to form
            Controls.Add(splitter);
            Controls.Add(_filterPanel);
            Controls.Add(_headerPanel);
        }
        
        private void LoadAttachments()
        {
            _attachments.Clear();
            
            if (Directory.Exists(_attachmentsPath))
            {
                // Load screenshots
                var screenshotsPath = Path.Combine(_attachmentsPath, "Screenshots");
                if (Directory.Exists(screenshotsPath))
                {
                    foreach (var file in Directory.GetFiles(screenshotsPath, "*.png"))
                    {
                        var info = new FileInfo(file);
                        _attachments.Add(new AttachmentInfo
                        {
                            FileName = info.Name,
                            FilePath = info.FullName,
                            FileType = "Screenshot",
                            FileSize = info.Length,
                            CreatedAt = info.CreationTime,
                            EventType = "Screenshot Capture"
                        });
                    }
                }
                
                // Load audio files
                var audioPath = Path.Combine(_attachmentsPath, "Audio");
                if (Directory.Exists(audioPath))
                {
                    foreach (var file in Directory.GetFiles(audioPath, "*.wav"))
                    {
                        var info = new FileInfo(file);
                        _attachments.Add(new AttachmentInfo
                        {
                            FileName = info.Name,
                            FilePath = info.FullName,
                            FileType = "Audio",
                            FileSize = info.Length,
                            CreatedAt = info.CreationTime,
                            EventType = "Audio Recording"
                        });
                    }
                }
            }
            
            ApplyFilters();
            UpdateStats();
        }
        
        private void ApplyFilters()
        {
            var filtered = _attachments.AsEnumerable();
            
            // Filter by type
            if (_typeFilter.SelectedIndex > 0)
            {
                var selectedType = _typeFilter.SelectedItem.ToString();
                if (selectedType == "Screenshots")
                    filtered = filtered.Where(a => a.FileType == "Screenshot");
                else if (selectedType == "Audio")
                    filtered = filtered.Where(a => a.FileType == "Audio");
            }
            
            // Filter by date range
            filtered = filtered.Where(a => 
                a.CreatedAt >= _dateFromPicker.Value.Date &&
                a.CreatedAt <= _dateToPicker.Value.Date.AddDays(1));
            
            // Filter by search text
            if (!string.IsNullOrWhiteSpace(_searchBox.Text))
            {
                var searchText = _searchBox.Text.ToLower();
                filtered = filtered.Where(a => 
                    a.FileName.ToLower().Contains(searchText) ||
                    a.EventType.ToLower().Contains(searchText));
            }
            
            // Update list view
            _fileListView.Items.Clear();
            foreach (var attachment in filtered.OrderByDescending(a => a.CreatedAt))
            {
                var item = new ListViewItem(attachment.FileName);
                item.SubItems.Add(attachment.FileType);
                item.SubItems.Add(FormatFileSize(attachment.FileSize));
                item.SubItems.Add(attachment.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                item.SubItems.Add(attachment.EventType);
                item.Tag = attachment;
                
                // Color code by type
                if (attachment.FileType == "Screenshot")
                    item.ForeColor = Color.FromArgb(76, 175, 80);
                else if (attachment.FileType == "Audio")
                    item.ForeColor = Color.FromArgb(33, 150, 243);
                
                _fileListView.Items.Add(item);
            }
            
            UpdateStats();
        }
        
        private void ClearFilters()
        {
            _typeFilter.SelectedIndex = 0;
            _dateFromPicker.Value = DateTime.Now.AddDays(-7);
            _dateToPicker.Value = DateTime.Now;
            _searchBox.Text = "";
            ApplyFilters();
        }
        
        private void OnFileSelected(object? sender, EventArgs e)
        {
            if (_fileListView.SelectedItems.Count > 0)
            {
                _selectedAttachment = _fileListView.SelectedItems[0].Tag as AttachmentInfo;
                if (_selectedAttachment != null)
                {
                    ShowPreview(_selectedAttachment);
                    _openButton.Enabled = true;
                    _deleteButton.Enabled = true;
                    _exportButton.Enabled = true;
                }
            }
            else
            {
                _selectedAttachment = null;
                ClearPreview();
                _openButton.Enabled = false;
                _deleteButton.Enabled = false;
                _exportButton.Enabled = false;
            }
        }
        
        private void ShowPreview(AttachmentInfo attachment)
        {
            _fileInfoLabel.Text = $"{attachment.FileName}\n{attachment.FileType} - {FormatFileSize(attachment.FileSize)} - {attachment.CreatedAt:yyyy-MM-dd HH:mm:ss}";
            
            // Clear previous preview
            _imagePreview.Visible = false;
            _audioPlayer.Visible = false;
            _imagePreview.Image?.Dispose();
            _imagePreview.Image = null;
            
            // Show appropriate preview
            if (attachment.FileType == "Screenshot" && File.Exists(attachment.FilePath))
            {
                try
                {
                    using (var stream = File.OpenRead(attachment.FilePath))
                    {
                        _imagePreview.Image = Image.FromStream(stream);
                    }
                    _imagePreview.Visible = true;
                }
                catch
                {
                    _fileInfoLabel.Text += "\n[Preview not available]";
                }
            }
            else if (attachment.FileType == "Audio")
            {
                _audioPlayer.Visible = true;
            }
        }
        
        private void ClearPreview()
        {
            _fileInfoLabel.Text = "Select a file to preview";
            _imagePreview.Visible = false;
            _audioPlayer.Visible = false;
            _imagePreview.Image?.Dispose();
            _imagePreview.Image = null;
        }
        
        private void OpenSelectedFile()
        {
            if (_selectedAttachment != null && File.Exists(_selectedAttachment.FilePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _selectedAttachment.FilePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Cannot open file: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private void DeleteSelectedFile()
        {
            if (_selectedAttachment == null) return;
            
            var result = MessageBox.Show(
                $"Are you sure you want to delete '{_selectedAttachment.FileName}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    if (File.Exists(_selectedAttachment.FilePath))
                    {
                        File.Delete(_selectedAttachment.FilePath);
                    }
                    
                    _attachments.Remove(_selectedAttachment);
                    ClearPreview();
                    ApplyFilters();
                    
                    MessageBox.Show("File deleted successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting file: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private void ExportSelectedFile()
        {
            if (_selectedAttachment == null) return;
            
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.FileName = _selectedAttachment.FileName;
                saveDialog.Filter = _selectedAttachment.FileType == "Screenshot" 
                    ? "PNG files (*.png)|*.png" 
                    : "WAV files (*.wav)|*.wav";
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.Copy(_selectedAttachment.FilePath, saveDialog.FileName, true);
                        MessageBox.Show("File exported successfully.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting file: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        private void DeleteAllFiles()
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete ALL {_fileListView.Items.Count} files?\nThis action cannot be undone!",
                "Confirm Delete All",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    int deleted = 0;
                    foreach (var attachment in _attachments.ToList())
                    {
                        if (File.Exists(attachment.FilePath))
                        {
                            File.Delete(attachment.FilePath);
                            deleted++;
                        }
                    }
                    
                    _attachments.Clear();
                    ClearPreview();
                    ApplyFilters();
                    
                    MessageBox.Show($"{deleted} files deleted successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting files: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private void UpdateStats()
        {
            var statsLabel = Controls.Find("statsLabel", true).FirstOrDefault() as Label;
            if (statsLabel != null)
            {
                var totalSize = _attachments.Sum(a => a.FileSize);
                var screenshotCount = _attachments.Count(a => a.FileType == "Screenshot");
                var audioCount = _attachments.Count(a => a.FileType == "Audio");
                
                statsLabel.Text = $"Total: {_attachments.Count} files ({FormatFileSize(totalSize)}) | " +
                                  $"Screenshots: {screenshotCount} | Audio: {audioCount}";
            }
        }
        
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
        
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _imagePreview?.Image?.Dispose();
                    
                    // Dispose file watcher
                    _fileWatcher?.Dispose();
                    
                    // Clear thumbnail cache
                    foreach (var thumbnail in _thumbnailCache.Values)
                    {
                        thumbnail?.Dispose();
                    }
                    _thumbnailCache.Clear();
                    
                    // Remove event handlers để tránh memory leaks
                    if (_typeFilter != null) _typeFilter.SelectedIndexChanged -= (s, e) => ApplyFilters();
                    if (_dateFromPicker != null) _dateFromPicker.ValueChanged -= (s, e) => ApplyFilters();
                    if (_dateToPicker != null) _dateToPicker.ValueChanged -= (s, e) => ApplyFilters();
                    if (_searchButton != null) _searchButton.Click -= (s, e) => ApplyFilters();
                    if (_fileListView != null)
                    {
                        _fileListView.SelectedIndexChanged -= OnFileSelected;
                        _fileListView.DoubleClick -= (s, e) => OpenSelectedFile();
                    }
                    
                    // Dispose controls
                    _headerPanel?.Dispose();
                    _filterPanel?.Dispose();
                    _fileListView?.Dispose();
                    _previewPanel?.Dispose();
                    _imagePreview?.Dispose();
                    _audioPlayer?.Dispose();
                    _actionPanel?.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
    
    // Model để lưu thông tin attachment
    public class AttachmentInfo
    {
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string FileType { get; set; } = "";
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public string EventType { get; set; } = "";
    }
}
