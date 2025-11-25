using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SummerSplashWeb.Models;

namespace SummerSplashWeb.Services
{
    // Clock Service
    public interface IClockService
    {
        Task<List<ClockRecord>> GetTodayClockRecordsAsync();
        Task<List<ClockRecord>> GetTodaysRecordsAsync();
        Task<List<ClockRecord>> GetClockRecordsByUserAsync(int userId, DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalHoursWorkedAsync(int userId, DateTime startDate, DateTime endDate);
        Task<List<ClockRecord>> GetActiveShiftsAsync();
        Task<bool> ClockInAsync(ClockRecord clockRecord);
        Task<bool> ClockOutAsync(int recordId);
    }

    public class ClockService : IClockService
    {
        private readonly IDatabaseService _databaseService;

        public ClockService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<ClockRecord>> GetTodayClockRecordsAsync()
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT cr.*, u.FirstName + ' ' + u.LastName AS UserName, jl.Name AS LocationName
                FROM ClockRecords cr
                INNER JOIN Users u ON cr.UserId = u.UserId
                INNER JOIN JobLocations jl ON cr.LocationId = jl.LocationId
                WHERE CAST(cr.ClockInTime AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY cr.ClockInTime DESC";

            var records = await connection.QueryAsync<ClockRecord>(sql);
            return records.ToList();
        }

        public async Task<List<ClockRecord>> GetClockRecordsByUserAsync(int userId, DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT cr.*, u.FirstName + ' ' + u.LastName AS UserName, jl.Name AS LocationName
                FROM ClockRecords cr
                INNER JOIN Users u ON cr.UserId = u.UserId
                INNER JOIN JobLocations jl ON cr.LocationId = jl.LocationId
                WHERE cr.UserId = @UserId
                  AND cr.ClockInTime >= @StartDate
                  AND cr.ClockInTime <= @EndDate
                ORDER BY cr.ClockInTime DESC";

            var records = await connection.QueryAsync<ClockRecord>(sql, new { UserId = userId, StartDate = startDate, EndDate = endDate });
            return records.ToList();
        }

        public async Task<decimal> GetTotalHoursWorkedAsync(int userId, DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT ISNULL(SUM(TotalHours), 0)
                FROM ClockRecords
                WHERE UserId = @UserId
                  AND ClockInTime >= @StartDate
                  AND ClockInTime <= @EndDate
                  AND TotalHours IS NOT NULL";

            var totalHours = await connection.ExecuteScalarAsync<decimal>(sql, new { UserId = userId, StartDate = startDate, EndDate = endDate });
            return totalHours;
        }

        public async Task<List<ClockRecord>> GetTodaysRecordsAsync()
        {
            return await GetTodayClockRecordsAsync();
        }

        public async Task<List<ClockRecord>> GetActiveShiftsAsync()
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT cr.*, u.FirstName + ' ' + u.LastName AS UserName, ISNULL(jl.Name, 'Unknown Location') AS LocationName
                FROM ClockRecords cr
                LEFT JOIN Users u ON cr.UserId = u.UserId
                LEFT JOIN JobLocations jl ON cr.LocationId = jl.LocationId
                WHERE cr.ClockOutTime IS NULL
                ORDER BY cr.ClockInTime DESC";

            var records = await connection.QueryAsync<ClockRecord>(sql);
            return records.ToList();
        }

        public async Task<bool> ClockInAsync(ClockRecord clockRecord)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                INSERT INTO ClockRecords (UserId, LocationId, ClockInTime, CreatedAt)
                VALUES (@UserId, @LocationId, @ClockInTime, @CreatedAt)";

            var result = await connection.ExecuteAsync(sql, clockRecord);
            return result > 0;
        }

