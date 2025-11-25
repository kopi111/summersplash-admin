using System;

namespace SummerSplashWeb.Models
{
    public class SiteEvaluation
    {
        public int EvaluationId { get; set; }
        public int UserId { get; set; }
        public int LocationId { get; set; }
        public int ClockRecordId { get; set; }
        public string EvaluationType { get; set; } = string.Empty; // SafetyAudit, ManagerCheck, SupervisorCheck

        // Safety Checklist
        public bool? PoolOpen { get; set; }
        public bool? FacilityEntryProcedures { get; set; }
        public bool? MainDrainVisible { get; set; }
        public bool? AEDPresent { get; set; }
        public bool? RescueTubePresent { get; set; }
        public bool? BackboardPresent { get; set; }
        public bool? FirstAidKit { get; set; }
        public bool? BloodbornePathogenKit { get; set; }
        public bool? HazMatKit { get; set; }
        public bool? GateFenceSecured { get; set; }
        public bool? EmergencyPhoneWorking { get; set; }
        public bool? MSDS { get; set; }
        public bool? SafetySuppliesNeeded { get; set; }

        // Additional Manager/Supervisor Checks
        public bool? StaffOnDuty { get; set; }
        public bool? ScanningRotationDiscussed { get; set; }
        public bool? ZonesEstablished { get; set; }
        public bool? BreakTimeDiscussed { get; set; }
        public bool? GateControlDiscussed { get; set; }
        public bool? CellphonePolicyDiscussed { get; set; }
        public bool? PumproomCleaned { get; set; }
        public bool? ChemicalsTestedLogged { get; set; }
        public bool? ClosingProceduresDiscussed { get; set; }
        public bool? StaffWearingUniform { get; set; }

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public string? UserName { get; set; }
        public string? LocationName { get; set; }

        public int SafetyCompliancePercentage
        {
            get
            {
                var safetyItems = new bool?[] {
                    PoolOpen, FacilityEntryProcedures, MainDrainVisible, AEDPresent,
                    RescueTubePresent, BackboardPresent, FirstAidKit, BloodbornePathogenKit,
                    HazMatKit, GateFenceSecured, EmergencyPhoneWorking, MSDS
                };

                int total = safetyItems.Length;
                int passed = 0;

                foreach (var item in safetyItems)
                {
                    if (item == true) passed++;
                }

                return total > 0 ? (int)((passed / (double)total) * 100) : 0;
            }
        }
    }
}
