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
        public IConfiguration Config;
        private static List<string> Cities = new List<string>() { "Bacaxa", "Saquarema" };
        private static List<string> Countries = new List<string>() { "br", "br" };
        private static string Lang = "pt";
        private static string Unit = "metric";
        private IService<Configuration> _service;
        public CurrentWeatherController(IService<Configuration> service, IConfiguration config)
        {
            Config = config;
            _service = service;
        }

        [HttpGet("AddCity")]
        public async Task<bool> AddCity(string city, string country = "br")
        {
            if (!Cities.Any(x => x.Equals(city, StringComparison.InvariantCultureIgnoreCase)))
            {
                Cities.Add(city);
                Countries.Add(country);
            }

            return true;
        }

        [HttpGet("SetLocale/{lang}/{unit}")]
        public async Task<string> SetLang(string lang, string unit)
        {
            return string.Join("|", Lang = lang, Unit = unit);
        }

        [HttpGet("ListCities")]
        public Task<List<string>> ListCities()
        {
            return Task.FromResult(Cities);
        }

        [HttpGet("RemoveCity")]
        public async Task<int> RemoveCity(string city)
        {
            if (!Cities.Any(x => x.Equals(city, StringComparison.InvariantCultureIgnoreCase)))
                return 0;

            var index = Cities.FindIndex(x => x.Equals(city, StringComparison.InvariantCultureIgnoreCase));
            Cities.RemoveAt(index);
            Countries.RemoveAt(index);
            return 1;
        }

        [HttpGet("")]
        [HttpPost("")]
        public async Task<IEnumerable<dynamic>> GetCurrentWeather()
        {
            var totalCities = Cities.Count;
            var time = DateTime.Now;

            var index = (int)time.TimeOfDay.TotalMinutes % totalCities;
            var city = Cities[index];
            var country = Countries[index];

            var request =
                $"data/2.5/weather?q={city},{country}&APPID={Config["OpenWeatherApiKey"]}&lang={Lang}&units={Unit}";

            var client = new HttpClient()
            {
                BaseAddress = new Uri("http://api.openweathermap.org")
            };
            var response = await client.GetAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsAsync<dynamic>();
        }

        [HttpGet("/api/Forecast")]
        [HttpPost("/api/Forecast")]
        public async Task<IEnumerable<dynamic>> GetForecast()
        {
            var totalCities = Cities.Count;
            var time = DateTime.Now;

            var index = (int)time.TimeOfDay.TotalMinutes % totalCities;
            var city = Cities[index];
            var country = Countries[index];

            var request =
                $"data/2.5/forecast?q={city},{country}&APPID={Config["OpenWeatherApiKey"]}&lang={Lang}&units={Unit}";

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
