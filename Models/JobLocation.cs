using System;

namespace SummerSplashWeb.Models
{
    public class JobLocation
    {
        public int LocationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? PoolType { get; set; }
        public string? PoolSize { get; set; }
        public string? Notes { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int GeofenceRadius { get; set; } = 100; // Default 100 meters
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public string DisplayAddress => Address ?? "No address provided";
        public string DisplayContact => !string.IsNullOrEmpty(ContactName)
            ? $"{ContactName} - {ContactPhone}"
            : "No contact info";
    }
}
