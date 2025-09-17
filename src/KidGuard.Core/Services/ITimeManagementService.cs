using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KidGuard.Core.DTOs;
using KidGuard.Core.Models;

namespace KidGuard.Core.Services
{
    /// <summary>
    /// Interface for time management service
    /// </summary>
    public interface ITimeManagementService
    {
        Task<bool> SetDailyTimeLimitAsync(TimeSpan limit);
        Task<bool> SetTimeScheduleAsync(DayOfWeek day, TimeSpan startTime, TimeSpan endTime);
        Task<TimeRestriction> GetTimeRestrictionAsync(DateTime date);
        Task<TimeSpan> GetRemainingTimeAsync();
        Task<TimeUsageDto> GetTimeUsageAsync(DateTime date);
        Task<bool> AddBonusTimeAsync(TimeSpan bonusTime);
        Task<bool> SetBreakTimeAsync(TimeSpan workInterval, TimeSpan breakDuration);
        Task<bool> EnableTimeRestrictionAsync();
        Task<bool> DisableTimeRestrictionAsync();
        event EventHandler<TimeEventArgs> TimeLimitReached;
        event EventHandler<TimeEventArgs> BreakTimeRequired;
    }

    public class TimeEventArgs : EventArgs
    {
        public TimeSpan TimeUsed { get; set; }
        public TimeSpan TimeRemaining { get; set; }
        public string Message { get; set; }
    }
}