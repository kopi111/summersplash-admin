using Microsoft.AspNetCore.Mvc;
using SummerSplashWeb.Services;
using SummerSplashWeb.Models;
using Dapper;
using System.Threading.Tasks;

namespace SummerSplashWeb.Controllers.Api
{
    /// <summary>
    /// Unified Mobile API - All endpoints for SummerSplash Field Mobile Application
    /// </summary>
    [ApiController]
    [Route("api/mobile")]
    [Produces("application/json")]
    public class MobileApiController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IDatabaseService _databaseService;
        private readonly IAnalyticsService _analyticsService;
        private readonly INotificationService _notificationService;
        private readonly IClockService _clockService;
        private readonly ILocationService _locationService;
        private readonly IScheduleService _scheduleService;
        private readonly IReportService _reportService;
        private readonly ILogger<MobileApiController> _logger;

        public MobileApiController(
            IAuthService authService,
            IUserService userService,
            IDatabaseService databaseService,
            IAnalyticsService analyticsService,
            INotificationService notificationService,
            IClockService clockService,
            ILocationService locationService,
            IScheduleService scheduleService,
            IReportService reportService,
            ILogger<MobileApiController> logger)
        {
            _authService = authService;
            _userService = userService;
            _databaseService = databaseService;
            _analyticsService = analyticsService;
            _notificationService = notificationService;
            _clockService = clockService;
            _locationService = locationService;
            _scheduleService = scheduleService;
            _reportService = reportService;
            _logger = logger;
        }

