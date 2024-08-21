using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;

namespace FeatureFlagging.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IFeatureManager _featureManager;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IFeatureManager featureManager)
        {
            _logger = logger;
            _featureManager = featureManager;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
            })
            .ToArray();

            if (await _featureManager.IsEnabledAsync(nameof(Feature.WeatherContainsSummary)))
            {
                foreach (var f in forecast)
                {
                    f.Summary = Summaries[Random.Shared.Next(Summaries.Length)];
                }
            }

            return forecast;
        }

        [HttpGet("Features")]
        public async Task<ActionResult> List()
        {
            var features = new Dictionary<string, bool>();
            await foreach (var f in _featureManager.GetFeatureNamesAsync())
            {
                features.Add(f, await _featureManager.IsEnabledAsync(f).ConfigureAwait(false));
            }

            return Ok(features);
        }
    }
}
