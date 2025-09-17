using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KidGuard.Core.Models;

namespace KidGuard.Core.Services
{
    /// <summary>
    /// Interface for web filtering service
    /// </summary>
    public interface IWebFilteringService
    {
        Task<bool> IsWebsiteBlockedAsync(string url);
        Task<bool> BlockWebsiteAsync(string domain);
        Task<bool> UnblockWebsiteAsync(string domain);
        Task<List<WebsiteFilter>> GetBlockedWebsitesAsync();
        Task<bool> AddCategoryFilterAsync(string category, FilterAction action);
        Task<List<string>> GetBlockedCategoriesAsync();
        Task<bool> EnableWebFilteringAsync();
        Task<bool> DisableWebFilteringAsync();
    }
}