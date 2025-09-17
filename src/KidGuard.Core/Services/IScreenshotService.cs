using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using KidGuard.Core.Models;

namespace KidGuard.Core.Services
{
    /// <summary>
    /// Interface for screenshot service
    /// </summary>
    public interface IScreenshotService
    {
        Task<Screenshot> TakeScreenshotAsync();
        Task<List<Screenshot>> GetScreenshotsAsync(DateTime startDate, DateTime endDate);
        Task<bool> DeleteScreenshotAsync(int screenshotId);
        Task<bool> DeleteOldScreenshotsAsync(DateTime olderThan);
        Task StartPeriodicScreenshotAsync(TimeSpan interval);
        Task StopPeriodicScreenshotAsync();
        string ScreenshotStoragePath { get; set; }
    }
}