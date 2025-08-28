using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChildGuard.Core.Data;
using ChildGuard.Core.Models;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI.Controls
{
    public class DashboardControl : UserControl
    {
        private readonly IEventRepository? _eventRepository;
        private bool _protectionActive;
        private Panel _headerPanel;
        private Panel _statsPanel;
        private Panel _chartsPanel;
        private Panel _recentEventsPanel;
        private System.Windows.Forms.Timer _refreshTimer;
        
        // Stats cards
        private StatCard _totalEventsCard;
        private StatCard _threatsCard;
        private StatCard _blockedProcessesCard;
        private StatCard _screenshotsCard;
        
        // Charts
        private ChartPanel _eventsChart;
        private ChartPanel _threatTypesChart;
        private ChartPanel _hourlyActivityChart;
        
        // Recent events list
        private ListView _recentEventsList;
        
        // Filter controls
        private ComboBox _timeRangeCombo;
        private Button _refreshButton;
        
        // Data
        private List<EventLog> _events = new List<EventLog>();
        private DateTime _startDate = DateTime.Now.AddDays(-7);
        private DateTime _endDate = DateTime.Now;

        public DashboardControl() : this(null)
        {
        }
        
        public DashboardControl(IEventRepository? eventRepository)
        {
            _eventRepository = eventRepository;
            InitializeComponent();
            LoadData();
            StartAutoRefresh();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            Size = new Size(1000, 700);
            BackColor = ColorScheme.Modern.BackgroundPrimary;
            AutoScroll = true;
            
            // Main container with TableLayoutPanel for better layout management
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                AutoScroll = true,
                Padding = new Padding(0),
                BackColor = ColorScheme.Modern.BackgroundPrimary
            };
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F)); // Header
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F)); // Stats
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 320F)); // Charts
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Recent Events
            
            // Header Panel
            _headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(20, 15, 20, 15)
            };

            // Layout: [Title][spacer][Actions(panel)]
            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var titleLabel = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Dock = DockStyle.Left
            };

            var actionsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Dock = DockStyle.Fill,
                WrapContents = false
            };

            _timeRangeCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Width = 150
            };
            _timeRangeCombo.Items.AddRange(new[] { "Last 24 Hours", "Last 7 Days", "Last 30 Days", "All Time" });
            _timeRangeCombo.SelectedIndex = 1;
            _timeRangeCombo.SelectedIndexChanged += (s, e) => { UpdateTimeRange(); LoadData(); };

            _refreshButton = new ModernButton
            {
                Text = "Refresh",
                Size = new Size(100, 35)
            };
            _refreshButton.Click += (s, e) => LoadData();

            actionsPanel.Controls.Add(_timeRangeCombo);
            actionsPanel.Controls.Add(_refreshButton);

            headerLayout.Controls.Add(titleLabel, 0, 0);
            headerLayout.Controls.Add(new Panel() { Dock = DockStyle.Fill }, 1, 0);
            headerLayout.Controls.Add(actionsPanel, 2, 0);

            _headerPanel.Controls.Add(headerLayout);

            // Stats Panel using TableLayoutPanel (4 columns)
            var statsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(20, 0, 20, 10),
                ColumnCount = 4,
                RowCount = 1
            };
            statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            _statsPanel = statsLayout;

            _totalEventsCard = new StatCard { Title = "Total Events", Value = "0", Icon = "▣", BackgroundColor = ColorScheme.Modern.Primary, Dock = DockStyle.Fill, Margin = new Padding(0,10,20,10) };
            _threatsCard = new StatCard { Title = "Threats Detected", Value = "0", Icon = "▲", BackgroundColor = Color.FromArgb(220, 53, 69), Dock = DockStyle.Fill, Margin = new Padding(0,10,20,10) };
            _blockedProcessesCard = new StatCard { Title = "Blocked Processes", Value = "0", Icon = "■", BackgroundColor = Color.FromArgb(255, 152, 0), Dock = DockStyle.Fill, Margin = new Padding(0,10,20,10) };
            _screenshotsCard = new StatCard { Title = "Screenshots", Value = "0", Icon = "◉", BackgroundColor = Color.FromArgb(76, 175, 80), Dock = DockStyle.Fill, Margin = new Padding(0,10,0,10) };

            statsLayout.Controls.Add(_totalEventsCard, 0, 0);
            statsLayout.Controls.Add(_threatsCard, 1, 0);
            statsLayout.Controls.Add(_blockedProcessesCard, 2, 0);
            statsLayout.Controls.Add(_screenshotsCard, 3, 0);

            // Charts Panel using split layout (55% / 45%), right split 50/50
            var chartsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(20, 0, 20, 10),
                ColumnCount = 2,
                RowCount = 1
            };
            chartsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            chartsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

            var rightSplit = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            rightSplit.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            rightSplit.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            _chartsPanel = chartsLayout;

            _eventsChart = new ChartPanel { Title = "Events Over Time", ChartType = ChartType.Line, Dock = DockStyle.Fill };
            _threatTypesChart = new ChartPanel { Title = "Threat Types", ChartType = ChartType.Pie, Dock = DockStyle.Fill };
            _hourlyActivityChart = new ChartPanel { Title = "Hourly Activity", ChartType = ChartType.Bar, Dock = DockStyle.Fill };

            chartsLayout.Controls.Add(_eventsChart, 0, 0);
            rightSplit.Controls.Add(_threatTypesChart, 0, 0);
            rightSplit.Controls.Add(_hourlyActivityChart, 0, 1);
            chartsLayout.Controls.Add(rightSplit, 1, 0);

            // Recent Events Panel
            _recentEventsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorScheme.Modern.BackgroundPrimary,
                Padding = new Padding(20, 0, 20, 20)
            };

            var recentLabel = new Label
            {
                Text = "Recent Events",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 10)
            };

            _recentEventsList = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                BackColor = ColorScheme.Modern.BackgroundSecondary,
                ForeColor = ColorScheme.Modern.TextPrimary,
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 40),
                Size = new Size(940, 180),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            _recentEventsList.Columns.AddRange(new[] {
                new ColumnHeader { Text = "Time", Width = 150 },
                new ColumnHeader { Text = "Type", Width = 120 },
                new ColumnHeader { Text = "Severity", Width = 100 },
                new ColumnHeader { Text = "Title", Width = 300 },
                new ColumnHeader { Text = "Source", Width = 150 }
            });

            // Auto-resize columns on control resize
            _recentEventsList.Resize += (s, e) =>
            {
                if (_recentEventsList.Columns.Count < 5) return;
                int w = _recentEventsList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 10;
                if (w <= 300) return;
                _recentEventsList.Columns[0].Width = (int)(w * 0.16); // Time
                _recentEventsList.Columns[1].Width = (int)(w * 0.12); // Type
                _recentEventsList.Columns[2].Width = (int)(w * 0.10); // Severity
                _recentEventsList.Columns[3].Width = (int)(w * 0.42); // Title
                _recentEventsList.Columns[4].Width = w - (_recentEventsList.Columns[0].Width + _recentEventsList.Columns[1].Width + _recentEventsList.Columns[2].Width + _recentEventsList.Columns[3].Width); // Source
            };

            _recentEventsPanel.Controls.AddRange(new Control[] { recentLabel, _recentEventsList });

            // Add panels to TableLayoutPanel in correct order
            mainContainer.Controls.Add(_headerPanel, 0, 0);
            mainContainer.Controls.Add(_statsPanel, 0, 1);
            mainContainer.Controls.Add(_chartsPanel, 0, 2);
            mainContainer.Controls.Add(_recentEventsPanel, 0, 3);
            
            // Add main container to control
            Controls.Add(mainContainer);
            this.ResumeLayout(false);
        }

        private void UpdateTimeRange()
        {
            _endDate = DateTime.Now;
            switch (_timeRangeCombo.SelectedIndex)
            {
                case 0: // Last 24 Hours
                    _startDate = DateTime.Now.AddDays(-1);
                    break;
                case 1: // Last 7 Days
                    _startDate = DateTime.Now.AddDays(-7);
                    break;
                case 2: // Last 30 Days
                    _startDate = DateTime.Now.AddDays(-30);
                    break;
                case 3: // All Time
                    _startDate = DateTime.MinValue;
                    break;
            }
        }

        private async void LoadData()
        {
            try
            {
                if (_eventRepository != null)
                {
                // Load events from database
                _events = (await _eventRepository.GetEventsAsync(_startDate, _endDate)).ToList();
                }
                else
                {
                    // Use mock data when repository is not available
                    _events = GenerateMockEvents();
                }
                
                // Update stats
                UpdateStats();
                
                // Update charts
                UpdateCharts();
                
                // Update recent events list
                UpdateRecentEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStats()
        {
            _totalEventsCard.Value = _events.Count.ToString();
            
            var threats = _events.Where(e => 
                e.Type == EventType.BadWordDetected || 
                e.Type == EventType.UrlThreat || 
                e.Type == EventType.ProcessBlocked).ToList();
            _threatsCard.Value = threats.Count.ToString();
            
            var blockedProcesses = _events.Where(e => e.Type == EventType.ProcessBlocked).ToList();
            _blockedProcessesCard.Value = blockedProcesses.Count.ToString();
            
            var screenshots = _events.Where(e => e.Type == EventType.ScreenshotCaptured).ToList();
            _screenshotsCard.Value = screenshots.Count.ToString();
        }

        private void UpdateCharts()
        {
            // Events over time
            var eventsByDay = _events
                .GroupBy(e => e.TimestampUtc.Date)
                .Select(g => new ChartDataPoint 
                { 
                    Label = g.Key.ToString("MM/dd"), 
                    Value = g.Count() 
                })
                .OrderBy(d => d.Label)
                .ToList();
            _eventsChart.SetData(eventsByDay);

            // Threat types
            var threatTypes = _events
                .GroupBy(e => e.Type)
                .Select(g => new ChartDataPoint 
                { 
                    Label = g.Key.ToString(), 
                    Value = g.Count() 
                })
                .OrderByDescending(d => d.Value)
                .Take(5)
                .ToList();
            _threatTypesChart.SetData(threatTypes);

            // Hourly activity
            var hourlyActivity = _events
                .Where(e => e.TimestampUtc.Date == DateTime.Today)
                .GroupBy(e => e.TimestampUtc.Hour)
                .Select(g => new ChartDataPoint 
                { 
                    Label = $"{g.Key}:00", 
                    Value = g.Count() 
                })
                .OrderBy(d => d.Label)
                .ToList();
            _hourlyActivityChart.SetData(hourlyActivity);
        }

        private void UpdateRecentEvents()
        {
            _recentEventsList.Items.Clear();
            
            var recentEvents = _events
                .OrderByDescending(e => e.TimestampUtc)
                .Take(50)
                .ToList();

            foreach (var evt in recentEvents)
            {
                var item = new ListViewItem(evt.TimestampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                item.SubItems.Add(evt.Type.ToString());
                item.SubItems.Add(evt.Severity.ToString());
                item.SubItems.Add(evt.Title);
                item.SubItems.Add(evt.Source);
                
                // Color code by severity
                switch (evt.Severity)
                {
                    case EventSeverity.High:
                        item.ForeColor = Color.FromArgb(220, 53, 69);
                        break;
                    case EventSeverity.Medium:
                        item.ForeColor = Color.FromArgb(255, 152, 0);
                        break;
                    default:
                        item.ForeColor = ColorScheme.Modern.TextPrimary;
                        break;
                }
                
                _recentEventsList.Items.Add(item);
            }
        }

        private void StartAutoRefresh()
        {
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 30000; // Refresh every 30 seconds
            _refreshTimer.Tick += (s, e) => LoadData();
            _refreshTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
        
        // Public methods for integration with main form
        public void UpdateProtectionStatus(bool isRunning)
        {
            _protectionActive = isRunning;
            
            // Update UI elements based on protection status
            if (_threatsCard != null)
            {
                _threatsCard.BackgroundColor = isRunning ? 
                    Color.FromArgb(220, 53, 69) : 
                    Color.FromArgb(128, 128, 128);
            }
        }
        
        public async Task RefreshDataAsync()
        {
            await Task.Run(() =>
            {
                BeginInvoke(new Action(() => LoadData()));
            });
        }
        
        // Generate mock events for testing without database
        private List<EventLog> GenerateMockEvents()
        {
            var random = new Random();
            var events = new List<EventLog>();
            var startDate = DateTime.Now.AddDays(-7);
            
            for (int i = 0; i < 50; i++)
            {
                events.Add(new EventLog
                {
                    Id = i + 1,  // Use integer ID instead of Guid
                    TimestampUtc = startDate.AddHours(random.Next(0, 168)),
                    Type = (EventType)random.Next(0, 5),
                    Severity = (EventSeverity)random.Next(0, 3),
                    Title = $"Event {i + 1}",
                    Source = "System",
                    Content = $"This is a mock event for testing purposes."
                });
            }
            
            return events;
        }
    }

    // Stat Card Control
    public class StatCard : Panel
    {
        private Label _titleLabel;
        private Label _valueLabel;
        private Label _iconLabel;
        
        public string Title
        {
            get => _titleLabel?.Text ?? "";
            set { if (_titleLabel != null) _titleLabel.Text = value; }
        }
        
        public string Value
        {
            get => _valueLabel?.Text ?? "";
            set { if (_valueLabel != null) _valueLabel.Text = value; }
        }
        
        public string Icon
        {
            get => _iconLabel?.Text ?? "";
            set { if (_iconLabel != null) _iconLabel.Text = value; }
        }
        
        public Color BackgroundColor
        {
            get => BackColor;
            set => BackColor = value;
        }

        public StatCard()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            BackColor = ColorScheme.Modern.Primary;
            ForeColor = Color.White;
            Padding = new Padding(10);
            
            // Icon positioned at top-right
            _iconLabel = new Label
            {
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Size = new Size(40, 40),
                Location = new Point(this.Width - 50, 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            
            // Title at top-left
            _titleLabel = new Label
            {
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 255, 255, 255),
                Location = new Point(10, 10),
                Size = new Size(120, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            // Value below title
            _valueLabel = new Label
            {
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                Location = new Point(10, 35),
                Size = new Size(120, 40),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            Controls.AddRange(new Control[] { _iconLabel, _titleLabel, _valueLabel });
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Add rounded corners
            using (var path = new GraphicsPath())
            {
                int radius = 10;
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(Width - radius * 2, Height - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(0, Height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();
                
                Region = new Region(path);
            }
        }
    }

    // Simple Chart Panel
    public class ChartPanel : Panel
    {
        private Label _titleLabel;
        private List<ChartDataPoint> _data = new List<ChartDataPoint>();
        
        public string Title
        {
            get => _titleLabel?.Text ?? "";
            set { if (_titleLabel != null) _titleLabel.Text = value; }
        }
        
        public ChartType ChartType { get; set; } = ChartType.Bar;

        public ChartPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            BackColor = ColorScheme.Modern.BackgroundSecondary;
            ForeColor = ColorScheme.Modern.TextPrimary;
            Padding = new Padding(15);
            
            _titleLabel = new Label
            {
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 15)
            };
            
            Controls.Add(_titleLabel);
        }

        public void SetData(List<ChartDataPoint> data)
        {
            _data = data ?? new List<ChartDataPoint>();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Draw rounded rectangle background
            using (var path = new GraphicsPath())
            {
                int radius = 10;
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(Width - radius * 2, Height - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(0, Height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();
                
                using (var brush = new SolidBrush(BackColor))
                {
                    g.FillPath(brush, path);
                }
            }
            
            if (_data.Count == 0) return;
            
            // Chart drawing area
            var chartArea = new Rectangle(20, 50, Width - 40, Height - 80);
            
            switch (ChartType)
            {
                case ChartType.Bar:
                    DrawBarChart(g, chartArea);
                    break;
                case ChartType.Line:
                    DrawLineChart(g, chartArea);
                    break;
                case ChartType.Pie:
                    DrawPieChart(g, chartArea);
                    break;
            }
        }

        private void DrawBarChart(Graphics g, Rectangle area)
        {
            if (_data.Count == 0) return;
            
            var maxValue = _data.Max(d => d.Value);
            if (maxValue == 0) maxValue = 1;
            
            var barWidth = area.Width / (_data.Count * 2);
            var spacing = barWidth;
            
            for (int i = 0; i < _data.Count; i++)
            {
                var data = _data[i];
                var barHeight = (int)(area.Height * data.Value / maxValue);
                var x = area.X + i * (barWidth + spacing);
                var y = area.Bottom - barHeight;
                
                using (var brush = new SolidBrush(ColorScheme.Modern.Primary))
                {
                    g.FillRectangle(brush, x, y, barWidth, barHeight);
                }
                
                // Draw label
                using (var font = new Font("Segoe UI", 8))
                using (var brush = new SolidBrush(ColorScheme.Modern.TextSecondary))
                {
                    var labelSize = g.MeasureString(data.Label, font);
                    g.DrawString(data.Label, font, brush, 
                        x + barWidth / 2 - labelSize.Width / 2, 
                        area.Bottom + 5);
                }
            }
        }

        private void DrawLineChart(Graphics g, Rectangle area)
        {
            if (_data.Count < 2) return;
            
            var maxValue = _data.Max(d => d.Value);
            if (maxValue == 0) maxValue = 1;
            
            var points = new List<PointF>();
            var xStep = area.Width / (_data.Count - 1);
            
            for (int i = 0; i < _data.Count; i++)
            {
                var x = area.X + i * xStep;
                var y = area.Bottom - (area.Height * _data[i].Value / maxValue);
                points.Add(new PointF(x, y));
            }
            
            // Draw line
            using (var pen = new Pen(ColorScheme.Modern.Primary, 2))
            {
                if (points.Count > 1)
                    g.DrawLines(pen, points.ToArray());
            }
            
            // Draw points
            foreach (var point in points)
            {
                using (var brush = new SolidBrush(ColorScheme.Modern.Primary))
                {
                    g.FillEllipse(brush, point.X - 4, point.Y - 4, 8, 8);
                }
            }
        }

        private void DrawPieChart(Graphics g, Rectangle area)
        {
            if (_data.Count == 0) return;
            
            var total = _data.Sum(d => d.Value);
            if (total == 0) return;
            
            var colors = new[] {
                ColorScheme.Modern.Primary,
                Color.FromArgb(220, 53, 69),
                Color.FromArgb(255, 152, 0),
                Color.FromArgb(76, 175, 80),
                Color.FromArgb(156, 39, 176)
            };
            
            float startAngle = -90;
            var size = Math.Min(area.Width, area.Height);
            var pieRect = new Rectangle(
                area.X + (area.Width - size) / 2,
                area.Y + (area.Height - size) / 2,
                size, size
            );
            
            for (int i = 0; i < _data.Count && i < colors.Length; i++)
            {
                var sweepAngle = 360f * _data[i].Value / total;
                
                using (var brush = new SolidBrush(colors[i % colors.Length]))
                {
                    g.FillPie(brush, pieRect, startAngle, sweepAngle);
                }
                
                startAngle += sweepAngle;
            }
        }
    }

    public enum ChartType
    {
        Bar,
        Line,
        Pie
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = "";
        public float Value { get; set; }
    }
}
