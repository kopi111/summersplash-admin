using System;

namespace SummerSplashWeb.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string? Position { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public DateTime? HireDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
        public bool EmailVerified { get; set; }
        public string? VerificationCode { get; set; }
        public DateTime? VerificationCodeExpiry { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public string DisplayPosition => Position ?? "Not Assigned";
        public string StatusText => IsApproved ? (IsActive ? "Active" : "Inactive") : "Pending Approval";
    }

    public enum UserPosition
    {
        Lifeguard,
        ServiceTech,
        Manager,
        Supervisor,
        SafetyAudit,
        Foreman,
        Laborer,
        SuperAdmin
    }
}
