using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SummerSplashWeb.Models;
using SummerSplashWeb.Services;

namespace SummerSplashWeb.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ILocationService _locationService;
        private readonly IUserService _userService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IReportService reportService,
            ILocationService locationService,
            IUserService userService,
            ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _locationService = locationService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null, int? locationId = null, int? techId = null)
        {
            try
            {
                // Default to last 30 days if no date range specified
                if (!startDate.HasValue)
                {
                    startDate = DateTime.Today.AddDays(-30);
                }
                if (!endDate.HasValue)
                {
                    endDate = DateTime.Today;
                }

                var reports = await _reportService.GetReportsByDateRangeAsync(startDate.Value, endDate.Value);

                // Apply filters
                if (locationId.HasValue)
                {
                    reports = reports.Where(r => r.LocationId == locationId.Value).ToList();
                }

                if (techId.HasValue)
                {
                    reports = reports.Where(r => r.TechId == techId.Value).ToList();
                }

                var locations = await _locationService.GetAllLocationsAsync();
                var techs = await _userService.GetUsersByPositionAsync("Service Tech");

                ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
                ViewBag.LocationId = locationId;
                ViewBag.TechId = techId;
                ViewBag.Locations = locations;
                ViewBag.Techs = techs;

                return View(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports");
                ViewBag.Error = "Error loading reports. Please try again.";
                return View(new List<ServiceTechReport>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var report = await _reportService.GetReportByIdAsync(id);
                if (report == null)
                {
                    return NotFound();
                }

                // Get chemical readings for this report
                var chemicalReadings = await _reportService.GetChemicalReadingsForReportAsync(id);
                ViewBag.ChemicalReadings = chemicalReadings;

                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading report details for ID {ReportId}", id);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? userId = null, int? locationId = null, int? clockRecordId = null)
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var locations = await _locationService.GetAllLocationsAsync();

                ViewBag.Users = users;
                ViewBag.Locations = locations;
                ViewBag.PreSelectedUserId = userId;
                ViewBag.PreSelectedLocationId = locationId;
                ViewBag.PreSelectedClockRecordId = clockRecordId;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create report form");
                TempData["Error"] = "Error loading form. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceTechReport report, List<ChemicalReading>? chemicalReadings = null)
        {
            try
            {
                report.ServiceDate = DateTime.Now;
                report.CreatedAt = DateTime.Now;

                var reportId = await _reportService.CreateServiceTechReportAsync(report);

                // Add chemical readings if provided
                if (chemicalReadings != null && chemicalReadings.Any())
                {
                    foreach (var reading in chemicalReadings)
                    {
                        reading.ReportId = reportId;
                        reading.CreatedAt = DateTime.Now;
                        await _reportService.AddChemicalReadingAsync(reading);
                    }
                }

                TempData["Success"] = "Service report created successfully!";
                return RedirectToAction("Details", new { id = reportId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service report");
                TempData["Error"] = "Error creating report. Please try again.";
                return RedirectToAction("Create");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var report = await _reportService.GetReportByIdAsync(id);
                if (report == null)
                {
                    return NotFound();
                }

                var users = await _userService.GetAllUsersAsync();
                var locations = await _locationService.GetAllLocationsAsync();
                var chemicalReadings = await _reportService.GetChemicalReadingsForReportAsync(id);

                ViewBag.Users = users;
                ViewBag.Locations = locations;
                ViewBag.ChemicalReadings = chemicalReadings;

                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit report form for ID {ReportId}", id);
                TempData["Error"] = "Error loading report. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceTechReport report)
        {
            try
            {
                report.UpdatedAt = DateTime.Now;
                var result = await _reportService.UpdateServiceTechReportAsync(report);

                if (result)
                {
                    TempData["Success"] = "Service report updated successfully!";
                    return RedirectToAction("Details", new { id = report.ReportId });
                }
                else
                {
                    TempData["Error"] = "Failed to update report.";
                    return RedirectToAction("Edit", new { id = report.ReportId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service report ID {ReportId}", report.ReportId);
                TempData["Error"] = "Error updating report. Please try again.";
                return RedirectToAction("Edit", new { id = report.ReportId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _reportService.DeleteServiceTechReportAsync(id);

                if (result)
                {
                    TempData["Success"] = "Service report deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete report.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service report ID {ReportId}", id);
                TempData["Error"] = "Error deleting report. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChemicalReading(int reportId, ChemicalReading reading)
        {
            try
            {
                reading.ReportId = reportId;
                reading.CreatedAt = DateTime.Now;

                await _reportService.AddChemicalReadingAsync(reading);

                TempData["Success"] = "Chemical reading added successfully!";
                return RedirectToAction("Details", new { id = reportId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding chemical reading to report {ReportId}", reportId);
                TempData["Error"] = "Error adding chemical reading. Please try again.";
                return RedirectToAction("Details", new { id = reportId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoto(int reportId, string photoUrl, string? description = null)
        {
            try
            {
                var photo = new Photo
                {
                    ReportId = reportId,
                    PhotoUrl = photoUrl,
                    PhotoTimestamp = DateTime.Now,
                    Description = description,
                    CreatedAt = DateTime.Now
                };

                await _reportService.AddPhotoAsync(photo);

                TempData["Success"] = "Photo added successfully!";
                return RedirectToAction("Details", new { id = reportId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding photo to report {ReportId}", reportId);
                TempData["Error"] = "Error adding photo. Please try again.";
                return RedirectToAction("Details", new { id = reportId });
            }
        }
    }
}
