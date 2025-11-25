using System;

namespace SummerSplashWeb.Models
{
    public class Training
    {
        public int TrainingId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsCertification { get; set; }
        public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed
        public string? CertificateUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public string? EmployeeName { get; set; }
    }

    public class TrainingTopic
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TrainingAssignment
    {
        public int AssignmentId { get; set; }
        public int TopicId { get; set; }
        public int UserId { get; set; }
        public int AssignedBy { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed
        public decimal? Score { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public string? TopicName { get; set; }
        public string? UserName { get; set; }
        public string? AssignedByName { get; set; }

        public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.Now && Status != "Completed";
        public string StatusColor => Status switch
        {
            "Completed" => "#4CAF50",
            "InProgress" => "#2196F3",
            _ when IsOverdue => "#F44336",
            _ => "#FF9800"
        };
    }
}
