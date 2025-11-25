using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SummerSplashWeb.Services;
using SummerSplashWeb.Models;

namespace SummerSplashWeb.Controllers
{
    [Authorize]
    public class LocationsController : Controller
    {
        private readonly ILocationService _locationService;
        private readonly ILogger<LocationsController> _logger;

        public LocationsController(ILocationService locationService, ILogger<LocationsController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> SearchAddresses(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                    return Json(new { success = true, addresses = Array.Empty<object>() });

                var locations = await _locationService.GetAllLocationsAsync();
                var matches = locations
                    .Where(l => !string.IsNullOrEmpty(l.Address) &&
                               (l.Address.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                (l.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                (l.City?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)))
                    .Take(10)
                    .Select(l => new
                    {
                        locationId = l.LocationId,
                        name = l.Name,
                        address = l.Address,
                        city = l.City,
                        state = l.State,
                        zipCode = l.ZipCode,
                        latitude = l.Latitude,
                        longitude = l.Longitude,
                        fullAddress = $"{l.Address}, {l.City}, {l.State} {l.ZipCode}".Trim(' ', ',')
                    })
                    .ToList();

                return Json(new { success = true, addresses = matches });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching addresses");
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var locations = await _locationService.GetAllLocationsAsync();
                return View(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading locations");
                ViewBag.Error = "Error loading locations. Please try again.";
                return View(new List<JobLocation>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var location = await _locationService.GetLocationByIdAsync(id);
                if (location == null)
                    return NotFound();

                return View(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading location details");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobLocation location)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _locationService.CreateLocationAsync(location);
                    TempData["Success"] = "Location created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                return View(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating location");
                ModelState.AddModelError("", "Error creating location. Please try again.");
                return View(location);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var location = await _locationService.GetLocationByIdAsync(id);
                if (location == null)
                    return NotFound();

                return View(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading location for edit");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JobLocation location)
        {
            try
            {
                if (id != location.LocationId)
                    return BadRequest();

                if (ModelState.IsValid)
                {
                    await _locationService.UpdateLocationAsync(location);
                    TempData["Success"] = "Location updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                return View(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location");
                ModelState.AddModelError("", "Error updating location. Please try again.");
                return View(location);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _locationService.DeleteLocationAsync(id);
                TempData["Success"] = "Location deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting location");
                TempData["Error"] = "Error deleting location. It may be in use.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
