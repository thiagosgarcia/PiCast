using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using PiCast.Model;
using PiCast.Service;

namespace PiCast.Controllers
{
    [Route("api/[controller]")]
    public class CurrentWeatherController
    {
        public IConfiguration Config;
        private readonly IMemoryCache _cache;
        private static List<string> Cities = new List<string>() { "Bacaxa", "Saquarema" };
        private static List<string> Countries = new List<string>() { "br", "br" };
        private static string Lang = "pt";
        private static string Unit = "metric";
        private IService<Configuration> _service;
        public CurrentWeatherController(IService<Configuration> service, IConfiguration config,
            IMemoryCache cache)
        {
            Config = config;
            _cache = cache;
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
        public async Task<IEnumerable<dynamic>> GetWeather(string operation = "weather")
        {
            var totalCities = Cities.Count;
            var time = DateTime.Now;

            var index = (int)time.TimeOfDay.TotalMinutes % totalCities;
            var city = Cities[index];
            var country = Countries[index];
            var apiKey = Config["OpenWeatherApiKey"];

            var cacheKey = $"{apiKey}.{operation}.{city}.{country}.{Lang}.{Unit}";

            if (_cache.TryGetValue(cacheKey, out dynamic value)) 
                return value;

            var request =
                $"data/2.5/{operation}?q={city},{country}&APPID={apiKey}&lang={Lang}&units={Unit}";

            var client = new HttpClient()
            {
                BaseAddress = new Uri("http://api.openweathermap.org")
            };
            var response = await client.GetAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            value = await response.Content.ReadAsAsync<dynamic>();
            _cache.Set(cacheKey, (object)value, TimeSpan.FromSeconds(100));

            return value;
        }

        [HttpGet("/api/Forecast")]
        [HttpPost("/api/Forecast")]
        public Task<IEnumerable<dynamic>> GetForecast()
        {
            return GetWeather("forecast");
        }
    }
}
