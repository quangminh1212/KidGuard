using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;

namespace ChildGuard.Core.Detection;

public class UrlSafetyChecker
{
    private readonly HashSet<string> _blacklistedDomains;
    private readonly HashSet<string> _whitelistedDomains;
    private readonly List<Regex> _suspiciousPatterns;
    private readonly HttpClient _httpClient;
    
    public UrlSafetyChecker()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        
        // Danh sách domain đáng tin cậy
        _whitelistedDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "google.com", "youtube.com", "facebook.com", "wikipedia.org",
            "microsoft.com", "github.com", "stackoverflow.com",
            // Các trang giáo dục Việt Nam
            "edu.vn", "hocmai.vn", "olm.vn", "violet.vn",
            "moet.gov.vn", "vnu.edu.vn"
        };
        
        // Danh sách domain nguy hiểm hoặc không phù hợp
        _blacklistedDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Thêm các domain cần chặn
            "malware-site.com", "phishing-example.com",
            // Các trang cờ bạc
            "bet365.com", "188bet.com", "w88.com"
        };
        
        // Patterns đáng ngờ
        _suspiciousPatterns = new List<Regex>
        {
            // IP addresses thay vì domain
            new Regex(@"^https?://\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}", RegexOptions.IgnoreCase),
            // URLs với nhiều dấu gạch ngang hoặc số
            new Regex(@"https?://[^/]*(-\d{3,}|\d{5,})[^/]*", RegexOptions.IgnoreCase),
            // Shortened URLs
            new Regex(@"^https?://(bit\.ly|tinyurl\.com|goo\.gl|ow\.ly|short\.link)", RegexOptions.IgnoreCase),
            // Suspicious TLDs
            new Regex(@"\.(tk|ml|ga|cf|click|download|win)(/|$)", RegexOptions.IgnoreCase)
        };
    }
    
    public async Task<UrlCheckResult> CheckUrlAsync(string url)
    {
        var result = new UrlCheckResult { Url = url };
        
        if (string.IsNullOrWhiteSpace(url))
        {
            result.IsSafe = true;
            result.Reason = "Empty URL";
            return result;
        }
        
        // Normalize URL
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "http://" + url;
        }
        
        Uri uri;
        try
        {
            uri = new Uri(url);
        }
        catch
        {
            result.IsSafe = false;
            result.Reason = "Invalid URL format";
            result.RiskLevel = RiskLevel.Medium;
            return result;
        }
        
        // Check whitelist
        if (IsWhitelisted(uri.Host))
        {
            result.IsSafe = true;
            result.Reason = "Trusted domain";
            result.RiskLevel = RiskLevel.None;
            result.ThreatLevel = UrlThreatLevel.Low;
            return result;
        }
        
        // Check blacklist
        if (IsBlacklisted(uri.Host))
        {
            result.IsSafe = false;
            result.Reason = "Blacklisted domain";
            result.RiskLevel = RiskLevel.High;
            result.ThreatLevel = UrlThreatLevel.High;
            result.Categories.Add("Blocked");
            return result;
        }
        
        // Check suspicious patterns
        foreach (var pattern in _suspiciousPatterns)
        {
            if (pattern.IsMatch(url))
            {
                result.IsSafe = false;
                result.Reason = "Suspicious URL pattern";
                result.RiskLevel = RiskLevel.Medium;
                result.ThreatLevel = UrlThreatLevel.Medium;
                result.Categories.Add("Suspicious");
                return result;
            }
        }
        
        // Check for phishing indicators
        if (CheckPhishingIndicators(url, uri.Host))
        {
            result.IsSafe = false;
            result.Reason = "Possible phishing site";
            result.RiskLevel = RiskLevel.High;
            result.ThreatLevel = UrlThreatLevel.High;
            result.Categories.Add("Phishing");
            return result;
        }
        
        // Try to check if site is accessible
        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                result.IsSafe = false;
                result.Reason = $"Site returned error: {response.StatusCode}";
                result.RiskLevel = RiskLevel.Low;
            }
            else
            {
                result.IsSafe = true;
                result.Reason = "Site is accessible";
                result.RiskLevel = RiskLevel.None;
            }
        }
        catch (Exception ex)
        {
            result.IsSafe = false;
            result.Reason = $"Could not access site: {ex.Message}";
            result.RiskLevel = RiskLevel.Medium;
        }
        
        return result;
    }
    
    private bool IsWhitelisted(string domain)
    {
        // Check exact match
        if (_whitelistedDomains.Contains(domain))
            return true;
        
        // Check subdomain of whitelisted
        return _whitelistedDomains.Any(white => 
            domain.EndsWith("." + white, StringComparison.OrdinalIgnoreCase));
    }
    
    private bool IsBlacklisted(string domain)
    {
        // Check exact match
        if (_blacklistedDomains.Contains(domain))
            return true;
        
        // Check if contains blacklisted domain
        return _blacklistedDomains.Any(black => 
            domain.Contains(black, StringComparison.OrdinalIgnoreCase));
    }
    
    private bool CheckPhishingIndicators(string url, string domain)
    {
        // Check for common phishing patterns
        var lowerUrl = url.ToLower();
        var lowerDomain = domain.ToLower();
        
        // Fake popular sites
        var popularSites = new[] { "facebook", "google", "paypal", "amazon", "microsoft", "apple" };
        foreach (var site in popularSites)
        {
            // Domain contains popular site name but isn't the real site
            if (lowerDomain.Contains(site) && !IsWhitelisted(domain))
            {
                // Check for typos or variations
                if (lowerDomain.Contains(site + "-") || 
                    lowerDomain.Contains("-" + site) ||
                    lowerDomain.Replace("0", "o").Contains(site) ||
                    lowerDomain.Replace("1", "l").Contains(site))
                {
                    return true;
                }
            }
        }
        
        // Check for suspicious keywords in URL
        var suspiciousKeywords = new[] { 
            "verify", "confirm", "update", "secure", "account", 
            "suspend", "limit", "expire", "click-here"
        };
        
        return suspiciousKeywords.Count(keyword => lowerUrl.Contains(keyword)) >= 2;
    }
    
    public void AddToBlacklist(string domain)
    {
        _blacklistedDomains.Add(domain);
    }
    
    public void AddToWhitelist(string domain)
    {
        _whitelistedDomains.Add(domain);
    }
}

public class UrlCheckResult
{
    public string Url { get; set; }
    public bool IsSafe { get; set; }
    public string Reason { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public UrlThreatLevel ThreatLevel { get; set; }
    public List<string> Categories { get; set; } = new List<string>();
    
    public string GetSummary()
    {
        if (IsSafe)
            return $"URL an toàn: {Reason}";
        
        return $"URL nguy hiểm - {RiskLevel}: {Reason}";
    }
}

public enum RiskLevel
{
    None,
    Low,
    Medium,
    High,
    Critical
}

public enum UrlThreatLevel
{
    Low,
    Medium,
    High
}