        public async Task<bool> ClockOutAsync(int recordId)
        {
            using var connection = _databaseService.CreateConnection();

            // Get the clock-in time to calculate total hours
            var getRecordSql = "SELECT ClockInTime FROM ClockRecords WHERE RecordId = @RecordId";
            var clockInTime = await connection.QueryFirstOrDefaultAsync<DateTime?>(getRecordSql, new { RecordId = recordId });

            if (!clockInTime.HasValue)
                return false;

            var clockOutTime = DateTime.Now;
            var totalHours = (decimal)(clockOutTime - clockInTime.Value).TotalHours;

            var sql = @"
                UPDATE ClockRecords
                SET ClockOutTime = @ClockOutTime, TotalHours = @TotalHours
                WHERE RecordId = @RecordId";

            var result = await connection.ExecuteAsync(sql, new {
                RecordId = recordId,
                ClockOutTime = clockOutTime,
                TotalHours = totalHours
            });

            return result > 0;
        }
    }

    // Report Service
    public interface IReportService
    {
        Task<List<ServiceTechReport>> GetServiceTechReportsAsync(DateTime startDate, DateTime endDate);
        Task<List<SiteEvaluation>> GetSiteEvaluationsAsync(DateTime startDate, DateTime endDate);
        Task<int> GetReportsCountTodayAsync();
        Task<List<ServiceTechReport>> GetRecentReportsAsync(int count);
        Task<ServiceTechReport?> GetReportByIdAsync(int reportId);
        Task<List<ChemicalReading>> GetChemicalReadingsForReportAsync(int reportId);
        Task<List<ServiceTechReport>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> CreateServiceTechReportAsync(ServiceTechReport report);
        Task<bool> UpdateServiceTechReportAsync(ServiceTechReport report);
        Task<bool> DeleteServiceTechReportAsync(int reportId);
        Task<int> AddChemicalReadingAsync(ChemicalReading reading);
        Task<int> AddPhotoAsync(Photo photo);
        Task<List<ServiceTechReport>> GetReportsByLocationAsync(int locationId, DateTime startDate, DateTime endDate);
    }

    public class ReportService : IReportService
    {
        private readonly IDatabaseService _databaseService;

        public ReportService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<ServiceTechReport>> GetServiceTechReportsAsync(DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT sr.*, u.FirstName + ' ' + u.LastName AS UserName, jl.Name AS LocationName
                FROM ServiceTechReports sr
                INNER JOIN Users u ON sr.UserId = u.UserId
                INNER JOIN JobLocations jl ON sr.LocationId = jl.LocationId
                WHERE sr.CreatedAt >= @StartDate AND sr.CreatedAt <= @EndDate
                ORDER BY sr.CreatedAt DESC";

            var reports = await connection.QueryAsync<ServiceTechReport>(sql, new { StartDate = startDate, EndDate = endDate });
            return reports.ToList();
        }

        public async Task<List<SiteEvaluation>> GetSiteEvaluationsAsync(DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT se.*, u.FirstName + ' ' + u.LastName AS UserName, jl.Name AS LocationName
                FROM SiteEvaluations se
                INNER JOIN Users u ON se.UserId = u.UserId
                INNER JOIN JobLocations jl ON se.LocationId = jl.LocationId
                WHERE se.CreatedAt >= @StartDate AND se.CreatedAt <= @EndDate
                ORDER BY se.CreatedAt DESC";

            var evals = await connection.QueryAsync<SiteEvaluation>(sql, new { StartDate = startDate, EndDate = endDate });
            return evals.ToList();
        }

        public async Task<int> GetReportsCountTodayAsync()
        {
            using var connection = _databaseService.CreateConnection();
            var sql = "SELECT COUNT(*) FROM ServiceTechReports WHERE CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE)";
            return await connection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<List<ServiceTechReport>> GetRecentReportsAsync(int count)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = $@"
                SELECT TOP {count} sr.*, u.FirstName + ' ' + u.LastName AS TechName, jl.Name AS LocationName,
                       cr.ClockInTime AS ServiceDate
                FROM ServiceTechReports sr
                INNER JOIN Users u ON sr.UserId = u.UserId
                INNER JOIN JobLocations jl ON sr.LocationId = jl.LocationId
                LEFT JOIN ClockRecords cr ON sr.ClockRecordId = cr.RecordId
                ORDER BY cr.ClockInTime DESC";

            var reports = await connection.QueryAsync<ServiceTechReport>(sql);
            return reports.ToList();
        }

        public async Task<ServiceTechReport?> GetReportByIdAsync(int reportId)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT sr.*, u.FirstName + ' ' + u.LastName AS TechName, jl.Name AS LocationName,
                       cr.ClockInTime AS ServiceDate
                FROM ServiceTechReports sr
                INNER JOIN Users u ON sr.UserId = u.UserId
                INNER JOIN JobLocations jl ON sr.LocationId = jl.LocationId
                LEFT JOIN ClockRecords cr ON sr.ClockRecordId = cr.RecordId
                WHERE sr.ReportId = @ReportId";

            return await connection.QueryFirstOrDefaultAsync<ServiceTechReport>(sql, new { ReportId = reportId });
        }

        public async Task<List<ChemicalReading>> GetChemicalReadingsForReportAsync(int reportId)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT * FROM ChemicalReadings
                WHERE ReportId = @ReportId
                ORDER BY ReadingTime";

            var readings = await connection.QueryAsync<ChemicalReading>(sql, new { ReportId = reportId });
            return readings.ToList();
        }

        public async Task<List<ServiceTechReport>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT sr.*, u.FirstName + ' ' + u.LastName AS TechName, jl.Name AS LocationName
                FROM ServiceTechReports sr
                INNER JOIN Users u ON sr.TechId = u.UserId
                INNER JOIN JobLocations jl ON sr.LocationId = jl.LocationId
                WHERE sr.ServiceDate >= @StartDate AND sr.ServiceDate <= @EndDate
                ORDER BY sr.ServiceDate DESC, sr.ServiceTime DESC";

            var reports = await connection.QueryAsync<ServiceTechReport>(sql, new { StartDate = startDate, EndDate = endDate });
            return reports.ToList();
        }

        public async Task<int> CreateServiceTechReportAsync(ServiceTechReport report)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                INSERT INTO ServiceTechReports (
                    UserId, LocationId, ClockRecordId, ServiceDate, ClockInTime, ClockOutTime,
                    PoolVacuumed, PoolBrushed, SkimmerBasketsEmptied, PumpBasketsEmptied,
                    FilterCleaned, ChemicalsAdded, PoolDeckCleaned, EquipmentChecked,
                    GateLocksChecked, SafetyEquipmentInspected, WaterLevelChecked, DebrisRemoved,
                    TilesInspected, DrainCoversChecked, LightsChecked, SignageChecked,
                    FurnitureArranged, RestroomsCleaned, PoolGateLocked,
                    SuppliesNeeded, ReportSentTo, CustomerRating, CustomerFeedback, Notes, CreatedAt
                )
                VALUES (
                    @UserId, @LocationId, @ClockRecordId, @ServiceDate, @ClockInTime, @ClockOutTime,
                    @PoolVacuumed, @PoolBrushed, @SkimmerBasketsEmptied, @PumpBasketsEmptied,
                    @FilterCleaned, @ChemicalsAdded, @PoolDeckCleaned, @EquipmentChecked,
                    @GateLocksChecked, @SafetyEquipmentInspected, @WaterLevelChecked, @DebrisRemoved,
                    @TilesInspected, @DrainCoversChecked, @LightsChecked, @SignageChecked,
                    @FurnitureArranged, @RestroomsCleaned, @PoolGateLocked,
                    @SuppliesNeeded, @ReportSentTo, @CustomerRating, @CustomerFeedback, @Notes, @CreatedAt
                );
                SELECT CAST(SCOPE_IDENTITY() as int)";

            var reportId = await connection.ExecuteScalarAsync<int>(sql, report);
            return reportId;
        }

        public async Task<bool> UpdateServiceTechReportAsync(ServiceTechReport report)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                UPDATE ServiceTechReports SET
                    UserId = @UserId, LocationId = @LocationId, ClockRecordId = @ClockRecordId,
                    ServiceDate = @ServiceDate, ClockInTime = @ClockInTime, ClockOutTime = @ClockOutTime,
                    PoolVacuumed = @PoolVacuumed, PoolBrushed = @PoolBrushed,
                    SkimmerBasketsEmptied = @SkimmerBasketsEmptied, PumpBasketsEmptied = @PumpBasketsEmptied,
                    FilterCleaned = @FilterCleaned, ChemicalsAdded = @ChemicalsAdded,
                    PoolDeckCleaned = @PoolDeckCleaned, EquipmentChecked = @EquipmentChecked,
                    GateLocksChecked = @GateLocksChecked, SafetyEquipmentInspected = @SafetyEquipmentInspected,
                    WaterLevelChecked = @WaterLevelChecked, DebrisRemoved = @DebrisRemoved,
                    TilesInspected = @TilesInspected, DrainCoversChecked = @DrainCoversChecked,
                    LightsChecked = @LightsChecked, SignageChecked = @SignageChecked,
                    FurnitureArranged = @FurnitureArranged, RestroomsCleaned = @RestroomsCleaned,
                    PoolGateLocked = @PoolGateLocked, SuppliesNeeded = @SuppliesNeeded,
                    ReportSentTo = @ReportSentTo, CustomerRating = @CustomerRating,
                    CustomerFeedback = @CustomerFeedback, Notes = @Notes, UpdatedAt = @UpdatedAt
                WHERE ReportId = @ReportId";

            var result = await connection.ExecuteAsync(sql, report);
            return result > 0;
        }

        public async Task<bool> DeleteServiceTechReportAsync(int reportId)
        {
            using var connection = _databaseService.CreateConnection();

            // Delete related chemical readings first
            await connection.ExecuteAsync("DELETE FROM ChemicalReadings WHERE ReportId = @ReportId", new { ReportId = reportId });

            // Delete related photos
            await connection.ExecuteAsync("DELETE FROM Photos WHERE ReportId = @ReportId", new { ReportId = reportId });

            // Delete the report
            var sql = "DELETE FROM ServiceTechReports WHERE ReportId = @ReportId";
            var result = await connection.ExecuteAsync(sql, new { ReportId = reportId });
            return result > 0;
        }

        public async Task<int> AddChemicalReadingAsync(ChemicalReading reading)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                INSERT INTO ChemicalReadings (
                    ReportId, PoolType, ChlorineBromine, pH, CalciumHardness,
                    TotalAlkalinity, CyanuricAcid, Salt, Temperature, CreatedAt
                )
                VALUES (
                    @ReportId, @PoolType, @ChlorineBromine, @pH, @CalciumHardness,
                    @TotalAlkalinity, @CyanuricAcid, @Salt, @Temperature, @CreatedAt
                );
                SELECT CAST(SCOPE_IDENTITY() as int)";

            var readingId = await connection.ExecuteScalarAsync<int>(sql, reading);
            return readingId;
        }

        public async Task<int> AddPhotoAsync(Photo photo)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                INSERT INTO Photos (ReportId, PhotoUrl, PhotoTimestamp, Description, CreatedAt)
                VALUES (@ReportId, @PhotoUrl, @PhotoTimestamp, @Description, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            var photoId = await connection.ExecuteScalarAsync<int>(sql, photo);
            return photoId;
        }

        public async Task<List<ServiceTechReport>> GetReportsByLocationAsync(int locationId, DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT sr.*, u.FirstName + ' ' + u.LastName AS UserName, jl.Name AS LocationName, jl.Address AS LocationAddress
                FROM ServiceTechReports sr
                INNER JOIN Users u ON sr.UserId = u.UserId
                INNER JOIN JobLocations jl ON sr.LocationId = jl.LocationId
                WHERE sr.LocationId = @LocationId
                  AND sr.ServiceDate >= @StartDate
                  AND sr.ServiceDate <= @EndDate
                ORDER BY sr.ServiceDate DESC";

            var reports = await connection.QueryAsync<ServiceTechReport>(sql, new { LocationId = locationId, StartDate = startDate, EndDate = endDate });
            return reports.ToList();
        }
    }

    // Location Service
    public interface ILocationService
    {
        Task<List<JobLocation>> GetAllLocationsAsync();
        Task<JobLocation?> GetLocationByIdAsync(int locationId);
        Task<bool> CreateLocationAsync(JobLocation location);
        Task<bool> UpdateLocationAsync(JobLocation location);
        Task<bool> DeleteLocationAsync(int locationId);
    }

    public class LocationService : ILocationService
    {
        private readonly IDatabaseService _databaseService;

        public LocationService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<JobLocation>> GetAllLocationsAsync()
        {
            using var connection = _databaseService.CreateConnection();
            var sql = "SELECT * FROM JobLocations WHERE IsActive = 1 ORDER BY Name";
            var locations = await connection.QueryAsync<JobLocation>(sql);
            return locations.ToList();
        }

        public async Task<JobLocation?> GetLocationByIdAsync(int locationId)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = "SELECT * FROM JobLocations WHERE LocationId = @LocationId";
            return await connection.QueryFirstOrDefaultAsync<JobLocation>(sql, new { LocationId = locationId });
        }

        public async Task<bool> CreateLocationAsync(JobLocation location)
        {
            using var connection = _databaseService.CreateConnection();
            // Using only columns that exist in the remote database
            var sql = @"
                INSERT INTO JobLocations (Name, Address, ContactPerson, ContactPhone, ContactEmail, Latitude, Longitude, IsActive, CreatedAt)
                VALUES (@Name, @Address, @ContactName, @ContactPhone, @ContactEmail, @Latitude, @Longitude, 1, GETDATE())";

            var result = await connection.ExecuteAsync(sql, location);
            return result > 0;
        }

        public async Task<bool> UpdateLocationAsync(JobLocation location)
        {
            using var connection = _databaseService.CreateConnection();
            // Using only columns that exist in the remote database
            var sql = @"
                UPDATE JobLocations
                SET Name = @Name, Address = @Address,
                    ContactPerson = @ContactName, ContactPhone = @ContactPhone, ContactEmail = @ContactEmail,
                    Latitude = @Latitude, Longitude = @Longitude,
                    IsActive = @IsActive
                WHERE LocationId = @LocationId";

            var result = await connection.ExecuteAsync(sql, location);
            return result > 0;
        }

        public async Task<bool> DeleteLocationAsync(int locationId)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = "UPDATE JobLocations SET IsActive = 0 WHERE LocationId = @LocationId";
            var result = await connection.ExecuteAsync(sql, new { LocationId = locationId });
            return result > 0;
        }
    }

    // Schedule Service
    public interface IScheduleService
    {
        Task<List<Schedule>> GetSchedulesAsync(DateTime startDate, DateTime endDate);
        Task<List<Schedule>> GetUserSchedulesAsync(int userId, DateTime startDate, DateTime endDate);
        Task<Schedule?> GetScheduleByIdAsync(int scheduleId);
        Task<bool> CreateScheduleAsync(Schedule schedule);
        Task<bool> UpdateScheduleAsync(Schedule schedule);
        Task<bool> DeleteScheduleAsync(int scheduleId);
    }

    public class ScheduleService : IScheduleService
    {
        private readonly IDatabaseService _databaseService;

        public ScheduleService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<Schedule>> GetSchedulesAsync(DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT s.*, u.FirstName + ' ' + u.LastName AS UserName,
                       jl.Name AS LocationName,
                       sup.FirstName + ' ' + sup.LastName AS SupervisorName
                FROM Schedules s
                INNER JOIN Users u ON s.UserId = u.UserId
                INNER JOIN JobLocations jl ON s.LocationId = jl.LocationId
                LEFT JOIN Users sup ON s.SupervisorId = sup.UserId
                WHERE s.ScheduledDate >= @StartDate AND s.ScheduledDate <= @EndDate
                ORDER BY s.ScheduledDate, s.StartTime";

            var schedules = await connection.QueryAsync<Schedule>(sql, new { StartDate = startDate, EndDate = endDate });
            return schedules.ToList();
        }

        public async Task<List<Schedule>> GetUserSchedulesAsync(int userId, DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT s.*, u.FirstName + ' ' + u.LastName AS UserName,
                       jl.Name AS LocationName,
                       sup.FirstName + ' ' + sup.LastName AS SupervisorName
                FROM Schedules s
                INNER JOIN Users u ON s.UserId = u.UserId
                INNER JOIN JobLocations jl ON s.LocationId = jl.LocationId
                LEFT JOIN Users sup ON s.SupervisorId = sup.UserId
                WHERE s.UserId = @UserId
                  AND s.ScheduledDate >= @StartDate AND s.ScheduledDate <= @EndDate
                ORDER BY s.ScheduledDate, s.StartTime";

            var schedules = await connection.QueryAsync<Schedule>(sql, new { UserId = userId, StartDate = startDate, EndDate = endDate });
            return schedules.ToList();
        }

        public async Task<Schedule?> GetScheduleByIdAsync(int scheduleId)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT s.*, u.FirstName + ' ' + u.LastName AS UserName,
                       jl.Name AS LocationName,
                       sup.FirstName + ' ' + sup.LastName AS SupervisorName
                FROM Schedules s
                INNER JOIN Users u ON s.UserId = u.UserId
                INNER JOIN JobLocations jl ON s.LocationId = jl.LocationId
                LEFT JOIN Users sup ON s.SupervisorId = sup.UserId
                WHERE s.ScheduleId = @ScheduleId";

            return await connection.QueryFirstOrDefaultAsync<Schedule>(sql, new { ScheduleId = scheduleId });
        }

        public async Task<bool> CreateScheduleAsync(Schedule schedule)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                INSERT INTO Schedules (UserId, LocationId, ScheduledDate, StartTime, EndTime, SupervisorId, Notes, CreatedAt)
                VALUES (@UserId, @LocationId, @ScheduledDate, @StartTime, @EndTime, @SupervisorId, @Notes, GETDATE())";

            var result = await connection.ExecuteAsync(sql, schedule);
            return result > 0;
        }

        public async Task<bool> UpdateScheduleAsync(Schedule schedule)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                UPDATE Schedules
                SET UserId = @UserId, LocationId = @LocationId, ScheduledDate = @ScheduledDate,
                    StartTime = @StartTime, EndTime = @EndTime, SupervisorId = @SupervisorId, Notes = @Notes
                WHERE ScheduleId = @ScheduleId";

            var result = await connection.ExecuteAsync(sql, schedule);
            return result > 0;
        }

        public async Task<bool> DeleteScheduleAsync(int scheduleId)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = "DELETE FROM Schedules WHERE ScheduleId = @ScheduleId";
            var result = await connection.ExecuteAsync(sql, new { ScheduleId = scheduleId });
            return result > 0;
        }
    }

    // Training Service
    public interface ITrainingService
    {
        Task<List<TrainingTopic>> GetAllTrainingTopicsAsync();
        Task<List<Training>> GetAllTrainingsAsync();
        Task<List<TrainingAssignment>> GetUserAssignmentsAsync(int userId);
        Task<int> GetOverdueTrainingsCountAsync();
    }

    public class TrainingService : ITrainingService
    {
        private readonly IDatabaseService _databaseService;

        public TrainingService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<TrainingTopic>> GetAllTrainingTopicsAsync()
        {
            using var connection = _databaseService.CreateConnection();
            var sql = "SELECT * FROM TrainingTopics WHERE IsActive = 1 ORDER BY Category, TopicName";
            var topics = await connection.QueryAsync<TrainingTopic>(sql);
            return topics.ToList();
        }

        public async Task<List<TrainingAssignment>> GetUserAssignmentsAsync(int userId)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT ta.*, tt.TopicName,
                       u.FirstName + ' ' + u.LastName AS UserName,
                       ab.FirstName + ' ' + ab.LastName AS AssignedByName
                FROM TrainingAssignments ta
                INNER JOIN TrainingTopics tt ON ta.TopicId = tt.TopicId
                INNER JOIN Users u ON ta.UserId = u.UserId
                INNER JOIN Users ab ON ta.AssignedBy = ab.UserId
                WHERE ta.UserId = @UserId
                ORDER BY ta.DueDate";

            var assignments = await connection.QueryAsync<TrainingAssignment>(sql, new { UserId = userId });
            return assignments.ToList();
        }

        public async Task<List<Training>> GetAllTrainingsAsync()
        {
            // Training table doesn't exist yet - returning empty list
            return await Task.FromResult(new List<Training>());
        }

        public async Task<int> GetOverdueTrainingsCountAsync()
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT COUNT(*)
                FROM TrainingAssignments
                WHERE DueDate < GETDATE() AND Status != 'Completed'";

            return await connection.ExecuteScalarAsync<int>(sql);
        }
    }

    // Notification Service
    public interface INotificationService
    {
        Task<List<Notification>> GetUserNotificationsAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId);
    }

    public class NotificationService : INotificationService
    {
        private readonly IDatabaseService _databaseService;

        public NotificationService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int userId)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = @"
                SELECT TOP 50 *
                FROM Notifications
                WHERE UserId = @UserId
                ORDER BY CreatedAt DESC";

            var notifications = await connection.QueryAsync<Notification>(sql, new { UserId = userId });
            return notifications.ToList();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = "SELECT COUNT(*) FROM Notifications WHERE UserId = @UserId AND IsRead = 0";
            return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = "UPDATE Notifications SET IsRead = 1 WHERE NotificationId = @NotificationId";
            var result = await connection.ExecuteAsync(sql, new { NotificationId = notificationId });
            return result > 0;
        }
    }

    // Analytics Service
    public interface IAnalyticsService
    {
        Task<DashboardStats> GetDashboardStatsAsync();
        Task<List<PerformanceMetric>> GetTopPerformersAsync(DateTime startDate, DateTime endDate, int topN = 10);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly IDatabaseService _databaseService;

        public AnalyticsService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            using var connection = _databaseService.CreateConnection();

            var stats = new DashboardStats
            {
                TotalEmployees = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users"),
                ActiveEmployees = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users WHERE IsActive = 1 AND IsApproved = 1"),
                PendingApprovals = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users WHERE IsApproved = 0"),
                TotalLocations = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM JobLocations WHERE IsActive = 1"),
                ActiveShifts = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM ClockRecords WHERE ClockOutTime IS NULL"),
                CompletedReportsToday = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM ServiceTechReports WHERE CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE)"),
                PendingSupplyOrders = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM SupplyOrders WHERE Status = 'Pending'"),
                AverageCustomerRating = await connection.ExecuteScalarAsync<decimal>("SELECT ISNULL(AVG(CAST(Rating AS DECIMAL(5,2))), 0) FROM CustomerFeedback WHERE CreatedAt >= DATEADD(month, -1, GETDATE())"),
                TotalHoursWorkedToday = await connection.ExecuteScalarAsync<decimal>("SELECT ISNULL(SUM(TotalHours), 0) FROM ClockRecords WHERE CAST(ClockInTime AS DATE) = CAST(GETDATE() AS DATE)"),
                OverdueTrainings = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM TrainingAssignments WHERE DueDate < GETDATE() AND Status != 'Completed'"),
                UnreadNotifications = 0 // Will be set per user
            };

            return stats;
        }

        public async Task<List<PerformanceMetric>> GetTopPerformersAsync(DateTime startDate, DateTime endDate, int topN = 10)
        {
            using var connection = _databaseService.CreateConnection();
            var sql = $@"
                SELECT TOP {topN} pm.*, u.FirstName + ' ' + u.LastName AS UserName
                FROM PerformanceMetrics pm
                INNER JOIN Users u ON pm.UserId = u.UserId
                WHERE pm.MetricDate >= @StartDate AND pm.MetricDate <= @EndDate
                ORDER BY pm.CustomerRatingAvg DESC, pm.TasksCompleted DESC";

            var metrics = await connection.QueryAsync<PerformanceMetric>(sql, new { StartDate = startDate, EndDate = endDate });
            return metrics.ToList();
        }
    }
}
