namespace KidGuardWin;

public partial class Form1 : Form
{
    private const string HostsFilePath = "C\\Windows\\System32\\drivers\\etc\\hosts";
    private const string MarkerStart = "# KIDGUARD_START";
    private const string MarkerEnd = "# KIDGUARD_END";

    public Form1()
    {
        InitializeComponent();
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        EnsureMarkersExist();
        RefreshBlockedList();
        if (!IsRunningAsAdministrator())
        {
            SetStatus("Cần chạy bằng quyền Administrator để chặn website.", isError: true);
        }
    }

    private void btnBlock_Click(object? sender, EventArgs e)
    {
        var domain = (txtDomain.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(domain))
        {
            SetStatus("Vui lòng nhập domain.", isError: true);
            return;
        }

        try
        {
            BlockDomain(domain);
            RefreshBlockedList();
            SetStatus($"Đã chặn: {domain}");
        }
        catch (Exception ex)
        {
            SetStatus($"Lỗi khi chặn: {ex.Message}", isError: true);
        }
    }

    private void btnUnblock_Click(object? sender, EventArgs e)
    {
        var domain = (txtDomain.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(domain))
        {
            if (lstBlocked.SelectedItem is string selected)
            {
                domain = selected;
            }
            else
            {
                SetStatus("Vui lòng nhập domain hoặc chọn từ danh sách.", isError: true);
                return;
            }
        }

        try
        {
            UnblockDomain(domain);
            RefreshBlockedList();
            SetStatus($"Đã bỏ chặn: {domain}");
        }
        catch (Exception ex)
        {
            SetStatus($"Lỗi khi bỏ chặn: {ex.Message}", isError: true);
        }
    }

    private static bool IsRunningAsAdministrator()
    {
        try
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    private void EnsureMarkersExist()
    {
        if (!File.Exists(HostsFilePath))
        {
            throw new FileNotFoundException("Không tìm thấy hosts file", HostsFilePath);
        }
        var lines = File.ReadAllLines(HostsFilePath).ToList();
        if (!lines.Contains(MarkerStart) || !lines.Contains(MarkerEnd))
        {
            lines.Add("");
            lines.Add(MarkerStart);
            lines.Add(MarkerEnd);
            File.WriteAllLines(HostsFilePath, lines);
        }
    }

    private void BlockDomain(string domain)
    {
        EnsureMarkersExist();
        var lines = File.ReadAllLines(HostsFilePath).ToList();
        var startIdx = lines.FindIndex(l => l.Trim() == MarkerStart);
        var endIdx = lines.FindIndex(l => l.Trim() == MarkerEnd);
        if (startIdx < 0 || endIdx < 0 || endIdx < startIdx) throw new InvalidOperationException("Markers không hợp lệ");

        string normalized = NormalizeDomain(domain);
        string entry1 = $"127.0.0.1 {normalized}";
        string entry2 = $"127.0.0.1 www.{normalized}";

        // Xóa trùng trong block
        for (int i = endIdx - 1; i > startIdx; i--)
        {
            if (string.Equals(lines[i].Trim(), entry1, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(lines[i].Trim(), entry2, StringComparison.OrdinalIgnoreCase))
            {
                lines.RemoveAt(i);
            }
        }
        lines.Insert(endIdx, entry2);
        lines.Insert(endIdx, entry1);
        File.WriteAllLines(HostsFilePath, lines);
    }

    private void UnblockDomain(string domain)
    {
        EnsureMarkersExist();
        var lines = File.ReadAllLines(HostsFilePath).ToList();
        var startIdx = lines.FindIndex(l => l.Trim() == MarkerStart);
        var endIdx = lines.FindIndex(l => l.Trim() == MarkerEnd);
        if (startIdx < 0 || endIdx < 0 || endIdx < startIdx) throw new InvalidOperationException("Markers không hợp lệ");

        string normalized = NormalizeDomain(domain);
        string entry1 = $"127.0.0.1 {normalized}";
        string entry2 = $"127.0.0.1 www.{normalized}";

        for (int i = endIdx - 1; i > startIdx; i--)
        {
            var t = lines[i].Trim();
            if (string.Equals(t, entry1, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t, entry2, StringComparison.OrdinalIgnoreCase))
            {
                lines.RemoveAt(i);
            }
        }
        File.WriteAllLines(HostsFilePath, lines);
    }

    private void RefreshBlockedList()
    {
        lstBlocked.Items.Clear();
        var lines = File.ReadAllLines(HostsFilePath).ToList();
        var startIdx = lines.FindIndex(l => l.Trim() == MarkerStart);
        var endIdx = lines.FindIndex(l => l.Trim() == MarkerEnd);
        if (startIdx >= 0 && endIdx > startIdx)
        {
            var entries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = startIdx + 1; i < endIdx; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("127.0.0.1 ", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        var d = parts[1];
                        d = d.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? d.Substring(4) : d;
                        entries.Add(d);
                    }
                }
            }
            foreach (var d in entries.OrderBy(x => x))
            {
                lstBlocked.Items.Add(d);
            }
        }
    }

    private static string NormalizeDomain(string domain)
    {
        domain = domain.ToLowerInvariant();
        if (domain.StartsWith("http://")) domain = domain.Substring(7);
        if (domain.StartsWith("https://")) domain = domain.Substring(8);
        domain = domain.Trim('/');
        return domain;
    }

    private void SetStatus(string message, bool isError = false)
    {
        lblStatus.Text = message;
        lblStatus.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.ForestGreen;
    }
}
