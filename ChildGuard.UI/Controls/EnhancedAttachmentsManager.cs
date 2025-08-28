using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChildGuard.Core.Models;
using ChildGuard.Core.Data;
using ChildGuard.UI.Theming;
using System.Diagnostics;

namespace ChildGuard.UI.Controls
{
    /// <summary>
    /// Phiên bản nâng cao của AttachmentsManager với async operations và performance optimizations
    /// </summary>
    public class EnhancedAttachmentsManager : UserControl
    {
        private readonly IEventRepository? _eventRepository;
        private readonly string _attachmentsPath;
        
        // UI Components
        private Panel _headerPanel = null!;
        private Panel _filterPanel = null!;
        private ListView _fileListView = null!;
        private Panel _previewPanel = null!;
        private PictureBox _imagePreview = null!;
        private Panel _audioPlayer = null!;
        private Label _fileInfoLabel = null!;
        private Panel _actionPanel = null!;
        private ProgressBar _loadingProgress = null!;
        private Label _loadingLabel = null!;
        
        // Filter controls
        private ComboBox _typeFilter = null!;
        private DateTimePicker _dateFromPicker = null!;
        private DateTimePicker _dateToPicker = null!;
        private TextBox _searchBox = null!;
        private ModernButton _searchButton = null!;
        
        // Action buttons
        private ModernButton _openButton = null!;
        private ModernButton _deleteButton = null!;
        private ModernButton _exportButton = null!;
        private ModernButton _deleteAllButton = null!;
        private ModernButton _refreshButton = null!;
        
        // Data
        private readonly List<AttachmentInfo> _attachments = new List<AttachmentInfo>();
        private AttachmentInfo? _selectedAttachment;
        
        // Threading & Async
        private CancellationTokenSource? _loadCancellation;
        private readonly SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1, 1);
        
        // Caching
        private readonly Dictionary<string, Image> _thumbnailCache = new Dictionary<string, Image>();
        private const int THUMBNAIL_SIZE = 256;
        private const int MAX_CACHE_SIZE = 50; // Max số thumbnails trong cache
        
        // File Watcher
        private FileSystemWatcher? _fileWatcher;
        
        // Virtualization
        private readonly List<AttachmentInfo> _visibleAttachments = new List<AttachmentInfo>();
        private const int ITEMS_PER_PAGE = 100;
        private int _currentPage = 0;
        
        // Dispose tracking
        private bool _disposed = false;
        
        public EnhancedAttachmentsManager(IEventRepository? eventRepository = null)
        {
            _eventRepository = eventRepository;
            
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _attachmentsPath = Path.Combine(appDataPath, "ChildGuard", "Attachments");
            
            InitializeComponent();
            InitializeFileWatcher();
            _ = LoadAttachmentsAsync(); // Fire and forget initial load
        }
        
        private void InitializeComponent()
        {
            Size = new Size(1000, 700);
            BackColor = ColorScheme.Modern.BackgroundPrimary;
            
            // Create loading overlay
            CreateLoadingOverlay();
            
            // Header Panel với Refresh button
            CreateHeaderPanel();
            
            // Filter Panel
            CreateFilterPanel();
            
            // Main content với virtualized ListView
            CreateMainContent();
        }
        
        private void CreateLoadingOverlay()
        {
            _loadingProgress = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                Size = new Size(300, 23),
                Visible = false
            };
            
