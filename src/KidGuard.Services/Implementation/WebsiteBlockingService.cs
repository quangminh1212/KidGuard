using System.Text;
using KidGuard.Core.Interfaces;
using KidGuard.Core.Models;
using Microsoft.Extensions.Logging;

namespace KidGuard.Services.Implementation;

/// <summary>
/// Service for managing website blocking through Windows hosts file
/// </summary>
public class WebsiteBlockingService : IWebsiteBlockingService
{
    private readonly ILogger<WebsiteBlockingService> _logger;
    private readonly string _hostsFilePath;
    private readonly string _markerStart = "# === KIDGUARD BLOCK START ===";
    private readonly string _markerEnd = "# === KIDGUARD BLOCK END ===";
    private readonly List<BlockedWebsite> _blockedWebsites = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public WebsiteBlockingService(ILogger<WebsiteBlockingService> logger)
    {
        _logger = logger;
        _hostsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            "drivers", "etc", "hosts"
        );
        
        InitializeHostsFile();
        LoadBlockedWebsites();
    }

    public async Task<bool> BlockWebsiteAsync(string domain, string category = "General", string? reason = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                _logger.LogWarning("Attempted to block empty domain");
                return false;
            }

            var website = new BlockedWebsite
            {
                Domain = domain,
                Category = category,
                Reason = reason
            };

            if (await IsWebsiteBlockedAsync(website.NormalizedDomain))
            {
                _logger.LogInformation("Website {Domain} is already blocked", domain);
                return true;
            }

            var lines = await File.ReadAllLinesAsync(_hostsFilePath);
            var updatedLines = AddBlockedDomain(lines.ToList(), website);
            
            await File.WriteAllLinesAsync(_hostsFilePath, updatedLines);
            _blockedWebsites.Add(website);
            
            _logger.LogInformation("Successfully blocked website: {Domain}", domain);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking website: {Domain}", domain);
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> UnblockWebsiteAsync(string domain)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (string.IsNullOrWhiteSpace(domain))
                return false;

            var normalizedDomain = NormalizeDomain(domain);
            var lines = await File.ReadAllLinesAsync(_hostsFilePath);
            var updatedLines = RemoveBlockedDomain(lines.ToList(), normalizedDomain);
            
            await File.WriteAllLinesAsync(_hostsFilePath, updatedLines);
            _blockedWebsites.RemoveAll(w => w.NormalizedDomain == normalizedDomain);
            
            _logger.LogInformation("Successfully unblocked website: {Domain}", domain);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblocking website: {Domain}", domain);
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IEnumerable<BlockedWebsite>> GetBlockedWebsitesAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return _blockedWebsites.ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> IsWebsiteBlockedAsync(string domain)
    {
        await _semaphore.WaitAsync();
        try
        {
            var normalizedDomain = NormalizeDomain(domain);
            return _blockedWebsites.Any(w => w.NormalizedDomain == normalizedDomain);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<int> ImportBlockListAsync(IEnumerable<string> domains, string category)
    {
        int blockedCount = 0;
        foreach (var domain in domains)
        {
            if (await BlockWebsiteAsync(domain, category))
                blockedCount++;
        }
        return blockedCount;
    }

    public async Task<IEnumerable<string>> ExportBlockListAsync()
    {
        var websites = await GetBlockedWebsitesAsync();
        return websites.Select(w => w.Domain);
    }

    private void InitializeHostsFile()
    {
        try
        {
            if (!File.Exists(_hostsFilePath))
            {
                _logger.LogWarning("Hosts file not found at {Path}", _hostsFilePath);
                return;
            }

            var lines = File.ReadAllLines(_hostsFilePath).ToList();
            
            if (!lines.Any(l => l.Contains(_markerStart)))
            {
                lines.Add("");
                lines.Add(_markerStart);
                lines.Add(_markerEnd);
                File.WriteAllLines(_hostsFilePath, lines);
                _logger.LogInformation("Initialized hosts file with KidGuard markers");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing hosts file");
        }
    }

    private void LoadBlockedWebsites()
    {
        try
        {
            var lines = File.ReadAllLines(_hostsFilePath);
            var startIndex = Array.FindIndex(lines, l => l.Contains(_markerStart));
            var endIndex = Array.FindIndex(lines, l => l.Contains(_markerEnd));
            
            if (startIndex >= 0 && endIndex > startIndex)
            {
                for (int i = startIndex + 1; i < endIndex; i++)
                {
                    var line = lines[i].Trim();
                    if (line.StartsWith("127.0.0.1") || line.StartsWith("0.0.0.0"))
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            var domain = parts[1];
                            if (!domain.StartsWith("www."))
                            {
                                _blockedWebsites.Add(new BlockedWebsite { Domain = domain });
                            }
                        }
                    }
                }
                _logger.LogInformation("Loaded {Count} blocked websites", _blockedWebsites.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading blocked websites");
        }
    }

    private List<string> AddBlockedDomain(List<string> lines, BlockedWebsite website)
    {
        var endIndex = lines.FindIndex(l => l.Contains(_markerEnd));
        if (endIndex < 0) return lines;

        var entries = new[]
        {
            $"127.0.0.1 {website.NormalizedDomain}",
            $"127.0.0.1 www.{website.NormalizedDomain}",
            $"::1 {website.NormalizedDomain}",
            $"::1 www.{website.NormalizedDomain}"
        };

        foreach (var entry in entries)
        {
            lines.Insert(endIndex, entry);
        }
        
        lines.Insert(endIndex, $"# {website.Category}: {website.Domain} - Blocked at {website.BlockedAt:yyyy-MM-dd HH:mm:ss}");
        
        return lines;
    }

    private List<string> RemoveBlockedDomain(List<string> lines, string normalizedDomain)
    {
        var startIndex = lines.FindIndex(l => l.Contains(_markerStart));
        var endIndex = lines.FindIndex(l => l.Contains(_markerEnd));
        
        if (startIndex >= 0 && endIndex > startIndex)
        {
            for (int i = endIndex - 1; i > startIndex; i--)
            {
                var line = lines[i];
                if (line.Contains(normalizedDomain))
                {
                    lines.RemoveAt(i);
                }
            }
        }
        
        return lines;
    }

    private static string NormalizeDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return string.Empty;
            
        domain = domain.ToLowerInvariant().Trim();
        
        if (domain.StartsWith("https://"))
            domain = domain[8..];
        else if (domain.StartsWith("http://"))
            domain = domain[7..];
            
        if (domain.StartsWith("www."))
            domain = domain[4..];
            
        domain = domain.TrimEnd('/');
        
        return domain;
    }
}