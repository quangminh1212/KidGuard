using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChildGuard.Core.Events;

namespace ChildGuard.Core.Detection
{
    /// <summary>
    /// Service phát hiện URL từ buffer keystrokes
    /// </summary>
    public class UrlDetectionService : IEventSource
    {
        private readonly Regex _urlRegex;
        private readonly HashSet<string> _commonDomains;
        private readonly Queue<string> _recentUrls;
        private readonly object _lockObject = new object();
        
        // Event source implementation
        public string SourceName => "UrlDetectionService";
        public IEventDispatcher? Dispatcher { get; set; }
        
        // Events
        public event EventHandler<UrlDetectedEventArgs>? UrlDetected;
        public event EventHandler<UrlVisitedEventArgs>? UrlVisited;
        
        public UrlDetectionService()
        {
            // Comprehensive URL pattern
            _urlRegex = new Regex(
                @"(?<url>" +
                @"(?:(?:https?|ftp):\/\/)?" + // Optional protocol
                @"(?:www\.)?" + // Optional www
                @"(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)*" + // Subdomain
                @"[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?" + // Domain name
                @"(?:\.[a-zA-Z]{2,})+" + // TLD
                @"(?::[0-9]{1,5})?" + // Optional port
                @"(?:\/[^\s]*)?" + // Path
                @")",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            );
            
            _recentUrls = new Queue<string>(100);
            
            // Common domains for quick detection
            _commonDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "google.com", "youtube.com", "facebook.com", "twitter.com", "instagram.com",
                "tiktok.com", "reddit.com", "amazon.com", "wikipedia.org", "github.com",
                "stackoverflow.com", "linkedin.com", "netflix.com", "twitch.tv", "discord.com",
                "spotify.com", "microsoft.com", "apple.com", "pornhub.com", "xvideos.com",
                "onlyfans.com", "4chan.org", "8chan.net", "torrent", "piratebay"
            };
        }
        
        /// <summary>
        /// Detect URLs in text buffer
        /// </summary>
        public List<DetectedUrl> DetectUrls(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<DetectedUrl>();
            
            var urls = new List<DetectedUrl>();
            var matches = _urlRegex.Matches(text);
            
            foreach (Match match in matches)
            {
                var url = match.Groups["url"].Value;
                
                // Normalize URL
                var normalizedUrl = NormalizeUrl(url);
                
                // Extract domain
                var domain = ExtractDomain(normalizedUrl);
                
                // Check if it's a valid URL
                if (IsValidUrl(normalizedUrl, domain))
                {
                    var detectedUrl = new DetectedUrl
                    {
                        OriginalUrl = url,
                        NormalizedUrl = normalizedUrl,
                        Domain = domain,
                        Position = match.Index,
                        DetectedAt = DateTime.UtcNow,
                        IsComplete = IsCompleteUrl(normalizedUrl)
                    };
                    
                    urls.Add(detectedUrl);
                    
                    // Fire event
                    OnUrlDetected(detectedUrl);
                }
            }
            
            // Also check for domain-only patterns
            CheckDomainPatterns(text, urls);
            
            return urls;
        }
        
