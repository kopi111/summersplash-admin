using System;

namespace SummerSplashWeb.Models
{
    public class ClockRecord
    {
        public int RecordId { get; set; }
        public int UserId { get; set; }
        public int LocationId { get; set; }
        public DateTime ClockInTime { get; set; }
        public decimal? ClockInLatitude { get; set; }
        public decimal? ClockInLongitude { get; set; }
        public DateTime? ClockOutTime { get; set; }
        public decimal? ClockOutLatitude { get; set; }
        public decimal? ClockOutLongitude { get; set; }
        public decimal? TotalHours { get; set; }
        public string? JobsiteNotes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public string? UserName { get; set; }
        public string? LocationName { get; set; }

        public string StatusText => ClockOutTime.HasValue ? "Completed" : "Active";
        public string ClockInLocation => ClockInLatitude.HasValue && ClockInLongitude.HasValue
            ? $"{ClockInLatitude:F6}, {ClockInLongitude:F6}"
            : "N/A";
    }
}
