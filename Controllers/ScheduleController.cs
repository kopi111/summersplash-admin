using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SummerSplashWeb.Services;
using SummerSplashWeb.Models;

namespace SummerSplashWeb.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;
        private readonly IUserService _userService;
        private readonly ILocationService _locationService;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(
            IScheduleService scheduleService,
            IUserService userService,
            ILocationService locationService,
            ILogger<ScheduleController> logger)
        {
            _scheduleService = scheduleService;
            _userService = userService;
            _locationService = locationService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string view = "employees", DateTime? date = null, int? userId = null, int? locationId = null)
        {
            try
            {
                var selectedDate = date ?? DateTime.Now;
                DateTime startDate;
                DateTime endDate;

                // Calculate date range based on view type
                switch (view.ToLower())
                {
                    case "calendar":
                        // Start of week (Sunday) and show 4 weeks
                        int diffCal = (7 + (selectedDate.DayOfWeek - DayOfWeek.Sunday)) % 7;
                        startDate = selectedDate.AddDays(-1 * diffCal).Date;
                        endDate = startDate.AddDays(27); // 4 weeks
                        break;
                    case "month":
                        startDate = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                        endDate = startDate.AddMonths(1).AddDays(-1);
                        break;
                    case "year":
                        startDate = new DateTime(selectedDate.Year, 1, 1);
                        endDate = new DateTime(selectedDate.Year, 12, 31);
                        break;
                    case "week":
                    case "employees":
                    default:
                        // Start of week (Sunday)
                        int diff = (7 + (selectedDate.DayOfWeek - DayOfWeek.Sunday)) % 7;
                        startDate = selectedDate.AddDays(-1 * diff).Date;
                        endDate = startDate.AddDays(6);
                        break;
                }

                var schedules = await _scheduleService.GetSchedulesAsync(startDate, endDate);

                // Apply filters
                if (userId.HasValue)
                {
                    schedules = schedules.Where(s => s.UserId == userId.Value).ToList();
                }

                if (locationId.HasValue)
                {
                    schedules = schedules.Where(s => s.LocationId == locationId.Value).ToList();
                }

                var users = await _userService.GetAllUsersAsync();
                var locations = await _locationService.GetAllLocationsAsync();

                ViewBag.View = view;
                ViewBag.SelectedDate = selectedDate;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.Schedules = schedules;
                ViewBag.Users = users;
                ViewBag.Locations = locations;
                ViewBag.FilterUserId = userId;
                ViewBag.FilterLocationId = locationId;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading schedules");
                ViewBag.Error = "Error loading schedule data. Please try again.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(DateTime? date = null, int? userId = null)
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var locations = await _locationService.GetAllLocationsAsync();

                ViewBag.Users = users;
                ViewBag.Locations = locations;
                ViewBag.SelectedDate = date ?? DateTime.Now;
                ViewBag.SelectedUserId = userId;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create schedule form");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Schedule schedule)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    schedule.CreatedAt = DateTime.Now;
                    var result = await _scheduleService.CreateScheduleAsync(schedule);

                    if (result)
                    {
                        TempData["Success"] = "Schedule created successfully!";
                        return RedirectToAction(nameof(Index), new { date = schedule.ScheduledDate });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create schedule. Please try again.");
                    }
                }

                var users = await _userService.GetAllUsersAsync();
                var locations = await _locationService.GetAllLocationsAsync();
                ViewBag.Users = users;
                ViewBag.Locations = locations;

                return View(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schedule");
                ModelState.AddModelError("", "Error creating schedule. Please try again.");

                var users = await _userService.GetAllUsersAsync();
                var locations = await _locationService.GetAllLocationsAsync();
                ViewBag.Users = users;
                ViewBag.Locations = locations;

                return View(schedule);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var schedule = await _scheduleService.GetScheduleByIdAsync(id);
                if (schedule == null)
                    return NotFound();

                var users = await _userService.GetAllUsersAsync();
                var locations = await _locationService.GetAllLocationsAsync();

                ViewBag.Users = users;
                ViewBag.Locations = locations;

                return View(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading schedule for edit");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Schedule schedule)
        {
            try
            {
                if (id != schedule.ScheduleId)
                    return BadRequest();

                if (ModelState.IsValid)
                {
                    var result = await _scheduleService.UpdateScheduleAsync(schedule);

                    if (result)
                    {
                        TempData["Success"] = "Schedule updated successfully!";
                        return RedirectToAction(nameof(Index), new { date = schedule.ScheduledDate });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update schedule. Please try again.");
                    }
                }

                var users = await _userService.GetAllUsersAsync();
                var locations = await _locationService.GetAllLocationsAsync();
                ViewBag.Users = users;
                ViewBag.Locations = locations;

                return View(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schedule");
                ModelState.AddModelError("", "Error updating schedule. Please try again.");

                var users = await _userService.GetAllUsersAsync();
                var locations = await _locationService.GetAllLocationsAsync();
                ViewBag.Users = users;
                ViewBag.Locations = locations;

                return View(schedule);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _scheduleService.DeleteScheduleAsync(id);

                if (result)
                {
                    TempData["Success"] = "Schedule deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete schedule. It may have already been deleted.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting schedule");
                TempData["Error"] = "Error deleting schedule. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSchedulesByDate(DateTime date)
        {
            try
            {
                var schedules = await _scheduleService.GetSchedulesAsync(date.Date, date.Date);
                return Json(schedules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedules by date");
                return Json(new { error = "Error loading schedules" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeSchedule(int userId, DateTime? startDate = null)
        {
            try
            {
                var selectedDate = startDate ?? DateTime.Now;
                int diff = (7 + (selectedDate.DayOfWeek - DayOfWeek.Sunday)) % 7;
                var weekStartDate = selectedDate.AddDays(-1 * diff).Date;
                var weekEndDate = weekStartDate.AddDays(6);

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound();

                var schedules = await _scheduleService.GetUserSchedulesAsync(userId, weekStartDate, weekEndDate);
                var locations = await _locationService.GetAllLocationsAsync();

                ViewBag.User = user;
                ViewBag.StartDate = weekStartDate;
                ViewBag.EndDate = weekEndDate;
                ViewBag.Schedules = schedules;
                ViewBag.Locations = locations;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee schedule for user {UserId}", userId);
                TempData["Error"] = "Error loading employee schedule. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateFourWeek(DateTime? startDate = null)
        {
            try
            {
                var selectedDate = startDate ?? DateTime.Now;
                int diff = (7 + (selectedDate.DayOfWeek - DayOfWeek.Sunday)) % 7;
                var weekStartDate = selectedDate.AddDays(-1 * diff).Date;

                var users = await _userService.GetAllUsersAsync();
                var locations = await _locationService.GetAllLocationsAsync();
                var existingSchedules = await _scheduleService.GetSchedulesAsync(weekStartDate, weekStartDate.AddDays(27));

                ViewBag.Users = users;
                ViewBag.Locations = locations;
                ViewBag.StartDate = weekStartDate;
                ViewBag.ExistingSchedules = existingSchedules;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading 4-week schedule creation form");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMultiDay(int userId, int locationId, int? supervisorId, DateTime startDate, TimeSpan startTime, TimeSpan endTime, string? notes, int numberOfDays)
        {
            try
            {
                int successCount = 0;
                int errorCount = 0;

                for (int i = 0; i < numberOfDays; i++)
                {
                    var schedule = new Schedule
                    {
                        UserId = userId,
                        LocationId = locationId,
                        SupervisorId = supervisorId,
                        ScheduledDate = startDate.AddDays(i),
                        StartTime = startTime,
                        EndTime = endTime,
                        Notes = notes,
                        CreatedAt = DateTime.Now
                    };

                    try
                    {
                        await _scheduleService.CreateScheduleAsync(schedule);
                        successCount++;
                    }
                    catch
                    {
                        errorCount++;
                    }
                }

                if (successCount > 0)
                {
                    TempData["Success"] = $"Successfully created {successCount} schedule(s)!";
                }

                if (errorCount > 0)
                {
                    TempData["Error"] = $"Failed to create {errorCount} schedule(s).";
                }

                return RedirectToAction(nameof(Index), new { date = startDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating multi-day schedules");
                TempData["Error"] = "Error creating schedules. Please try again.";
                return RedirectToAction(nameof(Create), new { date = startDate, userId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFourWeek(int userId, int locationId, int? supervisorId, DateTime startDate, List<DaySchedule> daySchedules)
        {
            try
            {
                int successCount = 0;
                int errorCount = 0;

                foreach (var daySchedule in daySchedules)
                {
                    if (daySchedule.IsScheduled && daySchedule.StartTime.HasValue && daySchedule.EndTime.HasValue)
                    {
                        var schedule = new Schedule
                        {
                            UserId = userId,
                            LocationId = locationId,
                            SupervisorId = supervisorId,
                            ScheduledDate = daySchedule.Date,
                            StartTime = daySchedule.StartTime.Value,
                            EndTime = daySchedule.EndTime.Value,
                            Notes = daySchedule.Notes,
                            CreatedAt = DateTime.Now
                        };

                        try
                        {
                            await _scheduleService.CreateScheduleAsync(schedule);
                            successCount++;
                        }
                        catch
                        {
                            errorCount++;
                        }
                    }
                }

                if (successCount > 0)
                {
                    TempData["Success"] = $"Successfully created {successCount} schedule(s)!";
                }

                if (errorCount > 0)
                {
                    TempData["Error"] = $"Failed to create {errorCount} schedule(s).";
                }

                return RedirectToAction(nameof(Index), new { view = "fourweeks", date = startDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating 4-week schedules");
                TempData["Error"] = "Error creating schedules. Please try again.";
                return RedirectToAction(nameof(CreateFourWeek), new { startDate });
            }
        }
    }
}