        /// <summary>
        /// Process keystroke buffer for URL detection
        /// </summary>
        public async Task ProcessKeystrokeBuffer(string buffer, bool enterPressed = false)
        {
            var urls = DetectUrls(buffer);
            
            foreach (var url in urls)
            {
                // If Enter was pressed after URL, consider it as visited
                if (enterPressed && url.IsComplete)
                {
                    await OnUrlVisitedAsync(url);
                }
                
                // Track recent URLs to avoid duplicates
                lock (_lockObject)
                {
                    if (!_recentUrls.Contains(url.NormalizedUrl))
                    {
                        _recentUrls.Enqueue(url.NormalizedUrl);
                        
                        // Keep only last 100 URLs
                        while (_recentUrls.Count > 100)
                        {
                            _recentUrls.Dequeue();
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Check for domain-only patterns
        /// </summary>
        private void CheckDomainPatterns(string text, List<DetectedUrl> urls)
        {
            var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var word in words)
            {
                var cleanWord = word.Trim().ToLowerInvariant();
                
                // Check against known domains
                foreach (var domain in _commonDomains)
                {
                    if (cleanWord.Contains(domain) && !urls.Any(u => u.Domain == domain))
                    {
                        var detectedUrl = new DetectedUrl
                        {
                            OriginalUrl = word,
                            NormalizedUrl = $"https://{domain}",
                            Domain = domain,
                            Position = text.IndexOf(word, StringComparison.OrdinalIgnoreCase),
                            DetectedAt = DateTime.UtcNow,
                            IsComplete = false
                        };
                        
                        urls.Add(detectedUrl);
                        OnUrlDetected(detectedUrl);
                    }
                }
            }
        }
        
        /// <summary>
        /// Normalize URL
        /// </summary>
        private string NormalizeUrl(string url)
        {
            url = url.Trim().ToLowerInvariant();
            
            // Add protocol if missing
            if (!url.StartsWith("http://") && !url.StartsWith("https://") && !url.StartsWith("ftp://"))
            {
                url = "https://" + url;
            }
            
            // Remove trailing slash
            if (url.EndsWith("/"))
            {
                url = url.TrimEnd('/');
            }
            
            return url;
        }
        
        /// <summary>
        /// Extract domain from URL
        /// </summary>
        private string ExtractDomain(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host.Replace("www.", "");
            }
            catch
            {
                // Fallback: extract domain using regex
                var match = Regex.Match(url, @"(?:https?:\/\/)?(?:www\.)?([^\/\s]+)");
                return match.Success ? match.Groups[1].Value : string.Empty;
            }
        }
        
        /// <summary>
        /// Validate URL
        /// </summary>
        private bool IsValidUrl(string url, string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return false;
            
            // Must have at least one dot for TLD
            if (!domain.Contains('.'))
                return false;
            
            // Check URL can be parsed
            try
            {
                var uri = new Uri(url);
                return uri.IsAbsoluteUri;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Check if URL is complete (has protocol and valid format)
        /// </summary>
        private bool IsCompleteUrl(string url)
        {
            return url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("ftp://");
        }
        
        /// <summary>
        /// Get recent URLs
        /// </summary>
        public List<string> GetRecentUrls()
        {
            lock (_lockObject)
            {
                return _recentUrls.ToList();
            }
        }
        
        /// <summary>
        /// Clear recent URLs
        /// </summary>
        public void ClearRecentUrls()
        {
            lock (_lockObject)
            {
                _recentUrls.Clear();
            }
        }
        
        private void OnUrlDetected(DetectedUrl url)
        {
            var args = new UrlDetectedEventArgs
            {
                Url = url,
                Timestamp = DateTime.UtcNow
            };
            
            UrlDetected?.Invoke(this, args);
            
            // Publish to event bus
            Dispatcher?.PublishAsync(new UrlDetectedEvent(url.NormalizedUrl, url.Domain));
        }
        
        private async Task OnUrlVisitedAsync(DetectedUrl url)
        {
            var args = new UrlVisitedEventArgs
            {
                Url = url,
                Timestamp = DateTime.UtcNow
            };
            
            UrlVisited?.Invoke(this, args);
            
            // Publish to event bus
            if (Dispatcher != null)
            {
                await Dispatcher.PublishAsync(new UrlVisitedEvent(url.NormalizedUrl, url.Domain));
            }
        }
    }
    
    /// <summary>
    /// Detected URL model
    /// </summary>
    public class DetectedUrl
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public string NormalizedUrl { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public int Position { get; set; }
        public DateTime DetectedAt { get; set; }
        public bool IsComplete { get; set; }
    }
    
    /// <summary>
    /// URL detected event arguments
    /// </summary>
    public class UrlDetectedEventArgs : EventArgs
    {
        public DetectedUrl Url { get; set; } = new DetectedUrl();
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// URL visited event arguments
    /// </summary>
    public class UrlVisitedEventArgs : EventArgs
    {
        public DetectedUrl Url { get; set; } = new DetectedUrl();
        public DateTime Timestamp { get; set; }
    }
}
