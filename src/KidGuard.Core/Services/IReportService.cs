using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KidGuard.Core.DTOs;

namespace KidGuard.Core.Services
{
    /// <summary>
    /// Interface for report generation service
    /// </summary>
    public interface IReportService
    {
        Task<ReportDto> GenerateDailyReportAsync(DateTime date);
        Task<ReportDto> GenerateWeeklyReportAsync(DateTime startDate);
        Task<ReportDto> GenerateMonthlyReportAsync(int year, int month);
        Task<byte[]> ExportReportToPdfAsync(ReportDto report);
        Task<byte[]> ExportReportToExcelAsync(ReportDto report);
        Task<bool> EmailReportAsync(ReportDto report, string emailAddress);
        Task<List<ReportDto>> GetSavedReportsAsync();
        Task<bool> SaveReportAsync(ReportDto report);
        Task<bool> DeleteReportAsync(int reportId);
    }
}