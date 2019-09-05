using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Configuration;
using PiCast.Model;
using PiCast.Service;

namespace PiCast.Controllers
{
    [Route("api/[controller]")]
    public class CurrentWeatherController
    {
        private IService<Configuration> _service;
        private readonly IConfiguration _configuration;
        public CurrentWeatherController(IService<Configuration> service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }
        [HttpGet("")]
        [HttpPost("")]
        public async Task<IEnumerable<dynamic>> GetCurrentWeather()
        {
            var city = /*_service.Get().SingleOrDefault(x => x.Name == "CurrentCity")?.Value ??*/ "Curitiba";
            var country = /*_service.Get().SingleOrDefault(x => x.Name == "CurrentCountry")?.Value ??*/ "br";

            var request =
                $"data/2.5/weather?q={city},{country}&APPID={_configuration["OpenWeatherApiKey"]}&lang=pt&units=metric";

            var client = new HttpClient()
            {
                BaseAddress = new Uri("http://api.openweathermap.org")
            };
            var response = await client.GetAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsAsync<dynamic>();
        }

    }
}
