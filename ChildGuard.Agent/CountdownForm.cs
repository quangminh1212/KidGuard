namespace ChildGuard.Agent;

public partial class CountdownForm : Form
{
    private int _seconds;
    private readonly string _procName;
    private readonly CancellationTokenSource _cts;
    private readonly Action _onCloseNow;
    private readonly System.Windows.Forms.Timer _timer;

    public CountdownForm(string procName, int seconds, CancellationTokenSource cts, Action onCloseNow)
    {
        InitializeComponent();
        _procName = procName;
        _seconds = Math.Max(0, seconds);
        _cts = cts;
        _onCloseNow = onCloseNow;
        _timer = new System.Windows.Forms.Timer
        {
            Interval = 1000
        };
        _timer.Tick += (_, __) => Tick();
    }

    private void CountdownForm_Load(object? sender, EventArgs e)
    {
        this.TopMost = true;
        lblMsg.Text = $"Ứng dụng '{_procName}' sẽ bị đóng theo cấu hình.";
        UpdateCountdownText();
        _timer.Start();
    }

    private void Tick()
    {
        if (_cts.IsCancellationRequested)
        {
            TryCloseForm();
            return;
        }
        if (_seconds <= 0)
        {
            _timer.Stop();
            try { _onCloseNow(); } catch { }
            TryCloseForm();
            return;
        }
        _seconds--;
        UpdateCountdownText();
    }

    private void UpdateCountdownText()
    {
        lblCountdown.Text = $"Đóng sau: {_seconds}s";
    }

    private void btnCloseNow_Click(object? sender, EventArgs e)
    {
        try { _onCloseNow(); } catch { }
        try { _cts.Cancel(); } catch { }
        TryCloseForm();
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        try { _cts.Cancel(); } catch { }
        TryCloseForm();
    }

    private async void btnDelay_Click(object? sender, EventArgs e)
    {
        await DelayAndClose(TimeSpan.FromMinutes(5), "Đã hoãn 5 phút");
    }

    private async void btnDelay10_Click(object? sender, EventArgs e)
    {
        await DelayAndClose(TimeSpan.FromMinutes(10), "Đã hoãn 10 phút");
    }

    private async void btnDelay30_Click(object? sender, EventArgs e)
    {
        await DelayAndClose(TimeSpan.FromMinutes(30), "Đã hoãn 30 phút");
    }

    private async Task DelayAndClose(TimeSpan span, string message)
    {
        try { _cts.Cancel(); } catch { }
        lblCountdown.Text = message;
        try { _timer.Stop(); } catch { }
        await Task.Delay(span);
        try { _onCloseNow(); } catch { }
        TryCloseForm();
    }

    private void TryCloseForm()
    {
        try { _timer.Stop(); } catch { }
        try { this.Close(); } catch { }
    }
}

