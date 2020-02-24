using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardGame.Server.CommonState;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CardGame.Server.Controllers
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
        private readonly ICommonStateModelFactory _commonStateModelFactory;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            ICommonStateModelFactory commonStateModelFactory)
        {
            _logger = logger;
            _commonStateModelFactory = commonStateModelFactory;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var state = _commonStateModelFactory.Create("some id");
            try
            {
                await state.ChangeSomething();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
