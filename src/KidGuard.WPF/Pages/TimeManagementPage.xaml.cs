using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace KidGuard.WPF.Pages
{
    /// <summary>
    /// Interaction logic for TimeManagementPage.xaml
    /// </summary>
    public partial class TimeManagementPage : Page
    {
        private DispatcherTimer _updateTimer;
        private TimeSpan _totalTimeToday;
        private TimeSpan _timeLimit;
        private DateTime _sessionStartTime;

        public TimeManagementPage()
        {
            InitializeComponent();
            InitializeTimer();
            LoadTimeSettings();
            UpdateTimeDisplay();
        }

        private void InitializeTimer()
        {
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void LoadTimeSettings()
        {
            // TODO: Load from service
            _totalTimeToday = TimeSpan.FromHours(4.5);
            _timeLimit = TimeSpan.FromHours(6);
            _sessionStartTime = DateTime.Now.AddHours(-2);
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateTimeDisplay();
        }

        private void UpdateTimeDisplay()
        {
            // Update current session time
            var currentSessionTime = DateTime.Now - _sessionStartTime;
            _totalTimeToday = TimeSpan.FromHours(4.5) + currentSessionTime;

            // Update percentage
            var percentUsed = Math.Min(100, (_totalTimeToday.TotalMinutes / _timeLimit.TotalMinutes) * 100);
            
            // Calculate remaining time
            var remainingTime = _timeLimit - _totalTimeToday;
            if (remainingTime < TimeSpan.Zero)
            {
                remainingTime = TimeSpan.Zero;
            }

            // Animate progress if needed
            if (percentUsed >= 90)
            {
                StartWarningAnimation();
            }
        }

        private void StartWarningAnimation()
        {
            // Create pulsing animation for warning
            var storyboard = new Storyboard();
            var colorAnimation = new ColorAnimation
            {
                From = Colors.Orange,
                To = Colors.Red,
                Duration = new Duration(TimeSpan.FromSeconds(1)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("(TextBlock.Foreground).(SolidColorBrush.Color)"));
            storyboard.Children.Add(colorAnimation);
        }

        private void OnAddTimeClick(object sender, RoutedEventArgs e)
        {
            // Show confirmation dialog
            var result = MessageBox.Show(
                "Bạn có chắc muốn thêm 30 phút vào giới hạn thời gian hôm nay?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _timeLimit = _timeLimit.Add(TimeSpan.FromMinutes(30));
                UpdateTimeDisplay();
                ShowNotification("Đã thêm 30 phút vào giới hạn thời gian");
            }
        }

        private void OnBreakTimeSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                var minutes = (int)slider.Value;
                // TODO: Update break time setting
                ShowNotification($"Thời gian nghỉ sau mỗi {minutes} phút");
            }
        }

        private void OnScheduleToggleChanged(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Primitives.ToggleButton toggle)
            {
                var isEnabled = toggle.IsChecked ?? false;
                // TODO: Update schedule setting
                ShowNotification(isEnabled ? "Đã bật lịch sử dụng" : "Đã tắt lịch sử dụng");
            }
        }

        private void OnTimeLimitTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Validate and update time limit
                if (TryParseTimeInput(textBox.Text, out var hours))
                {
                    // TODO: Update time limit for specific day
                    textBox.BorderBrush = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    textBox.BorderBrush = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private bool TryParseTimeInput(string input, out double hours)
        {
            hours = 0;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Remove 'h' suffix if present
            input = input.Replace("h", "").Trim();

            if (double.TryParse(input, out hours))
            {
                return hours >= 0 && hours <= 24;
            }

            return false;
        }

        private void OnAddRestrictionClick(object sender, RoutedEventArgs e)
        {
            // TODO: Show dialog to add new restriction
            var dialog = new AddRestrictionDialog();
            if (dialog.ShowDialog() == true)
            {
                // Add new restriction
                ShowNotification("Đã thêm hạn chế mới");
            }
        }

        private void ShowNotification(string message)
        {
            // TODO: Implement notification display
            Console.WriteLine($"Notification: {message}");
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            _updateTimer?.Stop();
        }
    }

    // Simple dialog for adding restrictions (placeholder)
    public class AddRestrictionDialog : Window
    {
        public AddRestrictionDialog()
        {
            Title = "Thêm hạn chế thời gian";
            Width = 400;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var title = new TextBlock
            {
                Text = "Thêm hạn chế thời gian mới",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(20, 20, 20, 10)
            };
            Grid.SetRow(title, 0);
            grid.Children.Add(title);

            var content = new StackPanel
            {
                Margin = new Thickness(20)
            };
            
            content.Children.Add(new TextBlock { Text = "Tên hạn chế:", Margin = new Thickness(0, 0, 0, 5) });
            content.Children.Add(new TextBox { Margin = new Thickness(0, 0, 0, 15) });
            
            content.Children.Add(new TextBlock { Text = "Thời gian bắt đầu:", Margin = new Thickness(0, 0, 0, 5) });
            content.Children.Add(new TextBox { Margin = new Thickness(0, 0, 0, 15) });
            
            content.Children.Add(new TextBlock { Text = "Thời gian kết thúc:", Margin = new Thickness(0, 0, 0, 5) });
            content.Children.Add(new TextBox { Margin = new Thickness(0, 0, 0, 15) });

            Grid.SetRow(content, 1);
            grid.Children.Add(content);

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(20)
            };
            
            var okButton = new Button
            {
                Content = "Thêm",
                Width = 80,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            okButton.Click += (s, e) => { DialogResult = true; Close(); };
            
            var cancelButton = new Button
            {
                Content = "Hủy",
                Width = 80,
                IsCancel = true
            };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

            buttons.Children.Add(okButton);
            buttons.Children.Add(cancelButton);
            
            Grid.SetRow(buttons, 2);
            grid.Children.Add(buttons);

            Content = grid;
        }
    }
}