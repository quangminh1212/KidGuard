using KidGuard.Core.Models;

namespace KidGuard.Core.Interfaces;

/// <summary>
/// Interface for website blocking functionality
/// </summary>
public interface IWebsiteBlockingService
{
    /// <summary>
    /// Blocks a website by adding it to the hosts file
    /// </summary>
    Task<bool> BlockWebsiteAsync(string domain, string category = "General", string? reason = null);
    
    /// <summary>
    /// Unblocks a website by removing it from the hosts file
    /// </summary>
    Task<bool> UnblockWebsiteAsync(string domain);
    
    /// <summary>
    /// Gets all blocked websites
    /// </summary>
    Task<IEnumerable<BlockedWebsite>> GetBlockedWebsitesAsync();
    
    /// <summary>
    /// Checks if a website is currently blocked
    /// </summary>
    Task<bool> IsWebsiteBlockedAsync(string domain);
    
    /// <summary>
    /// Imports a list of websites to block from categories
    /// </summary>
    Task<int> ImportBlockListAsync(IEnumerable<string> domains, string category);
    
    /// <summary>
    /// Exports the current block list
    /// </summary>
    Task<IEnumerable<string>> ExportBlockListAsync();
}