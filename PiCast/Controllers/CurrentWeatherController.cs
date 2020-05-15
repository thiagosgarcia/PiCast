using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using PiCast.Model;
using PiCast.Service;

namespace PiCast.Controllers
{
    [Route("api/[controller]")]
    public class CurrentWeatherController
    {
        private static List<string> Cities = new List<string>() { "Curitiba", "Saquarema" };
        private IService<Configuration> _service;
        private string ApiKey = "2454dee9bd72c126f0a19acd04993d3b";
        public CurrentWeatherController(IService<Configuration> service)
        {
            _service = service;
        }

        [HttpGet("AddCity")]
        public async Task<bool> AddCity(string city)
        {
            if (!Cities.Any(x => x.Equals(city, StringComparison.InvariantCultureIgnoreCase)))
                Cities.Add(city);

            return true;
        }

        [HttpGet("ListCities")]
        public Task<List<string>> ListCities(string city)
        {
            return Task.FromResult(Cities);
        }

        [HttpGet("RemoveCity")]
        public async Task<int> RemoveCity(string city)
        {
            if (!Cities.Any())
                return 0;
            if (Cities.Any(x => x.Equals(city, StringComparison.InvariantCultureIgnoreCase)))
            {
                Cities.RemoveAt(Cities.FindIndex(x => x.Equals(city, StringComparison.InvariantCultureIgnoreCase)));
                return 1;
            }
            return 0;
        }

        [HttpGet("")]
        [HttpPost("")]
        public async Task<IEnumerable<dynamic>> GetCurrentWeather()
        {
            var totalCities = Cities.Count;
            var time = DateTime.Now;

            var index = (int)time.TimeOfDay.TotalMinutes % totalCities;
            var city = Cities[index];
            //var city = /*_service.Get().SingleOrDefault(x => x.Name == "CurrentCity")?.Value ??*/ "Curitiba";

            var country = /*_service.Get().SingleOrDefault(x => x.Name == "CurrentCountry")?.Value ??*/ "br";

            var request =
                $"data/2.5/weather?q={city},{country}&APPID={ApiKey}&lang=pt&units=metric";

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
