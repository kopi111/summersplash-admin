using System;
using System.Collections.Generic;

namespace SummerSplashWeb.Models
{
    public class ServiceTechReport
    {
        public int ReportId { get; set; }
        public int UserId { get; set; }
        public int TechId => UserId; // Alias for UserId
        public int LocationId { get; set; }
        public int? ClockRecordId { get; set; }
        public DateTime ServiceDate { get; set; }
        public TimeSpan ServiceTime => ServiceDate.TimeOfDay;
        public DateTime? ClockInTime { get; set; }
        public DateTime? ClockOutTime { get; set; }
        public string? ServiceType { get; set; }
        public string? WorkPerformed { get; set; }
        public string? ChemicalsAddedNotes { get; set; }
        public string? IssuesFound { get; set; }
        public string? Recommendations { get; set; }

        // Checklist Items (19 items from user requirements)
        public bool PoolVacuumed { get; set; }
        public bool PoolBrushed { get; set; }
        public bool SkimmerBasketsEmptied { get; set; }
        public bool PumpBasketsEmptied { get; set; }
        public bool FilterCleaned { get; set; }
        public bool ChemicalsAdded { get; set; }
        public bool PoolDeckCleaned { get; set; }
        public bool EquipmentChecked { get; set; }
        public bool GateLocksChecked { get; set; }
        public bool SafetyEquipmentInspected { get; set; }
        public bool WaterLevelChecked { get; set; }
        public bool DebrisRemoved { get; set; }
        public bool TilesInspected { get; set; }
        public bool DrainCoversChecked { get; set; }
        public bool LightsChecked { get; set; }
        public bool SignageChecked { get; set; }
        public bool FurnitureArranged { get; set; }
        public bool RestroomsCleaned { get; set; }
        public bool PoolGateLocked { get; set; }

        // Supplies Needed
        public string? SuppliesNeeded { get; set; }

        // Report Distribution
        public string? ReportSentTo { get; set; } // Me, Manager, Customer
        public int? CustomerRating { get; set; } // 1-5 stars
        public string? CustomerFeedback { get; set; }

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public string? UserName { get; set; }
        public string? TechName => UserName; // Alias for UserName
        public string? LocationName { get; set; }
        public string? LocationAddress { get; set; }
        public List<ChemicalReading>? ChemicalReadings { get; set; }
        public List<Photo>? Photos { get; set; }

        public int ChecklistCompletionPercentage
        {
            get
            {
                var items = new bool[] {
                    PoolVacuumed, PoolBrushed, SkimmerBasketsEmptied, PumpBasketsEmptied,
                    FilterCleaned, ChemicalsAdded, PoolDeckCleaned, EquipmentChecked,
                    GateLocksChecked, SafetyEquipmentInspected, WaterLevelChecked, DebrisRemoved,
                    TilesInspected, DrainCoversChecked, LightsChecked, SignageChecked,
                    FurnitureArranged, RestroomsCleaned, PoolGateLocked
                };

                int total = items.Length;
                int completed = items.Count(item => item);

                return total > 0 ? (int)((completed / (double)total) * 100) : 0;
            }
        }
    }

    public class ChemicalReading
    {
        public int ReadingId { get; set; }
        public int ReportId { get; set; }
        public string PoolType { get; set; } = "Main pool"; // Main pool, Wading Pool, Spa, Other
        public decimal? ChlorineBromine { get; set; }
        public decimal? pH { get; set; }
        public decimal? PHLevel => pH; // Alias for pH
        public decimal? ChlorineLevel => ChlorineBromine; // Alias for ChlorineBromine
        public decimal? CalciumHardness { get; set; }
        public decimal? TotalAlkalinity { get; set; }
        public decimal? Alkalinity => TotalAlkalinity; // Alias for TotalAlkalinity
        public decimal? CyanuricAcid { get; set; }
        public decimal? Salt { get; set; }
        public decimal? Temperature { get; set; }
        public DateTime ReadingTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Photo
    {
        public int PhotoId { get; set; }
        public int ReportId { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public DateTime PhotoTimestamp { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
