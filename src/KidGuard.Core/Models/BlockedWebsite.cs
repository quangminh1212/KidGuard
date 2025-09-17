namespace KidGuard.Core.Models;

/// <summary>
/// Represents a blocked website entry
/// </summary>
public class BlockedWebsite
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Domain { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public DateTime BlockedAt { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
    public string? Reason { get; set; }
    
    /// <summary>
    /// Normalizes the domain name by removing protocol and www prefix
    /// </summary>
    public string NormalizedDomain => NormalizeDomain(Domain);
    
    private static string NormalizeDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return string.Empty;
            
        domain = domain.ToLowerInvariant().Trim();
        
        // Remove protocol
        if (domain.StartsWith("https://"))
            domain = domain[8..];
        else if (domain.StartsWith("http://"))
            domain = domain[7..];
            
        // Remove www prefix
        if (domain.StartsWith("www."))
            domain = domain[4..];
            
        // Remove trailing slash
        domain = domain.TrimEnd('/');
        
        return domain;
    }
}