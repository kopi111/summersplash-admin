using Microsoft.AspNetCore.Mvc;
using Dapper;
using SummerSplashWeb.Services;
using System;
using System.Threading.Tasks;

namespace SummerSplashWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<DataController> _logger;

        public DataController(IDatabaseService databaseService, ILogger<DataController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        [HttpPost("seed")]
        public async Task<IActionResult> SeedDummyData()
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                _logger.LogInformation("Starting dummy data insertion...");

                // Clear existing data (except admin)
                await connection.ExecuteAsync("DELETE FROM ServiceTechReports");
                await connection.ExecuteAsync("DELETE FROM ClockRecords");
                await connection.ExecuteAsync("DELETE FROM JobLocations");
                await connection.ExecuteAsync("DELETE FROM Users WHERE Email != 'admin@summersplash.com'");

                // Insert Users
                string passwordHash = "$2a$11$XZqJ5RMmEq3aBKw7YqYiN.T7VZ8p2g6nY5vQm9gKlDx4fHwE8rTxy"; // Employee123!
                string passwordSalt = "$2a$11$XZqJ5RMmEq3aBKw7YqYiN.";

                var userSql = @"INSERT INTO Users (FirstName, LastName, Email, PasswordHash, PasswordSalt, Position, IsActive, IsApproved, CreatedAt)
                                VALUES (@FirstName, @LastName, @Email, @PasswordHash, @PasswordSalt, @Position, 1, 1, @CreatedAt)";

                await connection.ExecuteAsync(userSql, new { FirstName = "Michael", LastName = "Johnson", Email = "michael.johnson@summersplash.com", PasswordHash = passwordHash, PasswordSalt = passwordSalt, Position = "Pool Technician", CreatedAt = DateTime.Now.AddDays(-120) });
                await connection.ExecuteAsync(userSql, new { FirstName = "Sarah", LastName = "Williams", Email = "sarah.williams@summersplash.com", PasswordHash = passwordHash, PasswordSalt = passwordSalt, Position = "Pool Technician", CreatedAt = DateTime.Now.AddDays(-90) });
                await connection.ExecuteAsync(userSql, new { FirstName = "David", LastName = "Martinez", Email = "david.martinez@summersplash.com", PasswordHash = passwordHash, PasswordSalt = passwordSalt, Position = "Senior Technician", CreatedAt = DateTime.Now.AddDays(-150) });
                await connection.ExecuteAsync(userSql, new { FirstName = "Emily", LastName = "Davis", Email = "emily.davis@summersplash.com", PasswordHash = passwordHash, PasswordSalt = passwordSalt, Position = "Pool Technician", CreatedAt = DateTime.Now.AddDays(-60) });
                await connection.ExecuteAsync(userSql, new { FirstName = "James", LastName = "Brown", Email = "james.brown@summersplash.com", PasswordHash = passwordHash, PasswordSalt = passwordSalt, Position = "Pool Technician", CreatedAt = DateTime.Now.AddDays(-75) });
                await connection.ExecuteAsync(userSql, new { FirstName = "Jessica", LastName = "Garcia", Email = "jessica.garcia@summersplash.com", PasswordHash = passwordHash, PasswordSalt = passwordSalt, Position = "Team Lead", CreatedAt = DateTime.Now.AddDays(-200) });
                await connection.ExecuteAsync(userSql, new { FirstName = "Christopher", LastName = "Rodriguez", Email = "chris.rodriguez@summersplash.com", PasswordHash = passwordHash, PasswordSalt = passwordSalt, Position = "Pool Technician", CreatedAt = DateTime.Now.AddDays(-45) });
                await connection.ExecuteAsync(userSql, new { FirstName = "Amanda", LastName = "Wilson", Email = "amanda.wilson@summersplash.com", PasswordHash = passwordHash, PasswordSalt = passwordSalt, Position = "Pool Technician", CreatedAt = DateTime.Now.AddDays(-30) });
                await connection.ExecuteAsync(userSql, new { FirstName = "Robert", LastName = "Taylor", Email = "robert.taylor@summersplash.com", PasswordHash = passwordHash, PasswordSalt = passwordSalt, Position = "Senior Technician", CreatedAt = DateTime.Now.AddDays(-180) });

                // Insert Locations
                var locationSql = @"INSERT INTO JobLocations (Name, Address, City, State, ZipCode, ContactName, ContactPhone, ContactEmail, PoolType, PoolSize, IsActive, CreatedAt)
                                    VALUES (@Name, @Address, @City, @State, @ZipCode, @ContactName, @ContactPhone, @ContactEmail, @PoolType, @PoolSize, 1, @CreatedAt)";

                await connection.ExecuteAsync(locationSql, new { Name = "Sunset Community Pool", Address = "123 Main Street", City = "Phoenix", State = "AZ", ZipCode = "85001", ContactName = "John Parker", ContactPhone = "602-555-0101", ContactEmail = "john.parker@sunsetpool.com", PoolType = "Commercial", PoolSize = "50m Olympic", CreatedAt = DateTime.Now.AddDays(-200) });
                await connection.ExecuteAsync(locationSql, new { Name = "Riverside Apartments", Address = "456 River Road", City = "Scottsdale", State = "AZ", ZipCode = "85251", ContactName = "Maria Gonzalez", ContactPhone = "480-555-0202", ContactEmail = "maria@riverside.com", PoolType = "Residential", PoolSize = "25m Standard", CreatedAt = DateTime.Now.AddDays(-180) });
                await connection.ExecuteAsync(locationSql, new { Name = "Desert Oasis Resort", Address = "789 Palm Avenue", City = "Tempe", State = "AZ", ZipCode = "85281", ContactName = "Robert Chen", ContactPhone = "480-555-0303", ContactEmail = "rchen@desertoasis.com", PoolType = "Resort", PoolSize = "40m Lagoon", CreatedAt = DateTime.Now.AddDays(-150) });
                await connection.ExecuteAsync(locationSql, new { Name = "Greenfield HOA", Address = "321 Oak Street", City = "Mesa", State = "AZ", ZipCode = "85201", ContactName = "Susan Miller", ContactPhone = "480-555-0404", ContactEmail = "susan@greenfieldhoa.com", PoolType = "Community", PoolSize = "30m L-Shape", CreatedAt = DateTime.Now.AddDays(-120) });
                await connection.ExecuteAsync(locationSql, new { Name = "Pine Valley Fitness", Address = "654 Pine Drive", City = "Chandler", State = "AZ", ZipCode = "85224", ContactName = "David Thompson", ContactPhone = "480-555-0505", ContactEmail = "david@pinevalley.com", PoolType = "Commercial", PoolSize = "25m Lap Pool", CreatedAt = DateTime.Now.AddDays(-90) });
                await connection.ExecuteAsync(locationSql, new { Name = "Lakeside Estates", Address = "987 Lake View Blvd", City = "Gilbert", State = "AZ", ZipCode = "85234", ContactName = "Patricia White", ContactPhone = "480-555-0606", ContactEmail = "patricia@lakeside.com", PoolType = "Residential", PoolSize = "20m Freeform", CreatedAt = DateTime.Now.AddDays(-75) });
                await connection.ExecuteAsync(locationSql, new { Name = "Sunny Days Daycare", Address = "147 Sunshine Lane", City = "Glendale", State = "AZ", ZipCode = "85301", ContactName = "Linda Harris", ContactPhone = "623-555-0707", ContactEmail = "linda@sunnydaycare.com", PoolType = "Commercial", PoolSize = "15m Kiddie", CreatedAt = DateTime.Now.AddDays(-60) });
                await connection.ExecuteAsync(locationSql, new { Name = "Vista Grande Resort", Address = "258 Vista Road", City = "Peoria", State = "AZ", ZipCode = "85345", ContactName = "Michael Brown", ContactPhone = "623-555-0808", ContactEmail = "mbrown@vistagrand.com", PoolType = "Resort", PoolSize = "35m Resort", CreatedAt = DateTime.Now.AddDays(-45) });

                // Get IDs for relationships
                var users = await connection.QueryAsync<dynamic>("SELECT UserId, Email FROM Users");
                var locations = await connection.QueryAsync<dynamic>("SELECT LocationId, Name FROM JobLocations");

                // Insert sample clock records (past completed + today's active shifts)
                var clockSql = @"INSERT INTO ClockRecords (UserId, LocationId, ClockInTime, ClockOutTime, CreatedAt)
                                 VALUES (@UserId, @LocationId, @ClockInTime, @ClockOutTime, @CreatedAt)";

                // Add some completed shifts from past days
                foreach (var user in users.Take(5))
                {
                    var loc = locations.FirstOrDefault();
                    if (loc != null)
                    {
                        await connection.ExecuteAsync(clockSql, new {
                            UserId = user.UserId,
                            LocationId = loc.LocationId,
                            ClockInTime = DateTime.Now.AddDays(-7).Date.AddHours(8),
                            ClockOutTime = DateTime.Now.AddDays(-7).Date.AddHours(17),
                            CreatedAt = DateTime.Now.AddDays(-7)
                        });
                    }
                }

                // Insert today's active shifts (no clock out time)
                var activeShiftSql = @"INSERT INTO ClockRecords (UserId, LocationId, ClockInTime, CreatedAt)
                                       VALUES (@UserId, @LocationId, @ClockInTime, @CreatedAt)";

                int userIndex = 0;
                foreach (var user in users.Take(3))
                {
                    var loc = locations.Skip(userIndex).FirstOrDefault();
                    if (loc != null)
                    {
                        await connection.ExecuteAsync(activeShiftSql, new {
                            UserId = user.UserId,
                            LocationId = loc.LocationId,
                            ClockInTime = DateTime.Now.Date.AddHours(8 + userIndex),
                            CreatedAt = DateTime.Now
                        });
                    }
                    userIndex++;
                }

                // Get counts
                var userCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users");
                var locationCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM JobLocations");
                var clockCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM ClockRecords");
                var activeShifts = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM ClockRecords WHERE ClockOutTime IS NULL");

                _logger.LogInformation("Dummy data insertion completed successfully!");

                return Ok(new
                {
                    success = true,
                    message = "Dummy data inserted successfully!",
                    summary = new
                    {
                        totalUsers = userCount,
                        totalLocations = locationCount,
                        totalClockRecords = clockCount,
                        activeShifts = activeShifts
                    },
                    note = "All employees can login with password: Employee123!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting dummy data");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("add-locations")]
        public async Task<IActionResult> AddSampleLocations()
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                _logger.LogInformation("Adding sample locations...");

                var locationSql = @"
                    INSERT INTO JobLocations (Name, Address, IsActive, CreatedAt)
                    VALUES (@Name, @Address, @IsActive, @CreatedAt)";

                await connection.ExecuteAsync(locationSql, new { Name = "Main Office", Address = "123 Main St", IsActive = true, CreatedAt = DateTime.Now });
                await connection.ExecuteAsync(locationSql, new { Name = "Downtown Pool", Address = "456 Downtown Ave", IsActive = true, CreatedAt = DateTime.Now });
                await connection.ExecuteAsync(locationSql, new { Name = "Westside Recreation Center", Address = "789 West Blvd", IsActive = true, CreatedAt = DateTime.Now });
                await connection.ExecuteAsync(locationSql, new { Name = "Eastside Aquatic Center", Address = "321 East Dr", IsActive = true, CreatedAt = DateTime.Now });
                await connection.ExecuteAsync(locationSql, new { Name = "North Community Pool", Address = "654 North Rd", IsActive = true, CreatedAt = DateTime.Now });

                var locationCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM JobLocations");

                _logger.LogInformation("Sample locations added successfully!");

                return Ok(new
                {
                    success = true,
                    message = "Sample locations added successfully!",
                    totalLocations = locationCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding sample locations");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("create-report-tables")]
        public async Task<IActionResult> CreateReportTables()
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                _logger.LogInformation("Creating report tables...");

                // Drop existing tables if they exist (in reverse order due to foreign keys)
                await connection.ExecuteAsync("IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Photos') DROP TABLE Photos");
                await connection.ExecuteAsync("IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ChemicalReadings') DROP TABLE ChemicalReadings");
                await connection.ExecuteAsync("IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ServiceTechReports') DROP TABLE ServiceTechReports");

                // Create ServiceTechReports table
                var createServiceTechReportsTable = @"
                    BEGIN
                        CREATE TABLE ServiceTechReports (
                            ReportId INT PRIMARY KEY IDENTITY(1,1),
                            UserId INT NOT NULL,
                            LocationId INT NOT NULL,
                            ClockRecordId INT NULL,
                            ServiceDate DATETIME NOT NULL,
                            ClockInTime DATETIME NULL,
                            ClockOutTime DATETIME NULL,
                            ServiceType NVARCHAR(100) NULL,
                            WorkPerformed NVARCHAR(MAX) NULL,
                            ChemicalsAddedNotes NVARCHAR(MAX) NULL,
                            IssuesFound NVARCHAR(MAX) NULL,
                            Recommendations NVARCHAR(MAX) NULL,
                            PoolVacuumed BIT NOT NULL DEFAULT 0,
                            PoolBrushed BIT NOT NULL DEFAULT 0,
                            SkimmerBasketsEmptied BIT NOT NULL DEFAULT 0,
                            PumpBasketsEmptied BIT NOT NULL DEFAULT 0,
                            FilterCleaned BIT NOT NULL DEFAULT 0,
                            ChemicalsAdded BIT NOT NULL DEFAULT 0,
                            PoolDeckCleaned BIT NOT NULL DEFAULT 0,
                            EquipmentChecked BIT NOT NULL DEFAULT 0,
                            GateLocksChecked BIT NOT NULL DEFAULT 0,
                            SafetyEquipmentInspected BIT NOT NULL DEFAULT 0,
                            WaterLevelChecked BIT NOT NULL DEFAULT 0,
                            DebrisRemoved BIT NOT NULL DEFAULT 0,
                            TilesInspected BIT NOT NULL DEFAULT 0,
                            DrainCoversChecked BIT NOT NULL DEFAULT 0,
                            LightsChecked BIT NOT NULL DEFAULT 0,
                            SignageChecked BIT NOT NULL DEFAULT 0,
                            FurnitureArranged BIT NOT NULL DEFAULT 0,
                            RestroomsCleaned BIT NOT NULL DEFAULT 0,
                            PoolGateLocked BIT NOT NULL DEFAULT 0,
                            SuppliesNeeded NVARCHAR(MAX) NULL,
                            ReportSentTo NVARCHAR(50) NULL,
                            CustomerRating INT NULL,
                            CustomerFeedback NVARCHAR(MAX) NULL,
                            Notes NVARCHAR(MAX) NULL,
                            CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                            UpdatedAt DATETIME NULL,
                            FOREIGN KEY (UserId) REFERENCES Users(UserId),
                            FOREIGN KEY (LocationId) REFERENCES JobLocations(LocationId)
                        );
                    END";

                await connection.ExecuteAsync(createServiceTechReportsTable);

                // Create ChemicalReadings table
                var createChemicalReadingsTable = @"
                    BEGIN
                        CREATE TABLE ChemicalReadings (
                            ReadingId INT PRIMARY KEY IDENTITY(1,1),
                            ReportId INT NOT NULL,
                            PoolType NVARCHAR(50) NOT NULL DEFAULT 'Main pool',
                            ChlorineBromine DECIMAL(5,2) NULL,
                            pH DECIMAL(4,2) NULL,
                            CalciumHardness DECIMAL(6,2) NULL,
                            TotalAlkalinity DECIMAL(6,2) NULL,
                            CyanuricAcid DECIMAL(6,2) NULL,
                            Salt DECIMAL(8,2) NULL,
                            Temperature DECIMAL(5,2) NULL,
                            ReadingTime DATETIME NOT NULL DEFAULT GETDATE(),
                            CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                            FOREIGN KEY (ReportId) REFERENCES ServiceTechReports(ReportId) ON DELETE CASCADE
                        );
                    END";

                await connection.ExecuteAsync(createChemicalReadingsTable);

                // Create Photos table
                var createPhotosTable = @"
                    BEGIN
                        CREATE TABLE Photos (
                            PhotoId INT PRIMARY KEY IDENTITY(1,1),
                            ReportId INT NOT NULL,
                            PhotoUrl NVARCHAR(500) NOT NULL,
                            PhotoTimestamp DATETIME NOT NULL,
                            Description NVARCHAR(500) NULL,
                            CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                            FOREIGN KEY (ReportId) REFERENCES ServiceTechReports(ReportId) ON DELETE CASCADE
                        );
                    END";

                await connection.ExecuteAsync(createPhotosTable);

                _logger.LogInformation("Report tables created successfully!");

                return Ok(new
                {
                    success = true,
                    message = "Report tables created successfully!",
                    tables = new[] { "ServiceTechReports", "ChemicalReadings", "Photos" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating report tables");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("seed-reports")]
        public async Task<IActionResult> SeedReports()
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                _logger.LogInformation("Starting report data seeding...");

                // First, ensure tables exist
                await CreateReportTables();

                // Get users and locations
                var users = await connection.QueryAsync<dynamic>("SELECT UserId FROM Users WHERE Position LIKE '%Tech%' OR Position LIKE '%Service%'");
                var locations = await connection.QueryAsync<dynamic>("SELECT LocationId FROM JobLocations WHERE IsActive = 1");

                var userList = users.ToList();
                var locationList = locations.ToList();

                if (!userList.Any() || !locationList.Any())
                {
                    return BadRequest(new { success = false, message = "No users or locations found. Please seed users and locations first." });
                }

                var random = new Random();

                // Create 15 sample reports
                for (int i = 0; i < 15; i++)
                {
                    var userId = userList[random.Next(userList.Count)].UserId;
                    var locationId = locationList[random.Next(locationList.Count)].LocationId;
                    var daysAgo = random.Next(0, 30);
                    var serviceDate = DateTime.Now.AddDays(-daysAgo);

                    var reportSql = @"
                        INSERT INTO ServiceTechReports (
                            UserId, LocationId, ServiceDate, ClockInTime, ClockOutTime,
                            PoolVacuumed, PoolBrushed, SkimmerBasketsEmptied, PumpBasketsEmptied,
                            FilterCleaned, ChemicalsAdded, PoolDeckCleaned, EquipmentChecked,
                            GateLocksChecked, SafetyEquipmentInspected, WaterLevelChecked, DebrisRemoved,
                            TilesInspected, DrainCoversChecked, LightsChecked, SignageChecked,
                            FurnitureArranged, RestroomsCleaned, PoolGateLocked,
                            SuppliesNeeded, ReportSentTo, Notes, CreatedAt
                        )
                        VALUES (
                            @UserId, @LocationId, @ServiceDate, @ClockInTime, @ClockOutTime,
                            @PoolVacuumed, @PoolBrushed, @SkimmerBasketsEmptied, @PumpBasketsEmptied,
                            @FilterCleaned, @ChemicalsAdded, @PoolDeckCleaned, @EquipmentChecked,
                            @GateLocksChecked, @SafetyEquipmentInspected, @WaterLevelChecked, @DebrisRemoved,
                            @TilesInspected, @DrainCoversChecked, @LightsChecked, @SignageChecked,
                            @FurnitureArranged, @RestroomsCleaned, @PoolGateLocked,
                            @SuppliesNeeded, @ReportSentTo, @Notes, @CreatedAt
                        );
                        SELECT CAST(SCOPE_IDENTITY() as int)";

                    var clockIn = serviceDate.Date.AddHours(8);
                    var clockOut = serviceDate.Date.AddHours(12 + random.Next(0, 5));

                    var reportId = await connection.ExecuteScalarAsync<int>(reportSql, new
                    {
                        UserId = userId,
                        LocationId = locationId,
                        ServiceDate = serviceDate,
                        ClockInTime = clockIn,
                        ClockOutTime = clockOut,
                        PoolVacuumed = random.Next(0, 2) == 1,
                        PoolBrushed = random.Next(0, 2) == 1,
                        SkimmerBasketsEmptied = random.Next(0, 2) == 1,
                        PumpBasketsEmptied = random.Next(0, 2) == 1,
                        FilterCleaned = random.Next(0, 2) == 1,
                        ChemicalsAdded = random.Next(0, 2) == 1,
                        PoolDeckCleaned = random.Next(0, 2) == 1,
                        EquipmentChecked = random.Next(0, 2) == 1,
                        GateLocksChecked = random.Next(0, 2) == 1,
                        SafetyEquipmentInspected = random.Next(0, 2) == 1,
                        WaterLevelChecked = random.Next(0, 2) == 1,
                        DebrisRemoved = random.Next(0, 2) == 1,
                        TilesInspected = random.Next(0, 2) == 1,
                        DrainCoversChecked = random.Next(0, 2) == 1,
                        LightsChecked = random.Next(0, 2) == 1,
                        SignageChecked = random.Next(0, 2) == 1,
                        FurnitureArranged = random.Next(0, 2) == 1,
                        RestroomsCleaned = random.Next(0, 2) == 1,
                        PoolGateLocked = true, // Always locked for safety
                        SuppliesNeeded = random.Next(0, 3) == 0 ? "Chlorine tablets, pH test strips" : null,
                        ReportSentTo = new[] { "Me", "Manager", "Customer" }[random.Next(0, 3)],
                        Notes = random.Next(0, 3) == 0 ? "Pool service completed successfully. All systems operating normally." : null,
                        CreatedAt = serviceDate
                    });

                    // Add chemical readings for this report
                    var poolTypes = new[] { "Main pool", "Wading Pool", "Spa" };
                    var poolType = poolTypes[random.Next(0, poolTypes.Length)];

                    var chemicalSql = @"
                        INSERT INTO ChemicalReadings (
                            ReportId, PoolType, ChlorineBromine, pH, CalciumHardness,
                            TotalAlkalinity, CyanuricAcid, Salt, Temperature, ReadingTime, CreatedAt
                        )
                        VALUES (
                            @ReportId, @PoolType, @ChlorineBromine, @pH, @CalciumHardness,
                            @TotalAlkalinity, @CyanuricAcid, @Salt, @Temperature, @ReadingTime, @CreatedAt
                        )";

                    await connection.ExecuteAsync(chemicalSql, new
                    {
                        ReportId = reportId,
                        PoolType = poolType,
                        ChlorineBromine = Math.Round((decimal)(random.NextDouble() * 5 + 1), 2), // 1-6 ppm
                        pH = Math.Round((decimal)(random.NextDouble() * 1.5 + 7.0), 1), // 7.0-8.5
                        CalciumHardness = random.Next(150, 400), // 150-400 ppm
                        TotalAlkalinity = random.Next(60, 180), // 60-180 ppm
                        CyanuricAcid = random.Next(20, 100), // 20-100 ppm
                        Salt = poolType == "Spa" ? (int?)null : random.Next(2500, 3500), // 2500-3500 ppm for salt pools
                        Temperature = Math.Round((decimal)(random.NextDouble() * 10 + 75), 1), // 75-85°F
                        ReadingTime = serviceDate.AddHours(9),
                        CreatedAt = serviceDate
                    });
                }

                var reportCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM ServiceTechReports");
                var readingCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM ChemicalReadings");

                _logger.LogInformation("Report seeding completed successfully!");

                return Ok(new
                {
                    success = true,
                    message = "Report data seeded successfully!",
                    summary = new
                    {
                        totalReports = reportCount,
                        totalChemicalReadings = readingCount,
                        note = "15 sample service tech reports created with chemical readings"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding report data");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                using var connection = _databaseService.CreateConnection();
                var result = await connection.ExecuteScalarAsync<int>("SELECT 1");
                return Ok(new { success = true, message = "Database connection successful", result = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection failed");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                using var connection = _databaseService.CreateConnection();
                var users = await connection.QueryAsync<dynamic>("SELECT UserId, FirstName, LastName, Email, Position, IsActive, IsApproved, CreatedAt FROM Users ORDER BY CreatedAt DESC");
                return Ok(users.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations()
        {
            try
            {
                using var connection = _databaseService.CreateConnection();
                var locations = await connection.QueryAsync<dynamic>("SELECT * FROM JobLocations ORDER BY Name");
                return Ok(locations.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching locations");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("schedules")]
        public async Task<IActionResult> GetSchedules()
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                // First check if table exists and get columns
                var tableExists = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Schedules'");

                if (tableExists == 0)
                {
                    return Ok(new { success = true, message = "Schedules table does not exist", schedules = new List<object>() });
                }

                // Get columns in Schedules table
                var columns = await connection.QueryAsync<string>(
                    "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Schedules'");
                var columnList = columns.ToList();

                // Use ShiftDate if it exists, otherwise use Date or just select all
                string dateColumn = columnList.Contains("ShiftDate") ? "ShiftDate" :
                                   columnList.Contains("Date") ? "Date" :
                                   columnList.Contains("StartDate") ? "StartDate" : "CreatedAt";

                var schedules = await connection.QueryAsync<dynamic>($@"
                    SELECT s.*, u.FirstName, u.LastName, l.Name as LocationName
                    FROM Schedules s
                    LEFT JOIN Users u ON s.UserId = u.UserId
                    LEFT JOIN JobLocations l ON s.LocationId = l.LocationId
                    ORDER BY s.{dateColumn} DESC");
                return Ok(schedules.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching schedules");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("check-schedule-columns")]
        public async Task<IActionResult> CheckScheduleColumns()
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                var tableExists = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Schedules'");

                if (tableExists == 0)
                {
                    return Ok(new { success = true, tableExists = false, columns = new List<string>() });
                }

                var columns = await connection.QueryAsync<string>(
                    "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Schedules' ORDER BY ORDINAL_POSITION");
                return Ok(new { success = true, tableExists = true, columns = columns.ToList() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("clock-records")]
        public async Task<IActionResult> GetClockRecords()
        {
            try
            {
                using var connection = _databaseService.CreateConnection();
                var records = await connection.QueryAsync<dynamic>(@"
                    SELECT TOP 50 c.*, u.FirstName, u.LastName, l.Name as LocationName
                    FROM ClockRecords c
                    LEFT JOIN Users u ON c.UserId = u.UserId
                    LEFT JOIN JobLocations l ON c.LocationId = l.LocationId
                    ORDER BY c.ClockInTime DESC");
                return Ok(records.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching clock records");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                var totalUsers = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users");
                var activeUsers = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users WHERE IsActive = 1");
                var totalLocations = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM JobLocations");
                var activeShifts = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM ClockRecords WHERE ClockOutTime IS NULL");

                return Ok(new {
                    success = true,
                    totalUsers,
                    activeUsers,
                    totalLocations,
                    activeShifts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard stats");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("check-user-columns")]
        public async Task<IActionResult> CheckUserColumns()
        {
            try
            {
                using var connection = _databaseService.CreateConnection();
                var columns = await connection.QueryAsync<string>(
                    "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' ORDER BY ORDINAL_POSITION");
                return Ok(new { success = true, columns = columns.ToList() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("add-geofencing-columns")]
        public async Task<IActionResult> AddGeofencingColumns()
        {
            var errors = new List<string>();
            var successes = new List<string>();

            try
            {
                using var connection = _databaseService.CreateConnection();

                _logger.LogInformation("Adding geofencing columns to JobLocations table...");

                // Check if table exists
                var tableExists = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'JobLocations'");

                if (tableExists == 0)
                {
                    // Create the table
                    await connection.ExecuteAsync(@"
                        CREATE TABLE JobLocations (
                            LocationId INT IDENTITY(1,1) PRIMARY KEY,
                            Name NVARCHAR(200) NOT NULL,
                            Address NVARCHAR(500) NULL,
                            City NVARCHAR(100) NULL,
                            State NVARCHAR(2) NULL,
                            ZipCode NVARCHAR(10) NULL,
                            ContactName NVARCHAR(100) NULL,
                            ContactPhone NVARCHAR(20) NULL,
                            ContactEmail NVARCHAR(100) NULL,
                            PoolType NVARCHAR(50) NULL,
                            PoolSize NVARCHAR(50) NULL,
                            Notes NVARCHAR(MAX) NULL,
                            Latitude FLOAT NULL,
                            Longitude FLOAT NULL,
                            GeofenceRadius INT NOT NULL DEFAULT 100,
                            IsActive BIT NOT NULL DEFAULT 1,
                            CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
                        )");
                    successes.Add("Created JobLocations table");
                }
                else
                {
                    successes.Add("JobLocations table exists");

                    // Add missing columns one at a time - with proper error capture
                    var columnsToAdd = new Dictionary<string, string>
                    {
                        { "City", "NVARCHAR(100) NULL" },
                        { "State", "NVARCHAR(2) NULL" },
                        { "ZipCode", "NVARCHAR(10) NULL" },
                        { "ContactName", "NVARCHAR(100) NULL" },
                        { "ContactPhone", "NVARCHAR(20) NULL" },
                        { "ContactEmail", "NVARCHAR(100) NULL" },
                        { "PoolType", "NVARCHAR(50) NULL" },
                        { "PoolSize", "NVARCHAR(50) NULL" },
                        { "Notes", "NVARCHAR(MAX) NULL" },
                        { "Latitude", "FLOAT NULL" },
                        { "Longitude", "FLOAT NULL" },
                        { "GeofenceRadius", "INT NOT NULL DEFAULT 100" },
                        { "IsActive", "BIT NOT NULL DEFAULT 1" },
                        { "CreatedAt", "DATETIME NOT NULL DEFAULT GETDATE()" }
                    };

                    foreach (var col in columnsToAdd)
                    {
                        try
                        {
                            // Check if column exists
                            var colExists = await connection.ExecuteScalarAsync<object>(
                                $"SELECT COL_LENGTH('JobLocations', '{col.Key}')");

                            if (colExists == null)
                            {
                                await connection.ExecuteAsync($"ALTER TABLE JobLocations ADD {col.Key} {col.Value}");
                                successes.Add($"Added column {col.Key}");
                            }
                            else
                            {
                                successes.Add($"Column {col.Key} already exists");
                            }
                        }
                        catch (Exception colEx)
                        {
                            errors.Add($"Error adding {col.Key}: {colEx.Message}");
                        }
                    }
                }

                // Get actual columns now
                var actualColumns = await connection.QueryAsync<string>(
                    "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JobLocations' ORDER BY ORDINAL_POSITION");

                return Ok(new
                {
                    success = errors.Count == 0,
                    message = errors.Count == 0 ? "All columns verified!" : "Some columns failed to add",
                    actualColumns = actualColumns.ToList(),
                    successes = successes,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding geofencing columns");
                return StatusCode(500, new { success = false, error = ex.Message, errors = errors, successes = successes });
            }
        }
    }
}
