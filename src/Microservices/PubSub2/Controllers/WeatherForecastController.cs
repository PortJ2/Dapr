using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using PubSub2.DAL;
using System.Text.Json;

namespace PubSub2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        private const string StoreName = "pubsub";
        private const string Subscription1 = "weatherreport";
        private const string Subscription2 = "weatherstatus";

        public WeatherForecastController(ApplicationDbContext context, ILogger<WeatherForecastController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Topic(StoreName, Subscription1)]
        [HttpPost(Name = "GetWeatherForecast")]
        public async Task<ActionResult> Post(object message)
        {
            var messageJson = JsonSerializer.Serialize<object>(message);
            using var _client = new DaprClientBuilder().Build();
            var weatherStatus = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToList();

            foreach (var status in weatherStatus)
                _context.WeatherForecasts.Add(status);

            _context.SaveChanges();

            await _client.PublishEventAsync<List<WeatherForecast>>(StoreName, Subscription2, weatherStatus);

            return Ok(weatherStatus);
        }
    }
}