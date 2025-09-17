namespace KidGuard;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private const string HostsPath = "C\\Windows\\System32\\drivers\\etc\\hosts";
    private const string Loopback = "127.0.0.1";

    private void SetStatus(string message)
    {
        statusLabel.Text = $"Trạng thái: {message}";
    }

    private static string NormalizeDomain(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var domain = input.Trim();
        if (domain.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) domain = domain[7..];
        if (domain.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) domain = domain[8..];
        if (domain.StartsWith("www.", StringComparison.OrdinalIgnoreCase)) domain = domain[4..];
        var slashIndex = domain.IndexOf('/') ;
        if (slashIndex >= 0) domain = domain.Substring(0, slashIndex);
        return domain;
    }

    private void blockButton_Click(object? sender, EventArgs e)
    {
        var domain = NormalizeDomain(domainTextBox.Text);
        if (string.IsNullOrEmpty(domain))
        {
            SetStatus("Vui lòng nhập domain hợp lệ");
            return;
        }
        try
        {
            if (!File.Exists(HostsPath))
            {
                SetStatus("Không tìm thấy hosts file");
                return;
            }
            var lines = File.ReadAllLines(HostsPath).ToList();
            var entry = $"{Loopback} {domain}";
            var wildcard = $"{Loopback} www.{domain}";
            bool changed = false;
            if (!lines.Any(l => l.Trim().Equals(entry, StringComparison.OrdinalIgnoreCase)))
            {
                lines.Add(entry);
                changed = true;
            }
            if (!lines.Any(l => l.Trim().Equals(wildcard, StringComparison.OrdinalIgnoreCase)))
            {
                lines.Add(wildcard);
                changed = true;
            }
            if (!changed)
            {
                SetStatus("Domain đã bị chặn sẵn");
                return;
            }
            File.WriteAllLines(HostsPath, lines);
            SetStatus($"Đã chặn: {domain}");
        }
        catch (UnauthorizedAccessException)
        {
            SetStatus("Cần chạy ứng dụng bằng quyền Administrator");
        }
        catch (Exception ex)
        {
            SetStatus($"Lỗi: {ex.Message}");
        }
    }

    private void unblockButton_Click(object? sender, EventArgs e)
    {
        var domain = NormalizeDomain(domainTextBox.Text);
        if (string.IsNullOrEmpty(domain))
        {
            SetStatus("Vui lòng nhập domain hợp lệ");
            return;
        }
        try
        {
            if (!File.Exists(HostsPath))
            {
                SetStatus("Không tìm thấy hosts file");
                return;
            }
            var lines = File.ReadAllLines(HostsPath).ToList();
            int before = lines.Count;
            lines = lines.Where(l =>
                !l.Trim().Equals($"{Loopback} {domain}", StringComparison.OrdinalIgnoreCase) &&
                !l.Trim().Equals($"{Loopback} www.{domain}", StringComparison.OrdinalIgnoreCase)
            ).ToList();
            if (lines.Count == before)
            {
                SetStatus("Không có rule để gỡ");
                return;
            }
            File.WriteAllLines(HostsPath, lines);
            SetStatus($"Đã gỡ chặn: {domain}");
        }
        catch (UnauthorizedAccessException)
        {
            SetStatus("Cần chạy ứng dụng bằng quyền Administrator");
        }
        catch (Exception ex)
        {
            SetStatus($"Lỗi: {ex.Message}");
        }
    }
}
