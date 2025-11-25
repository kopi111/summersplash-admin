using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SummerSplashWeb.Services;
using SummerSplashWeb.Models;

namespace SummerSplashWeb.Controllers
{
    [Authorize]
    public class ClockController : Controller
    {
        private readonly IClockService _clockService;
        private readonly IUserService _userService;
        private readonly ILocationService _locationService;
        private readonly ILogger<ClockController> _logger;

        public ClockController(
            IClockService clockService,
            IUserService userService,
            ILocationService locationService,
            ILogger<ClockController> logger)
        {
            _clockService = clockService;
            _userService = userService;
            _locationService = locationService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var activeShifts = await _clockService.GetActiveShiftsAsync();
                var todaysRecords = await _clockService.GetTodaysRecordsAsync();
                var locations = await _locationService.GetAllLocationsAsync();

                ViewBag.Users = users;
                ViewBag.ActiveShifts = activeShifts;
                ViewBag.TodaysRecords = todaysRecords;
                ViewBag.Locations = locations;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading clock records");
                ViewBag.Error = "Error loading clock data. Please try again.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeClockHistory(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound();

                var start = startDate ?? DateTime.Now.AddDays(-7);
                var end = endDate ?? DateTime.Now;

                var clockRecords = await _clockService.GetClockRecordsByUserAsync(userId, start, end);
                var totalHours = await _clockService.GetTotalHoursWorkedAsync(userId, start, end);
                var locations = await _locationService.GetAllLocationsAsync();

                ViewBag.User = user;
                ViewBag.StartDate = start;
                ViewBag.EndDate = end;
                ViewBag.ClockRecords = clockRecords;
                ViewBag.TotalHours = totalHours;
                ViewBag.Locations = locations;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading clock history for user {UserId}", userId);
                TempData["Error"] = "Error loading clock history. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClockIn(int userId, int locationId)
        {
            try
            {
                var clockRecord = new ClockRecord
                {
                    UserId = userId,
                    LocationId = locationId,
                    ClockInTime = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                var result = await _clockService.ClockInAsync(clockRecord);

                if (result)
                {
                    TempData["Success"] = "Clocked in successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to clock in. Please try again.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clocking in user {UserId}", userId);
                TempData["Error"] = "Error clocking in. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClockOut(int recordId)
        {
            try
            {
                var result = await _clockService.ClockOutAsync(recordId);

                if (result)
                {
                    TempData["Success"] = "Clocked out successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to clock out. Please try again.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clocking out record {RecordId}", recordId);
                TempData["Error"] = "Error clocking out. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