        #region Authentication

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="request">Registration details including name, email, and password</param>
        /// <returns>Success status and verification instructions</returns>
        /// <response code="200">Registration successful - verification code sent</response>
        /// <response code="400">Invalid request or email already registered</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                await _authService.RegisterAsync(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Password
                );

                return Ok(new
                {
                    success = true,
                    message = "Registration successful. Please check your email for the verification code.",
                    email = request.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Dev endpoint to approve user (REMOVE IN PRODUCTION)
        /// </summary>
        [HttpPost("dev-approve/{email}")]
        public async Task<IActionResult> DevApproveUser(string email)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();
                var sql = "UPDATE Users SET IsApproved = 1, IsActive = 1 WHERE Email = @Email";
                var rows = await connection.ExecuteAsync(sql, new { Email = email });
                return Ok(new { success = true, message = $"Approved {rows} user(s)" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Dev endpoint to create user directly (REMOVE IN PRODUCTION)
        /// </summary>
        [HttpPost("dev-create-user")]
        public async Task<IActionResult> DevCreateUser([FromBody] DevUserRequest request)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                // Check if user already exists
                var existingUser = await connection.QueryFirstOrDefaultAsync<int?>(
                    "SELECT UserId FROM Users WHERE Email = @Email",
                    new { request.Email });

                if (existingUser.HasValue)
                {
                    return Ok(new { success = true, userId = existingUser.Value, message = "User already exists" });
                }

                // Insert with specific UserId if provided
                string sql;
                if (request.UserId.HasValue)
                {
                    sql = @"SET IDENTITY_INSERT Users ON;
                            INSERT INTO Users (UserId, Email, FirstName, LastName, PasswordHash, PasswordSalt, Position, IsApproved, IsActive, EmailVerified, CreatedAt)
                            VALUES (@UserId, @Email, @FirstName, @LastName, @PasswordHash, @PasswordSalt, @Position, 1, 1, 1, GETDATE());
                            SET IDENTITY_INSERT Users OFF;
                            SELECT @UserId";
                }
                else
                {
                    sql = @"INSERT INTO Users (Email, FirstName, LastName, PasswordHash, PasswordSalt, Position, IsApproved, IsActive, EmailVerified, CreatedAt)
                            VALUES (@Email, @FirstName, @LastName, @PasswordHash, @PasswordSalt, @Position, 1, 1, 1, GETDATE());
                            SELECT CAST(SCOPE_IDENTITY() AS INT)";
                }

                var userId = await connection.ExecuteScalarAsync<int>(sql, new {
                    request.UserId,
                    request.Email,
                    FirstName = request.FirstName ?? "Test",
                    LastName = request.LastName ?? "User",
                    PasswordHash = "$2a$11$XZqJ5RMmEq3aBKw7YqYiN.demopasswordhash",
                    PasswordSalt = "$2a$11$XZqJ5RMmEq3aBKw7YqYiN.",
                    Position = request.Position ?? "Lifeguard"
                });

                return Ok(new { success = true, userId, message = "User created" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        public class DevUserRequest
        {
            public int? UserId { get; set; }
            public string Email { get; set; } = "";
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Position { get; set; }
        }

        /// <summary>
        /// Dev endpoint to create schedule for testing (REMOVE IN PRODUCTION)
        /// </summary>
        [HttpPost("dev-create-schedule")]
        public async Task<IActionResult> DevCreateSchedule([FromBody] DevScheduleRequest request)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();
                var sql = @"INSERT INTO Schedules (UserId, LocationId, ScheduledDate, StartTime, EndTime, Notes, CreatedAt)
                            VALUES (@UserId, @LocationId, @ScheduledDate, @StartTime, @EndTime, @Notes, GETDATE())";

                var rows = await connection.ExecuteAsync(sql, new {
                    request.UserId,
                    request.LocationId,
                    ScheduledDate = request.ScheduleDate ?? DateTime.Today,
                    StartTime = request.StartTime ?? "09:00",
                    EndTime = request.EndTime ?? "17:00",
                    Notes = request.Notes ?? "Test schedule"
                });
                return Ok(new { success = true, message = $"Created {rows} schedule(s)" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>User profile and authentication token if successful</returns>
        /// <response code="200">Login successful or verification required</response>
        /// <response code="401">Invalid credentials</response>
        /// <response code="400">Account not approved or other error</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _authService.LoginAsync(request.Email, request.Password);

                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Invalid email or password" });
                }

                // Generate JWT token (simplified for now)
                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        userId = user.UserId,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        role = user.Position ?? "Not Assigned",
                        emailVerified = user.EmailVerified,
                        isApproved = user.IsApproved,
                        isActive = user.IsActive
                    },
                    token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");

                // Check if email not verified
                if (ex.Message == "EMAIL_NOT_VERIFIED")
                {
                    return Ok(new
                    {
                        success = false,
                        requiresVerification = true,
                        message = "Please verify your email address",
                        email = request.Email
                    });
                }

                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Send verification code to email
        /// </summary>
        /// <param name="request">Email address to send code to</param>
        /// <returns>Success status</returns>
        /// <response code="200">Verification code sent successfully</response>
        /// <response code="400">Failed to send code or email not found</response>
        [HttpPost("send-verification-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendVerificationCode([FromBody] SendCodeRequest request)
        {
            try
            {
                var result = await _authService.SendVerificationCodeAsync(request.Email);

                if (result)
                {
                    return Ok(new { success = true, message = "Verification code sent successfully" });
                }

                return BadRequest(new { success = false, message = "Failed to send verification code" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification code");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Verify email with 7-digit code
        /// </summary>
        /// <param name="request">Email and verification code</param>
        /// <returns>Success status</returns>
        /// <response code="200">Email verified successfully</response>
        /// <response code="400">Invalid or expired code</response>
        [HttpPost("verify-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
        {
            try
            {
                var result = await _authService.VerifyCodeAsync(request.Email, request.Code);

                if (result)
                {
                    return Ok(new { success = true, message = "Email verified successfully" });
                }

                return BadRequest(new { success = false, message = "Invalid or expired verification code" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Verification failed");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Request password reset
        /// </summary>
        /// <param name="request">Email address</param>
        /// <returns>Success status</returns>
        /// <response code="200">Password reset instructions sent</response>
        /// <response code="400">Email not found</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var result = await _authService.RequestPasswordResetAsync(request.Email);

                if (result)
                {
                    return Ok(new { success = true, message = "Password reset instructions sent to your email" });
                }

                return BadRequest(new { success = false, message = "Email not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset request failed");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Reset password with token
        /// </summary>
        /// <param name="request">Reset token and new password</param>
        /// <returns>Success status</returns>
        /// <response code="200">Password reset successfully</response>
        /// <response code="400">Invalid or expired token</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);

                if (result)
                {
                    return Ok(new { success = true, message = "Password reset successfully" });
                }

                return BadRequest(new { success = false, message = "Invalid or expired reset token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset failed");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region User Profile & Status

        /// <summary>
        /// Get user profile information by email
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <returns>User profile data</returns>
        /// <response code="200">Profile retrieved successfully</response>
        /// <response code="404">User not found</response>
        /// <response code="400">Error retrieving profile</response>
        [HttpGet("profile/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUserProfile(string email)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                var user = await connection.QueryFirstOrDefaultAsync<User>(
                    @"SELECT UserId, FirstName, LastName, Email, Position, PhoneNumber,
                             IsActive, IsApproved, EmailVerified, HireDate, LastLoginAt, CreatedAt
                      FROM Users
                      WHERE Email = @Email",
                    new { Email = email }
                );

                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        userId = user.UserId,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        fullName = user.FullName,
                        role = user.Position ?? "Not Assigned",
                        phoneNumber = user.PhoneNumber,
                        emailVerified = user.EmailVerified,
                        isApproved = user.IsApproved,
                        isActive = user.IsActive,
                        hireDate = user.HireDate,
                        lastLoginAt = user.LastLoginAt,
                        createdAt = user.CreatedAt,
                        roleStatus = string.IsNullOrEmpty(user.Position) ? "Role not yet assigned" : user.Position
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user profile");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Check user verification and approval status
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <returns>User status including verification, approval, role assignment</returns>
        /// <response code="200">Status retrieved successfully</response>
        /// <response code="404">User not found</response>
        /// <response code="400">Error checking status</response>
        [HttpGet("status/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CheckUserStatus(string email)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                var user = await connection.QueryFirstOrDefaultAsync<User>(
                    @"SELECT UserId, Email, Position, IsApproved, EmailVerified, IsActive
                      FROM Users
                      WHERE Email = @Email",
                    new { Email = email }
                );

                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                var statusMessage = "";
                if (!user.EmailVerified)
                {
                    statusMessage = "Please verify your email";
                }
                else if (!user.IsApproved)
                {
                    statusMessage = "Your account is pending approval by an administrator";
                }
                else if (string.IsNullOrEmpty(user.Position))
                {
                    statusMessage = "Role not yet assigned";
                }
                else if (!user.IsActive)
                {
                    statusMessage = "Your account is inactive";
                }
                else
                {
                    statusMessage = "Active";
                }

                return Ok(new
                {
                    success = true,
                    userId = user.UserId,
                    emailVerified = user.EmailVerified,
                    isApproved = user.IsApproved,
                    isActive = user.IsActive,
                    hasRole = !string.IsNullOrEmpty(user.Position),
                    role = user.Position ?? "Not Assigned",
                    status = statusMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check user status");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Schedule

        /// <summary>
        /// Get user's schedule for a date range
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="startDate">Start date (optional, defaults to today)</param>
        /// <param name="endDate">End date (optional, defaults to 7 days from start)</param>
        /// <returns>List of scheduled shifts</returns>
        /// <response code="200">Schedule retrieved successfully</response>
        /// <response code="400">Error retrieving schedule</response>
        [HttpGet("schedule/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUserSchedule(int userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                // Default to current week if no dates provided
                if (!startDate.HasValue)
                {
                    startDate = DateTime.Today;
                }

                if (!endDate.HasValue)
                {
                    endDate = startDate.Value.AddDays(7);
                }

                var schedules = await connection.QueryAsync<dynamic>(
                    @"SELECT s.ScheduleId, s.UserId, s.LocationId, s.ScheduledDate,
                             s.StartTime, s.EndTime, s.Notes, s.CreatedAt,
                             l.LocationName, l.Address as LocationAddress
                      FROM Schedules s
                      LEFT JOIN JobLocations l ON s.LocationId = l.LocationId
                      WHERE s.UserId = @UserId
                        AND s.ScheduledDate >= @StartDate
                        AND s.ScheduledDate <= @EndDate
                      ORDER BY s.ScheduledDate, s.StartTime",
                    new { UserId = userId, StartDate = startDate, EndDate = endDate }
                );

                var scheduleList = schedules.ToList();

                if (!scheduleList.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        hasSchedule = false,
                        message = "No schedule at this time",
                        schedules = new List<object>()
                    });
                }

                return Ok(new
                {
                    success = true,
                    hasSchedule = true,
                    schedules = scheduleList.Select(s => new
                    {
                        scheduleId = s.ScheduleId,
                        date = s.ScheduledDate,
                        startTime = s.StartTime?.ToString(@"hh\:mm"),
                        endTime = s.EndTime?.ToString(@"hh\:mm"),
                        location = new
                        {
                            locationId = s.LocationId,
                            name = s.LocationName,
                            address = s.LocationAddress
                        },
                        notes = s.Notes
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user schedule");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Dashboard & Analytics

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        /// <returns>Dashboard stats including employees, locations, shifts, reports, etc.</returns>
        /// <response code="200">Statistics retrieved successfully</response>
        /// <response code="400">Error retrieving statistics</response>
        [HttpGet("dashboard/stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = await _analyticsService.GetDashboardStatsAsync();

                return Ok(new
                {
                    success = true,
                    stats = new
                    {
                        totalEmployees = stats.TotalEmployees,
                        activeEmployees = stats.ActiveEmployees,
                        pendingApprovals = stats.PendingApprovals,
                        totalLocations = stats.TotalLocations,
                        activeShifts = stats.ActiveShifts,
                        completedReportsToday = stats.CompletedReportsToday,
                        todayReports = stats.CompletedReportsToday,
                        pendingSupplyOrders = stats.PendingSupplyOrders,
                        averageCustomerRating = stats.AverageCustomerRating,
                        totalIncidentsThisMonth = stats.TotalIncidentsThisMonth,
                        totalHoursWorkedToday = stats.TotalHoursWorkedToday,
                        totalHoursToday = stats.TotalHoursWorkedToday,
                        scheduledShifts = stats.ScheduledShifts,
                        missedPunches = stats.MissedPunches,
                        overdueTrainings = stats.OverdueTrainings,
                        unreadNotifications = stats.UnreadNotifications
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get dashboard stats");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get user-specific dashboard with personalized stats
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Personalized dashboard statistics</returns>
        /// <response code="200">User dashboard retrieved successfully</response>
        /// <response code="400">Error retrieving user dashboard</response>
        [HttpGet("dashboard/user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUserDashboard(int userId)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                // Get today's schedule
                var todaySchedule = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT s.ScheduleId, s.ScheduledDate, s.StartTime, s.EndTime,
                             l.Name as LocationName, l.Address as LocationAddress,
                             sup.FirstName + ' ' + sup.LastName as SupervisorName
                      FROM Schedules s
                      LEFT JOIN JobLocations l ON s.LocationId = l.LocationId
                      LEFT JOIN Users sup ON s.SupervisorId = sup.UserId
                      WHERE s.UserId = @UserId AND s.ScheduledDate = CAST(GETDATE() AS DATE)",
                    new { UserId = userId }
                );

                // Get active clock record
                var activeClockIn = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT ClockRecordId, ClockInTime, LocationId
                      FROM ClockRecords
                      WHERE UserId = @UserId AND ClockOutTime IS NULL",
                    new { UserId = userId }
                );

                // Get this week's hours
                var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var weekHours = await connection.ExecuteScalarAsync<decimal>(
                    @"SELECT ISNULL(SUM(TotalHours), 0)
                      FROM ClockRecords
                      WHERE UserId = @UserId
                        AND ClockInTime >= @WeekStart",
                    new { UserId = userId, WeekStart = weekStart }
                );

                // Get unread notifications count
                var unreadNotifications = await _notificationService.GetUnreadCountAsync(userId);

                // Get pending tasks count
                var pendingTasks = await connection.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(*)
                      FROM ServiceTechReports
                      WHERE AssignedTo = @UserId AND Status = 'Pending'",
                    new { UserId = userId }
                );

                // Get overdue trainings for this user
                var overdueTrainings = await connection.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(*)
                      FROM TrainingAssignments
                      WHERE UserId = @UserId
                        AND DueDate < GETDATE()
                        AND Status != 'Completed'",
                    new { UserId = userId }
                );

                // Build schedule object
                object scheduleObj;
                if (todaySchedule != null)
                {
                    scheduleObj = new
                    {
                        hasSchedule = true,
                        scheduleId = (int?)todaySchedule.ScheduleId,
                        date = (DateTime?)todaySchedule.ScheduledDate,
                        startTime = todaySchedule.StartTime?.ToString(@"hh\:mm"),
                        endTime = todaySchedule.EndTime?.ToString(@"hh\:mm"),
                        location = new
                        {
                            name = (string)todaySchedule.LocationName,
                            address = (string)todaySchedule.LocationAddress
                        },
                        supervisor = (string)todaySchedule.SupervisorName
                    };
                }
                else
                {
                    scheduleObj = new
                    {
                        hasSchedule = false,
                        scheduleId = (int?)null,
                        date = (DateTime?)null,
                        startTime = (string)null,
                        endTime = (string)null,
                        location = new
                        {
                            name = (string)null,
                            address = (string)null
                        },
                        supervisor = (string)null
                    };
                }

                // Build clock status object
                object clockObj;
                if (activeClockIn != null)
                {
                    clockObj = new
                    {
                        isClockedIn = true,
                        clockInTime = (DateTime?)activeClockIn.ClockInTime,
                        locationId = (int?)activeClockIn.LocationId
                    };
                }
                else
                {
                    clockObj = new
                    {
                        isClockedIn = false,
                        clockInTime = (DateTime?)null,
                        locationId = (int?)null
                    };
                }

                return Ok(new
                {
                    success = true,
                    dashboard = new
                    {
                        todaySchedule = scheduleObj,
                        clockStatus = clockObj,
                        weeklyStats = new
                        {
                            hoursWorked = weekHours,
                            weekStart = weekStart,
                            weekEnd = weekStart.AddDays(6)
                        },
                        notifications = new
                        {
                            unreadCount = unreadNotifications
                        },
                        tasks = new
                        {
                            pendingCount = pendingTasks
                        },
                        training = new
                        {
                            overdueCount = overdueTrainings
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user dashboard for user {UserId}", userId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get top performers for a date range
        /// </summary>
        /// <param name="startDate">Start date (optional, defaults to 30 days ago)</param>
        /// <param name="endDate">End date (optional, defaults to today)</param>
        /// <param name="topN">Number of top performers to return (default 10)</param>
        /// <returns>List of top performing employees</returns>
        /// <response code="200">Top performers retrieved successfully</response>
        /// <response code="400">Error retrieving top performers</response>
        [HttpGet("dashboard/top-performers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTopPerformers([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int topN = 10)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today;

                var performers = await _analyticsService.GetTopPerformersAsync(start, end, topN);

                return Ok(new
                {
                    success = true,
                    dateRange = new
                    {
                        startDate = start,
                        endDate = end
                    },
                    topPerformers = performers.Select(p => new
                    {
                        userId = p.UserId,
                        userName = p.UserName,
                        tasksCompleted = p.TasksCompleted,
                        totalHoursWorked = p.TotalHoursWorked,
                        onTimeClockIns = p.OnTimeClockIns,
                        lateClockIns = p.LateClockIns,
                        onTimePercentage = p.OnTimePercentage,
                        customerRating = p.CustomerRatingAvg,
                        incidentCount = p.IncidentCount
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get top performers");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Notifications

        /// <summary>
        /// Get user notifications
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user notifications</returns>
        /// <response code="200">Notifications retrieved successfully</response>
        /// <response code="400">Error retrieving notifications</response>
        [HttpGet("notifications/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetNotifications(int userId)
        {
            try
            {
                var notifications = await _notificationService.GetUserNotificationsAsync(userId);

                return Ok(new
                {
                    success = true,
                    count = notifications.Count,
                    unreadCount = notifications.Count(n => !n.IsRead),
                    notifications = notifications.Select(n => new
                    {
                        notificationId = n.NotificationId,
                        title = n.Title,
                        message = n.Message,
                        type = n.Type,
                        isRead = n.IsRead,
                        actionUrl = n.ActionUrl,
                        createdAt = n.CreatedAt,
                        timeAgo = n.TimeAgo
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get notifications for user {UserId}", userId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <returns>Success status</returns>
        /// <response code="200">Notification marked as read</response>
        /// <response code="400">Error marking notification as read</response>
        [HttpPost("notifications/{notificationId}/mark-read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var result = await _notificationService.MarkAsReadAsync(notificationId);

                if (result)
                {
                    return Ok(new { success = true, message = "Notification marked as read" });
                }

                return BadRequest(new { success = false, message = "Failed to mark notification as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark notification {NotificationId} as read", notificationId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Clock In/Out

        /// <summary>
        /// Clock in for a shift
        /// </summary>
        /// <param name="request">Clock in request with user ID and location ID</param>
        /// <returns>Clock record created</returns>
        /// <response code="200">Clock in successful</response>
        /// <response code="400">Error clocking in</response>
        [HttpPost("clock/in")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ClockIn([FromBody] ClockInRequest request)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                // Check if user already has an active clock-in (not clocked out)
                var activeRecord = await connection.QueryFirstOrDefaultAsync<ClockRecord>(
                    "SELECT * FROM ClockRecords WHERE UserId = @UserId AND ClockOutTime IS NULL",
                    new { UserId = request.UserId }
                );

                if (activeRecord != null)
                {
                    return BadRequest(new { success = false, message = "You are already clocked in. Please clock out first." });
                }

                // Check if user already clocked out today (completed a shift today)
                var todayStart = DateTime.Today;
                var todayEnd = DateTime.Today.AddDays(1);
                var completedTodayRecord = await connection.QueryFirstOrDefaultAsync<ClockRecord>(
                    @"SELECT * FROM ClockRecords
                      WHERE UserId = @UserId
                        AND ClockOutTime IS NOT NULL
                        AND ClockInTime >= @TodayStart
                        AND ClockInTime < @TodayEnd",
                    new { UserId = request.UserId, TodayStart = todayStart, TodayEnd = todayEnd }
                );

                if (completedTodayRecord != null && !request.ForceOverride)
                {
                    return Ok(new
                    {
                        success = false,
                        requiresConfirmation = true,
                        message = "You have already clocked out today. Do you want to start a new shift?",
                        previousClockIn = completedTodayRecord.ClockInTime,
                        previousClockOut = completedTodayRecord.ClockOutTime
                    });
                }

                var clockRecord = new ClockRecord
                {
                    UserId = request.UserId,
                    LocationId = request.LocationId,
                    ClockInTime = DateTime.Now,
                    JobsiteNotes = request.Notes,
                    CreatedAt = DateTime.Now
                };

                var result = await _clockService.ClockInAsync(clockRecord);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Clocked in successfully",
                        clockInTime = clockRecord.ClockInTime
                    });
                }

                return BadRequest(new { success = false, message = "Failed to clock in" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clock in for user {UserId}", request.UserId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Clock out from a shift
        /// </summary>
        /// <param name="request">Clock out request with user ID</param>
        /// <returns>Clock out status</returns>
        /// <response code="200">Clock out successful</response>
        /// <response code="400">Error clocking out or no active shift</response>
        [HttpPost("clock/out")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ClockOut([FromBody] ClockOutRequest request)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                // Find active clock record
                var activeRecord = await connection.QueryFirstOrDefaultAsync<ClockRecord>(
                    "SELECT * FROM ClockRecords WHERE UserId = @UserId AND ClockOutTime IS NULL",
                    new { UserId = request.UserId }
                );

                if (activeRecord == null)
                {
                    return BadRequest(new { success = false, message = "No active clock-in found" });
                }

                var result = await _clockService.ClockOutAsync(activeRecord.RecordId);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Clocked out successfully",
                        clockOutTime = DateTime.Now,
                        totalHours = (DateTime.Now - activeRecord.ClockInTime).TotalHours
                    });
                }

                return BadRequest(new { success = false, message = "Failed to clock out" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clock out for user {UserId}", request.UserId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get all currently active shifts (employees clocked in)
        /// </summary>
        /// <returns>List of active shifts</returns>
        /// <response code="200">Active shifts retrieved successfully</response>
        /// <response code="400">Error retrieving active shifts</response>
        [HttpGet("clock/active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetActiveShifts()
        {
            try
            {
                var shifts = await _clockService.GetActiveShiftsAsync();

                return Ok(new
                {
                    success = true,
                    count = shifts.Count,
                    shifts = shifts.Select(s => new
                    {
                        recordId = s.RecordId,
                        userId = s.UserId,
                        userName = s.UserName,
                        locationId = s.LocationId,
                        locationName = s.LocationName,
                        clockInTime = s.ClockInTime
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get active shifts");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get user's clock records for a date range
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="startDate">Start date (optional, defaults to 7 days ago)</param>
        /// <param name="endDate">End date (optional, defaults to today)</param>
        /// <returns>List of clock records</returns>
        /// <response code="200">Clock records retrieved successfully</response>
        /// <response code="400">Error retrieving clock records</response>
        [HttpGet("clock/records/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetClockRecords(int userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-7);
                var end = endDate ?? DateTime.Today;

                var records = await _clockService.GetClockRecordsByUserAsync(userId, start, end);
                var totalHours = await _clockService.GetTotalHoursWorkedAsync(userId, start, end);

                return Ok(new
                {
                    success = true,
                    dateRange = new { startDate = start, endDate = end },
                    totalHours = totalHours,
                    records = records.Select(r => new
                    {
                        recordId = r.RecordId,
                        clockInTime = r.ClockInTime,
                        clockOutTime = r.ClockOutTime,
                        locationId = r.LocationId,
                        locationName = r.LocationName,
                        totalHours = r.TotalHours,
                        notes = r.JobsiteNotes
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get clock records for user {UserId}", userId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Check if user is currently clocked in
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Clock in status</returns>
        /// <response code="200">Status retrieved successfully</response>
        /// <response code="400">Error checking status</response>
        [HttpGet("clock/status/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetClockStatus(int userId)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                var activeRecord = await connection.QueryFirstOrDefaultAsync<ClockRecord>(
                    @"SELECT cr.*, l.Name as LocationName
                      FROM ClockRecords cr
                      LEFT JOIN JobLocations l ON cr.LocationId = l.LocationId
                      WHERE cr.UserId = @UserId AND cr.ClockOutTime IS NULL",
                    new { UserId = userId }
                );

                if (activeRecord != null)
                {
                    return Ok(new
                    {
                        success = true,
                        isClockedIn = true,
                        recordId = activeRecord.RecordId,
                        clockInTime = activeRecord.ClockInTime,
                        locationId = activeRecord.LocationId,
                        locationName = activeRecord.LocationName,
                        hoursWorked = (DateTime.Now - activeRecord.ClockInTime).TotalHours
                    });
                }

                return Ok(new
                {
                    success = true,
                    isClockedIn = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get clock status for user {UserId}", userId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Locations

        /// <summary>
        /// Get all job locations
        /// </summary>
        /// <returns>List of job locations</returns>
        /// <response code="200">Locations retrieved successfully</response>
        /// <response code="400">Error retrieving locations</response>
        [HttpGet("locations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetLocations()
        {
            try
            {
                var locations = await _locationService.GetAllLocationsAsync();

                return Ok(new
                {
                    success = true,
                    count = locations.Count,
                    locations = locations.Select(l => new
                    {
                        locationId = l.LocationId,
                        name = l.Name,
                        address = l.Address,
                        city = l.City,
                        state = l.State,
                        zipCode = l.ZipCode,
                        contactName = l.ContactName,
                        contactPhone = l.ContactPhone,
                        contactEmail = l.ContactEmail,
                        isActive = l.IsActive,
                        poolType = l.PoolType,
                        notes = l.Notes,
                        latitude = l.Latitude,
                        longitude = l.Longitude,
                        geofenceRadius = l.GeofenceRadius
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get locations");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Find nearby locations based on GPS coordinates
        /// </summary>
        /// <param name="latitude">User's latitude</param>
        /// <param name="longitude">User's longitude</param>
        /// <param name="maxDistance">Maximum distance in meters (default 500)</param>
        /// <returns>List of nearby locations with distance</returns>
        [HttpGet("locations/nearby")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetNearbyLocations(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] double maxDistance = 500)
        {
            try
            {
                var allLocations = await _locationService.GetAllLocationsAsync();
                var nearbyLocations = new List<object>();

                foreach (var loc in allLocations.Where(l => l.IsActive && l.Latitude.HasValue && l.Longitude.HasValue))
                {
                    var distance = CalculateDistance(latitude, longitude, loc.Latitude!.Value, loc.Longitude!.Value);
                    if (distance <= maxDistance)
                    {
                        nearbyLocations.Add(new
                        {
                            locationId = loc.LocationId,
                            name = loc.Name,
                            address = loc.Address,
                            latitude = loc.Latitude,
                            longitude = loc.Longitude,
                            geofenceRadius = loc.GeofenceRadius,
                            isActive = loc.IsActive,
                            distanceMeters = Math.Round(distance, 2)
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    locations = nearbyLocations.OrderBy(l => ((dynamic)l).distanceMeters).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get nearby locations");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth's radius in meters
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRad(double deg) => deg * Math.PI / 180;

        /// <summary>
        /// Get location details by ID
        /// </summary>
        /// <param name="locationId">Location ID</param>
        /// <returns>Location details</returns>
        /// <response code="200">Location retrieved successfully</response>
        /// <response code="404">Location not found</response>
        /// <response code="400">Error retrieving location</response>
        [HttpGet("locations/{locationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetLocation(int locationId)
        {
            try
            {
                var location = await _locationService.GetLocationByIdAsync(locationId);

                if (location == null)
                {
                    return NotFound(new { success = false, message = "Location not found" });
                }

                return Ok(new
                {
                    success = true,
                    location = new
                    {
                        locationId = location.LocationId,
                        name = location.Name,
                        address = location.Address,
                        city = location.City,
                        state = location.State,
                        zipCode = location.ZipCode,
                        contactName = location.ContactName,
                        contactPhone = location.ContactPhone,
                        contactEmail = location.ContactEmail,
                        isActive = location.IsActive,
                        poolType = location.PoolType,
                        poolSize = location.PoolSize,
                        notes = location.Notes,
                        createdAt = location.CreatedAt,
                        latitude = location.Latitude,
                        longitude = location.Longitude,
                        geofenceRadius = location.GeofenceRadius
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get location {LocationId}", locationId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Reports & Tasks

        /// <summary>
        /// Get recent reports
        /// </summary>
        /// <param name="count">Number of recent reports to retrieve (default 10)</param>
        /// <returns>List of recent service tech reports</returns>
        /// <response code="200">Reports retrieved successfully</response>
        /// <response code="400">Error retrieving reports</response>
        [HttpGet("reports/recent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRecentReports([FromQuery] int count = 10)
        {
            try
            {
                var reports = await _reportService.GetRecentReportsAsync(count);

                return Ok(new
                {
                    success = true,
                    count = reports.Count,
                    reports = reports.Select(r => new
                    {
                        reportId = r.ReportId,
                        userId = r.UserId,
                        technicianName = r.UserName,
                        locationId = r.LocationId,
                        locationName = r.LocationName,
                        serviceDate = r.ServiceDate,
                        workPerformed = r.WorkPerformed,
                        issuesFound = r.IssuesFound,
                        notes = r.Notes,
                        checklistCompletion = r.ChecklistCompletionPercentage,
                        createdAt = r.CreatedAt
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recent reports");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get reports for a date range
        /// </summary>
        /// <param name="startDate">Start date (optional, defaults to 7 days ago)</param>
        /// <param name="endDate">End date (optional, defaults to today)</param>
        /// <returns>List of service tech reports</returns>
        /// <response code="200">Reports retrieved successfully</response>
        /// <response code="400">Error retrieving reports</response>
        [HttpGet("reports")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetReports([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-7);
                var end = endDate ?? DateTime.Today;

                var reports = await _reportService.GetReportsByDateRangeAsync(start, end);

                return Ok(new
                {
                    success = true,
                    dateRange = new { startDate = start, endDate = end },
                    count = reports.Count,
                    reports = reports.Select(r => new
                    {
                        reportId = r.ReportId,
                        userId = r.UserId,
                        technicianName = r.UserName,
                        locationId = r.LocationId,
                        locationName = r.LocationName,
                        serviceDate = r.ServiceDate,
                        workPerformed = r.WorkPerformed,
                        issuesFound = r.IssuesFound,
                        suppliesNeeded = r.SuppliesNeeded,
                        notes = r.Notes,
                        checklistCompletion = r.ChecklistCompletionPercentage,
                        customerRating = r.CustomerRating,
                        createdAt = r.CreatedAt
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get reports");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get report by ID with full details
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <returns>Full report details</returns>
        /// <response code="200">Report retrieved successfully</response>
        /// <response code="404">Report not found</response>
        /// <response code="400">Error retrieving report</response>
        [HttpGet("reports/{reportId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetReport(int reportId)
        {
            try
            {
                var report = await _reportService.GetReportByIdAsync(reportId);

                if (report == null)
                {
                    return NotFound(new { success = false, message = "Report not found" });
                }

                var chemicalReadings = await _reportService.GetChemicalReadingsForReportAsync(reportId);

                return Ok(new
                {
                    success = true,
                    report = new
                    {
                        reportId = report.ReportId,
                        userId = report.UserId,
                        technicianName = report.UserName,
                        locationId = report.LocationId,
                        locationName = report.LocationName,
                        serviceDate = report.ServiceDate,
                        workPerformed = report.WorkPerformed,
                        issuesFound = report.IssuesFound,
                        recommendations = report.Recommendations,
                        suppliesNeeded = report.SuppliesNeeded,
                        notes = report.Notes,
                        checklistCompletion = report.ChecklistCompletionPercentage,
                        customerRating = report.CustomerRating,
                        customerFeedback = report.CustomerFeedback,
                        chemicalReadings = chemicalReadings,
                        createdAt = report.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get report {ReportId}", reportId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Helper Methods

        private string GenerateJwtToken(User user)
        {
            // For now, return a simple token. In production, use proper JWT generation
            return $"Bearer_{user.UserId}_{Guid.NewGuid()}";
        }

        #endregion
    }

    #region Request Models

    public class RegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class SendCodeRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyCodeRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ClockInRequest
    {
        public int UserId { get; set; }
        public int LocationId { get; set; }
        public string? Notes { get; set; }
        public bool ForceOverride { get; set; } = false;
    }

    public class ClockOutRequest
    {
        public int UserId { get; set; }
    }

    public class DevScheduleRequest
    {
        public int UserId { get; set; }
        public int LocationId { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? Notes { get; set; }
    }

    #endregion
}
