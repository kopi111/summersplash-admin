using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SummerSplashWeb.Services;

namespace SummerSplashWeb.Controllers
{
    [Authorize]
    public class TrainingController : Controller
    {
        private readonly ITrainingService _trainingService;
        private readonly ILogger<TrainingController> _logger;

        public TrainingController(ITrainingService trainingService, ILogger<TrainingController> logger)
        {
            _trainingService = trainingService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var trainings = await _trainingService.GetAllTrainingsAsync();
                return View(trainings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading training records");
                ViewBag.Error = "Error loading training data. Please try again.";
                return View(new List<SummerSplashWeb.Models.Training>());
            }
        }
    }
}
