using System;

namespace SummerSplashWeb.Models
{
    public class Schedule
    {
        public int ScheduleId { get; set; }
        public int UserId { get; set; }
        public int LocationId { get; set; }
        public int? SupervisorId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public string? UserName { get; set; }
        public string? LocationName { get; set; }
        public string? SupervisorName { get; set; }

        public string TimeRange => StartTime.HasValue && EndTime.HasValue
            ? $"{StartTime.Value:hh\\:mm} - {EndTime.Value:hh\\:mm}"
            : "Not set";
        public double ScheduledHours => StartTime.HasValue && EndTime.HasValue
            ? (EndTime.Value - StartTime.Value).TotalHours
            : 0;
    }

    public class DaySchedule
    {
        public DateTime Date { get; set; }
        public bool IsScheduled { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? Notes { get; set; }
    }
}
