using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PiCast.Model;
using PiCast.Service;

namespace PiCast.Controllers;

[Route("api/[controller]")]
public class CurrentWeatherController
{
    public IConfiguration Config;
    private readonly IMemoryCache _cache;
    private static List<string> Cities = new() { "Bacaxa", "Saquarema" };
    private static List<string> Countries = new() { "br", "br" };
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
    public Task<bool> AddCity(string city, string country = "br")
    {
        if (!Cities.Any(x => x.Equals(city, StringComparison.InvariantCultureIgnoreCase)))
        {
            Cities.Add(city);
            Countries.Add(country);
        }

        return Task.FromResult(true);
    }

    [HttpGet("SetLocale/{lang}/{unit}")]
    public Task<string> SetLang(string lang, string unit)
    {
        return Task.FromResult(string.Join("|", Lang = lang, Unit = unit));
    }

    [HttpGet("ListCities")]
    public Task<List<string>> ListCities()
    {
        return Task.FromResult(Cities);
    }

    [HttpGet("RemoveCity")]
    public Task<int> RemoveCity(string city)
    {
        if (!Cities.Any(x => x.Equals(city, StringComparison.InvariantCultureIgnoreCase)))
            return Task.FromResult(0);

        var index = Cities.FindIndex(x => x.Equals(city, StringComparison.InvariantCultureIgnoreCase));
        Cities.RemoveAt(index);
        Countries.RemoveAt(index);
        return Task.FromResult(1);
    }

    [HttpGet("")]
    [HttpPost("")]
    public async Task<WeatherPrediction> GetWeather(string operation = "weather")
    {
        var totalCities = Cities.Count;
        var time = DateTime.Now;

        var index = (int)time.TimeOfDay.TotalMinutes % totalCities;
        var city = Cities[index];
        var country = Countries[index];
        var apiKey = Config["OpenWeatherApiKey"];

        var cacheKey = $"{apiKey}.{operation}.{city}.{country}.{Lang}.{Unit}";

        if (_cache.TryGetValue(cacheKey, out WeatherPrediction value)) 
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

        value = await response.Content.ReadFromJsonAsync<WeatherPrediction>();
        _cache.Set(cacheKey, value, TimeSpan.FromSeconds(100));

        return value;
    }

    [HttpGet("/api/Forecast")]
    [HttpPost("/api/Forecast")]
    public async Task<Forecast> GetForecast(string operation = "forecast")
    {
        var totalCities = Cities.Count;
        var time = DateTime.Now;

        var index = (int)time.TimeOfDay.TotalMinutes % totalCities;
        var city = Cities[index];
        var country = Countries[index];
        var apiKey = Config["OpenWeatherApiKey"];

        var cacheKey = $"{apiKey}.{operation}.{city}.{country}.{Lang}.{Unit}";

        if (_cache.TryGetValue(cacheKey, out Forecast value)) 
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

        value = await response.Content.ReadFromJsonAsync<Forecast>();
        _cache.Set(cacheKey, value, TimeSpan.FromSeconds(100));

        return value;
    }
}