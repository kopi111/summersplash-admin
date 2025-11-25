using System;

namespace SummerSplashWeb.Models
{
    public class DashboardStats
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int PendingApprovals { get; set; }
        public int TotalLocations { get; set; }
        public int ActiveShifts { get; set; }
        public int CompletedReportsToday { get; set; }
        public int TodayReports { get; set; } // Alias for CompletedReportsToday
        public int PendingSupplyOrders { get; set; }
        public decimal AverageCustomerRating { get; set; }
        public int TotalIncidentsThisMonth { get; set; }
        public decimal TotalHoursWorkedToday { get; set; }
        public decimal TotalHoursToday { get; set; } // Alias for TotalHoursWorkedToday
        public int ScheduledShifts { get; set; }
        public int MissedPunches { get; set; }
        public int OverdueTrainings { get; set; }
        public int UnreadNotifications { get; set; }
    }

    public class PerformanceMetric
    {
        public int MetricId { get; set; }
        public int UserId { get; set; }
        public int LocationId { get; set; }
        public DateTime MetricDate { get; set; }
        public int TasksCompleted { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public int OnTimeClockIns { get; set; }
        public int LateClockIns { get; set; }
        public decimal CustomerRatingAvg { get; set; }
        public int IncidentCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? UserName { get; set; }
        public string? LocationName { get; set; }

        public double OnTimePercentage => (OnTimeClockIns + LateClockIns) > 0
            ? (OnTimeClockIns / (double)(OnTimeClockIns + LateClockIns)) * 100
            : 0;
    }

    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Info"; // Info, Warning, Alert, Success
        public bool IsRead { get; set; }
        public string? ActionUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public string IconKind => Type switch
        {
            "Warning" => "AlertCircle",
            "Alert" => "Alert",
            "Success" => "CheckCircle",
            _ => "Information"
        };

        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedAt;
                if (timeSpan.TotalMinutes < 1) return "Just now";
                if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
                if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
                if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";
                return CreatedAt.ToString("MMM dd");
            }
        }
    }
}