            _loadingLabel = new Label
            {
                Text = "Loading attachments...",
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Visible = false
            };
        }
        
        private void CreateHeaderPanel()
        {
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(20, 15, 20, 15)
            };
            
            var titleLabel = new Label
            {
                Text = "Enhanced File Attachments Manager",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 15)
            };
            
            _refreshButton = new ModernButton
            {
                Text = "↻ Refresh",
                Size = new Size(90, 30),
                Location = new Point(Width - 320, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _refreshButton.Click += async (s, e) => await RefreshAsync();
            
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
            
            _headerPanel.Controls.AddRange(new Control[] { titleLabel, _refreshButton, statsLabel });
        }
        
        private void CreateFilterPanel()
        {
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
            _typeFilter.SelectedIndexChanged += async (s, e) => await ApplyFiltersAsync();
            
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
            _dateFromPicker.ValueChanged += async (s, e) => await ApplyFiltersAsync();
            
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
            _dateToPicker.ValueChanged += async (s, e) => await ApplyFiltersAsync();
            
            // Search box với debouncing
            _searchBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Width = 200,
                Location = new Point(500, 12)
            };
            
            var searchTimer = new System.Windows.Forms.Timer { Interval = 500 }; // 500ms debounce
            searchTimer.Tick += async (s, e) =>
            {
                searchTimer.Stop();
                await ApplyFiltersAsync();
            };
            
            _searchBox.TextChanged += (s, e) =>
            {
                searchTimer.Stop();
                searchTimer.Start();
            };
            
            _searchButton = new ModernButton
            {
                Text = "Search",
                Size = new Size(80, 30),
                Location = new Point(710, 10)
            };
            _searchButton.Click += async (s, e) => await ApplyFiltersAsync();
            
            var clearButton = new ModernButton
            {
                Text = "Clear",
                Size = new Size(60, 30),
                Location = new Point(800, 10),
                Style = ModernButton.ButtonStyle.Secondary
            };
            clearButton.Click += async (s, e) => await ClearFiltersAsync();
            
            _filterPanel.Controls.AddRange(new Control[] { 
                typeLabel, _typeFilter, fromLabel, _dateFromPicker, 
                toLabel, _dateToPicker, _searchBox, _searchButton, clearButton 
            });
        }
        
        private void CreateMainContent()
        {
            var splitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 500
            };
            
            // Left panel - Virtualized File list
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
                Font = new Font("Segoe UI", 10),
                VirtualMode = false // Sẽ enable sau khi có data
            };
            
            _fileListView.Columns.AddRange(new[] {
                new ColumnHeader { Text = "Name", Width = 200 },
                new ColumnHeader { Text = "Type", Width = 80 },
                new ColumnHeader { Text = "Size", Width = 80 },
                new ColumnHeader { Text = "Date", Width = 120 },
                new ColumnHeader { Text = "Event", Width = 150 }
            });
            
            _fileListView.SelectedIndexChanged += async (s, e) => await OnFileSelectedAsync(s, e);
            _fileListView.DoubleClick += async (s, e) => await OpenSelectedFileAsync();
            
            // Add loading overlay to list panel
            _loadingProgress.Location = new Point((listPanel.Width - 300) / 2, (listPanel.Height - 23) / 2);
            _loadingLabel.Location = new Point(_loadingProgress.Location.X, _loadingProgress.Location.Y - 25);
            
            listPanel.Controls.Add(_fileListView);
            listPanel.Controls.Add(_loadingProgress);
            listPanel.Controls.Add(_loadingLabel);
            
            splitter.Panel1.Controls.Add(listPanel);
            
            // Right panel - Preview với lazy loading
            CreatePreviewPanel(splitter);
            
            Controls.Add(splitter);
            Controls.Add(_filterPanel);
            Controls.Add(_headerPanel);
        }
        
        private void CreatePreviewPanel(SplitContainer splitter)
        {
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            
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
            
            _actionPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(10)
            };
            
            CreateActionButtons();
            
            rightPanel.Controls.AddRange(new Control[] { _fileInfoLabel, _previewPanel, _actionPanel });
            splitter.Panel2.Controls.Add(rightPanel);
        }
        
        private void CreateActionButtons()
        {
            _openButton = new ModernButton
            {
                Text = "Open",
                Size = new Size(80, 35),
                Location = new Point(10, 12),
                Enabled = false
            };
            _openButton.Click += async (s, e) => await OpenSelectedFileAsync();
            
            _deleteButton = new ModernButton
            {
                Text = "Delete",
                Size = new Size(80, 35),
                Location = new Point(100, 12),
                Style = ModernButton.ButtonStyle.Danger,
                Enabled = false
            };
            _deleteButton.Click += async (s, e) => await DeleteSelectedFileAsync();
            
            _exportButton = new ModernButton
            {
                Text = "Export",
                Size = new Size(80, 35),
                Location = new Point(190, 12),
                Style = ModernButton.ButtonStyle.Secondary,
                Enabled = false
            };
            _exportButton.Click += async (s, e) => await ExportSelectedFileAsync();
            
            _deleteAllButton = new ModernButton
            {
                Text = "Delete All",
                Size = new Size(100, 35),
                Location = new Point(Width - 120, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Style = ModernButton.ButtonStyle.Danger
            };
            _deleteAllButton.Click += async (s, e) => await DeleteAllFilesAsync();
            
            _actionPanel.Controls.AddRange(new Control[] { 
                _openButton, _deleteButton, _exportButton, _deleteAllButton 
            });
        }
        
        private void InitializeFileWatcher()
        {
            if (Directory.Exists(_attachmentsPath))
            {
                _fileWatcher = new FileSystemWatcher(_attachmentsPath)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.CreationTime,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true
                };
                
                _fileWatcher.Created += async (s, e) => await OnFileSystemChanged(e.FullPath, FileSystemChangeType.Created);
                _fileWatcher.Deleted += async (s, e) => await OnFileSystemChanged(e.FullPath, FileSystemChangeType.Deleted);
                _fileWatcher.Changed += async (s, e) => await OnFileSystemChanged(e.FullPath, FileSystemChangeType.Changed);
            }
        }
        
        private async Task OnFileSystemChanged(string path, FileSystemChangeType changeType)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(async () => await OnFileSystemChanged(path, changeType)));
                return;
            }
            
            // Debounce rapid changes
            await Task.Delay(100);
            
            switch (changeType)
            {
                case FileSystemChangeType.Created:
                    await AddNewFileAsync(path);
                    break;
                case FileSystemChangeType.Deleted:
                    RemoveFile(path);
                    break;
                case FileSystemChangeType.Changed:
                    await UpdateFileAsync(path);
                    break;
            }
            
            await ApplyFiltersAsync();
        }
        
        private async Task LoadAttachmentsAsync()
        {
            await _loadSemaphore.WaitAsync();
            try
            {
                _loadCancellation?.Cancel();
                _loadCancellation = new CancellationTokenSource();
                var token = _loadCancellation.Token;
                
                ShowLoading(true);
                _attachments.Clear();
                
                await Task.Run(async () =>
                {
                    if (!Directory.Exists(_attachmentsPath)) return;
                    
                    var tasks = new List<Task<List<AttachmentInfo>>>();
                    
                    // Parallel loading của screenshots và audio
                    tasks.Add(LoadFilesAsync(Path.Combine(_attachmentsPath, "Screenshots"), "*.png", "Screenshot", token));
                    tasks.Add(LoadFilesAsync(Path.Combine(_attachmentsPath, "Audio"), "*.wav", "Audio", token));
                    
                    var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                    
                    foreach (var result in results)
                    {
                        _attachments.AddRange(result);
                    }
                }, token);
                
                if (!token.IsCancellationRequested)
                {
                    await ApplyFiltersAsync();
                }
            }
            finally
            {
                ShowLoading(false);
                _loadSemaphore.Release();
            }
        }
        
        private async Task<List<AttachmentInfo>> LoadFilesAsync(string path, string pattern, string fileType, CancellationToken token)
        {
            var attachments = new List<AttachmentInfo>();
            
            if (!Directory.Exists(path)) return attachments;
            
            var files = Directory.GetFiles(path, pattern);
            
            // Process files in parallel batches
            var batchSize = 10;
            for (int i = 0; i < files.Length; i += batchSize)
            {
                if (token.IsCancellationRequested) break;
                
                var batch = files.Skip(i).Take(batchSize);
                var tasks = batch.Select(file => Task.Run(() =>
                {
                    var info = new FileInfo(file);
                    return new AttachmentInfo
                    {
                        FileName = info.Name,
                        FilePath = info.FullName,
                        FileType = fileType,
                        FileSize = info.Length,
                        CreatedAt = info.CreationTime,
                        EventType = fileType == "Screenshot" ? "Screenshot Capture" : "Audio Recording"
                    };
                }, token));
                
                var batchResults = await Task.WhenAll(tasks).ConfigureAwait(false);
                attachments.AddRange(batchResults);
            }
            
            return attachments;
        }
        
        private async Task ApplyFiltersAsync()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(async () => await ApplyFiltersAsync()));
                return;
            }
            
            await Task.Run(() =>
            {
                var filtered = _attachments.AsParallel();
                
                // Filter by type
                if (_typeFilter.SelectedIndex > 0)
                {
                    var selectedType = _typeFilter.SelectedItem?.ToString();
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
                
                _visibleAttachments.Clear();
                _visibleAttachments.AddRange(filtered.OrderByDescending(a => a.CreatedAt));
            });
            
            UpdateListView();
            UpdateStats();
        }
        
        private void UpdateListView()
        {
            _fileListView.BeginUpdate();
            _fileListView.Items.Clear();
            
            // Virtualization - chỉ load items hiện tại
            var itemsToShow = _visibleAttachments.Take(ITEMS_PER_PAGE);
            
            foreach (var attachment in itemsToShow)
            {
                var item = new ListViewItem(attachment.FileName)
                {
                    Tag = attachment
                };
                item.SubItems.Add(attachment.FileType);
                item.SubItems.Add(FormatFileSize(attachment.FileSize));
                item.SubItems.Add(attachment.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                item.SubItems.Add(attachment.EventType);
                
                // Color coding
                if (attachment.FileType == "Screenshot")
                    item.ForeColor = Color.FromArgb(76, 175, 80);
                else if (attachment.FileType == "Audio")
                    item.ForeColor = Color.FromArgb(33, 150, 243);
                
                _fileListView.Items.Add(item);
            }
            
            _fileListView.EndUpdate();
        }
        
        private async Task OnFileSelectedAsync(object? sender, EventArgs e)
        {
            if (_fileListView.SelectedItems.Count > 0)
            {
                _selectedAttachment = _fileListView.SelectedItems[0].Tag as AttachmentInfo;
                if (_selectedAttachment != null)
                {
                    await ShowPreviewAsync(_selectedAttachment);
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
        
        private async Task ShowPreviewAsync(AttachmentInfo attachment)
        {
            _fileInfoLabel.Text = $"{attachment.FileName}\n{attachment.FileType} - {FormatFileSize(attachment.FileSize)} - {attachment.CreatedAt:yyyy-MM-dd HH:mm:ss}";
            
            // Clear previous preview
            _imagePreview.Visible = false;
            _audioPlayer.Visible = false;
            
            if (_imagePreview.Image != null && !_thumbnailCache.ContainsValue(_imagePreview.Image))
            {
                _imagePreview.Image.Dispose();
            }
            _imagePreview.Image = null;
            
            // Show appropriate preview
            if (attachment.FileType == "Screenshot" && File.Exists(attachment.FilePath))
            {
                try
                {
                    // Check cache first
                    if (_thumbnailCache.ContainsKey(attachment.FilePath))
                    {
                        _imagePreview.Image = _thumbnailCache[attachment.FilePath];
                    }
                    else
                    {
                        // Generate thumbnail async
                        var thumbnail = await GenerateThumbnailAsync(attachment.FilePath);
                        if (thumbnail != null)
                        {
                            CacheThumbnail(attachment.FilePath, thumbnail);
                            _imagePreview.Image = thumbnail;
                        }
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
        
        private async Task<Image?> GenerateThumbnailAsync(string imagePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    {
                        var ratio = Math.Min(
                            (float)THUMBNAIL_SIZE / originalImage.Width,
                            (float)THUMBNAIL_SIZE / originalImage.Height);
                        
                        var newWidth = (int)(originalImage.Width * ratio);
                        var newHeight = (int)(originalImage.Height * ratio);
                        
                        var thumbnail = new Bitmap(newWidth, newHeight);
                        using (var graphics = Graphics.FromImage(thumbnail))
                        {
                            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
                        }
                        
                        return thumbnail;
                    }
                }
                catch
                {
                    return null;
                }
            });
        }
        
        private void CacheThumbnail(string path, Image thumbnail)
        {
            // Implement LRU cache
            if (_thumbnailCache.Count >= MAX_CACHE_SIZE)
            {
                var oldest = _thumbnailCache.First();
                oldest.Value?.Dispose();
                _thumbnailCache.Remove(oldest.Key);
            }
            
            _thumbnailCache[path] = thumbnail;
        }
        
        private void ClearPreview()
        {
            _fileInfoLabel.Text = "Select a file to preview";
            _imagePreview.Visible = false;
            _audioPlayer.Visible = false;
            
            if (_imagePreview.Image != null && !_thumbnailCache.ContainsValue(_imagePreview.Image))
            {
                _imagePreview.Image.Dispose();
            }
            _imagePreview.Image = null;
        }
        
        private async Task OpenSelectedFileAsync()
        {
            if (_selectedAttachment == null || !File.Exists(_selectedAttachment.FilePath))
                return;
            
            await Task.Run(() =>
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
                    BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show($"Cannot open file: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            });
        }
        
        private async Task DeleteSelectedFileAsync()
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
                    await Task.Run(() =>
                    {
                        if (File.Exists(_selectedAttachment.FilePath))
                        {
                            // Secure delete - overwrite before deletion
                            SecureDelete(_selectedAttachment.FilePath);
                        }
                    });
                    
                    _attachments.Remove(_selectedAttachment);
                    
                    // Remove from cache
                    if (_thumbnailCache.ContainsKey(_selectedAttachment.FilePath))
                    {
                        _thumbnailCache[_selectedAttachment.FilePath]?.Dispose();
                        _thumbnailCache.Remove(_selectedAttachment.FilePath);
                    }
                    
                    ClearPreview();
                    await ApplyFiltersAsync();
                    
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
        
        private void SecureDelete(string filePath)
        {
            // Overwrite file with random data before deletion
            var random = new Random();
            var fileInfo = new FileInfo(filePath);
            var bufferSize = (int)Math.Min(fileInfo.Length, 4096);
            var buffer = new byte[bufferSize];
            
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
            {
                for (int i = 0; i < 3; i++) // 3 passes
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    while (stream.Position < stream.Length)
                    {
                        random.NextBytes(buffer);
                        stream.Write(buffer, 0, buffer.Length);
                    }
                    stream.Flush();
                }
            }
            
            File.Delete(filePath);
        }
        
        private async Task ExportSelectedFileAsync()
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
                        ShowLoading(true, "Exporting file...");
                        
                        await Task.Run(() =>
                        {
                            File.Copy(_selectedAttachment.FilePath, saveDialog.FileName, true);
                        });
                        
                        MessageBox.Show("File exported successfully.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting file: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        ShowLoading(false);
                    }
                }
            }
        }
        
        private async Task DeleteAllFilesAsync()
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete ALL {_fileListView.Items.Count} files?\nThis action cannot be undone!",
                "Confirm Delete All",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            
            if (result == DialogResult.Yes)
            {
                ShowLoading(true, "Deleting files...");
                
                try
                {
                    var deleted = 0;
                    var tasks = new List<Task>();
                    
                    foreach (var attachment in _attachments.ToList())
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            if (File.Exists(attachment.FilePath))
                            {
                                SecureDelete(attachment.FilePath);
                                Interlocked.Increment(ref deleted);
                            }
                        }));
                    }
                    
                    await Task.WhenAll(tasks);
                    
                    _attachments.Clear();
                    _thumbnailCache.Clear();
                    ClearPreview();
                    await ApplyFiltersAsync();
                    
                    MessageBox.Show($"{deleted} files deleted successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting files: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    ShowLoading(false);
                }
            }
        }
        
        private async Task ClearFiltersAsync()
        {
            _typeFilter.SelectedIndex = 0;
            _dateFromPicker.Value = DateTime.Now.AddDays(-7);
            _dateToPicker.Value = DateTime.Now;
            _searchBox.Text = "";
            await ApplyFiltersAsync();
        }
        
        private async Task RefreshAsync()
        {
            await LoadAttachmentsAsync();
        }
        
        private async Task AddNewFileAsync(string path)
        {
            await Task.Run(() =>
            {
                var info = new FileInfo(path);
                var fileType = Path.GetExtension(path).ToLower() == ".png" ? "Screenshot" : "Audio";
                
                _attachments.Add(new AttachmentInfo
                {
                    FileName = info.Name,
                    FilePath = info.FullName,
                    FileType = fileType,
                    FileSize = info.Length,
                    CreatedAt = info.CreationTime,
                    EventType = fileType == "Screenshot" ? "Screenshot Capture" : "Audio Recording"
                });
            });
        }
        
        private void RemoveFile(string path)
        {
            var attachment = _attachments.FirstOrDefault(a => a.FilePath == path);
            if (attachment != null)
            {
                _attachments.Remove(attachment);
                
                if (_thumbnailCache.ContainsKey(path))
                {
                    _thumbnailCache[path]?.Dispose();
                    _thumbnailCache.Remove(path);
                }
            }
        }
        
        private async Task UpdateFileAsync(string path)
        {
            var attachment = _attachments.FirstOrDefault(a => a.FilePath == path);
            if (attachment != null)
            {
                await Task.Run(() =>
                {
                    var info = new FileInfo(path);
                    attachment.FileSize = info.Length;
                    attachment.CreatedAt = info.CreationTime;
                });
                
                // Invalidate cache
                if (_thumbnailCache.ContainsKey(path))
                {
                    _thumbnailCache[path]?.Dispose();
                    _thumbnailCache.Remove(path);
                }
            }
        }
        
        private void UpdateStats()
        {
            var statsLabel = Controls.Find("statsLabel", true).FirstOrDefault() as Label;
            if (statsLabel != null)
            {
                var totalSize = _visibleAttachments.Sum(a => a.FileSize);
                var screenshotCount = _visibleAttachments.Count(a => a.FileType == "Screenshot");
                var audioCount = _visibleAttachments.Count(a => a.FileType == "Audio");
                
                statsLabel.Text = $"Showing: {_fileListView.Items.Count}/{_visibleAttachments.Count} | " +
                                  $"Total: {FormatFileSize(totalSize)} | " +
                                  $"Screenshots: {screenshotCount} | Audio: {audioCount}";
            }
        }
        
        private void ShowLoading(bool show, string message = "Loading...")
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ShowLoading(show, message)));
                return;
            }
            
            _loadingLabel.Text = message;
            _loadingLabel.Visible = show;
            _loadingProgress.Visible = show;
            _loadingLabel.BringToFront();
            _loadingProgress.BringToFront();
            
            // Disable controls during loading
            _filterPanel.Enabled = !show;
            _fileListView.Enabled = !show;
            _actionPanel.Enabled = !show;
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
                    // Cancel ongoing operations
                    _loadCancellation?.Cancel();
                    _loadCancellation?.Dispose();
                    
                    // Dispose semaphore
                    _loadSemaphore?.Dispose();
                    
                    // Dispose file watcher
                    if (_fileWatcher != null)
                    {
                        _fileWatcher.EnableRaisingEvents = false;
                        _fileWatcher.Dispose();
                    }
                    
                    // Clear and dispose thumbnail cache
                    foreach (var thumbnail in _thumbnailCache.Values)
                    {
                        thumbnail?.Dispose();
                    }
                    _thumbnailCache.Clear();
                    
                    // Dispose image preview
                    _imagePreview?.Image?.Dispose();
                    
                    // Dispose all controls
                    _headerPanel?.Dispose();
                    _filterPanel?.Dispose();
                    _fileListView?.Dispose();
                    _previewPanel?.Dispose();
                    _imagePreview?.Dispose();
                    _audioPlayer?.Dispose();
                    _actionPanel?.Dispose();
                    _loadingProgress?.Dispose();
                    _loadingLabel?.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
        
        private enum FileSystemChangeType
        {
            Created,
            Deleted,
            Changed
        }
    }
}
