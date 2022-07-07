using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PiCast.Model;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace PiCast.AlertsApp;

public static class WeatherAlert
{
    [FunctionName("WeatherAlert")]
    public static async Task RunAsync([TimerTrigger("10 0 9 * * *", RunOnStartup = false)] TimerInfo myTimer,
        ILogger log)
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        log.LogInformation($"Initializing alerts at {DateTime.UtcNow}");

        var cities = Environment.GetEnvironmentVariable("CITIES").Split(",").GetEnumerator();
        var cityData = new Dictionary<string, EmailTemplateData>();
        while (cities.MoveNext())
        {
            var c = cities.Current!.ToString()!.Trim()!;
            if (cityData.ContainsKey(c))
                continue;
            cityData.Add(c, await PrepareTemplateData(c));
        }

        var sendEmails = bool.Parse(Environment.GetEnvironmentVariable("EMAILS_ENABLED") ?? "false");
        var tasks = new List<Task>();
        var names = Environment.GetEnvironmentVariable("EMAILS_TO_NAME").Split(",").GetEnumerator();
        cities.Reset();
        foreach (var email in Environment.GetEnvironmentVariable("EMAILS_TO").Split(","))
        {
            if (!names.MoveNext() || !cities.MoveNext())
                break;

            var name = names.Current!.ToString()!.Trim()!;
            var city = cities.Current!.ToString()!.Trim()!;

            log.LogInformation($"Sending email [{sendEmails}] to {email.Trim()}, {name}, at {city}");

            if (sendEmails)
                tasks.Add(SendTemplateMail(new EmailAddress(email.Trim(), name), cityData[city]));
        }

        Task.WaitAll(tasks.ToArray(), new CancellationTokenSource(TimeSpan.FromMinutes(2)).Token);
        log.LogInformation($"Process finsihed {DateTime.UtcNow}");
    }

    static async Task SendTemplateMail(EmailAddress to, EmailTemplateData emailTemplateData)
    {
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(Environment.GetEnvironmentVariable("EMAIL_FROM"),
            Environment.GetEnvironmentVariable("EMAIL_FROM_NAME"));
        var templateId = Environment.GetEnvironmentVariable("WEATHER_TEMPLATE_ID");
        var msg = MailHelper.CreateSingleTemplateEmail(from, to, templateId, emailTemplateData);
        var response = await client.SendEmailAsync(msg);
    }

    private static async Task<EmailTemplateData> PrepareTemplateData(string city)
    {
        var weatherDataTask = PrepareWeatherData(city);
        var forecastDataTask = PrepareForecastData(city);
        var forecastList = new List<EmailData>();
        await foreach (var f in forecastDataTask)
            forecastList.Add(f);

        return new EmailTemplateData()
        {
            subject = $"Seu relatório diário do clima para {DateTime.Now:dddd, dd MMMM}",
            current = await weatherDataTask,
            forecast = forecastList
        };
    }

    private static async Task<EmailData> PrepareWeatherData(string city)
    {
        var weather = await PerformOperation<WeatherPrediction>(city, "weather");
        return new EmailData()
        {
            city = weather.name,
            humidity = weather.main.humidity.ToString("00"),
            pressure = weather.main.pressure.ToString(),
            temperature = weather.main.temp.ToString("00"),
            feelsLike = weather.main.feels_like.ToString("00"),
            tempMax = weather.main.temp_max.ToString("00"),
            tempMin = weather.main.temp_min.ToString("00"),
            weatherDescription = weather.weather[0].description,
            weatherIcon = weather.weather[0].icon,
            windDirection = weather.wind.deg.ToString("00"),
            windSpeed = weather.wind.speed.ToString("00.00"),
            windDescription = GetWindDescription(weather.wind.speed),
            stringWindDirection = "", //Todo
            stringDate = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(weather.dt).DateTime,
                TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")).ToString("dd MMMM yyyy"),
            stringDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(weather.dt).DateTime,
                TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")).ToString("G"),
            sunrise = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(weather.sys.sunrise).DateTime,
                TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")).ToString("t"),
            sunset = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(weather.sys.sunset).DateTime,
                TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")).ToString("t"),
            humidityAlertWarning = IsHumidityAlertWarning(weather.main.humidity),
            humidityAlertAlarm = IsHumidityAlertAlarm(weather.main.humidity),
            windAlertWarning = IsWindWarning(weather.wind.speed),
            windAlertAlarm = IsWindAlarm(weather.wind.speed)
        };
    }

    private static bool IsWindAlarm(double windSpeed)
    {
        return windSpeed switch
        {
            > 17.1 => true,
            _ => false
        };
    }

    private static bool IsWindWarning(double windSpeed)
    {
        return windSpeed switch
        {
            <= 7.9 => false,
            <= 17.1 => true,
            _ => false
        };
    }

    private static bool IsHumidityAlertAlarm(int mainHumidity)
    {
        return mainHumidity switch
        {
            < 50 => true,
            _ => false
        };
    }

    private static bool IsHumidityAlertWarning(int mainHumidity)
    {
        return mainHumidity switch
        {
            // > 85 => true, //Todo -> check
            < 50 => false,
            < 60 => true,
            _ => false
        };
    }

    private static string GetWindDescription(double windSpeed)
    {
        return windSpeed switch
        {
            < 0.3 => "Calmaria",
            <= 1.5 => "Bafagem",
            <= 3.3 => "Leve brisa",
            <= 5.4 => "Fraco",
            <= 7.9 => "Moderado",
            <= 10.7 => "Fresco",
            <= 13.8 => "Muito fresco",
            <= 17.1 => "Forte",
            <= 20.7 => "Muito forte",
            <= 24.4 => "Duro",
            <= 28.4 => "Muito duro",
            <= 32.6 => "Tempestuoso",
            _ => "Furacão"
        };
    }

    private static async IAsyncEnumerable<EmailData> PrepareForecastData(string city)
    {
        var forecast = await PerformOperation<Forecast>(city, "forecast");
        var count = 0;
        foreach (var item in forecast.list)
        {
            var stringDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(item.dt).DateTime,
                TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")).ToString("g");
            var currentStringDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")).ToString("g");
            if (stringDateTime == currentStringDateTime)
                continue;

            yield return new EmailData()
            {
                city = forecast.city.name,
                humidity = item.main.humidity.ToString("00"),
                pressure = item.main.pressure.ToString(),
                temperature = item.main.temp.ToString("00"),
                feelsLike = item.main.feels_like.ToString("00"),
                tempMax = item.main.temp_max.ToString("00"),
                tempMin = item.main.temp_min.ToString("00"),
                weatherDescription = item.weather[0].description,
                weatherIcon = item.weather[0].icon,
                windDirection = item.wind.deg.ToString("00"),
                windSpeed = item.wind.speed.ToString("00.00"),
                windDescription = GetWindDescription(item.wind.speed),
                stringWindDirection = "", //Todo
                stringDate = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(item.dt).DateTime,
                    TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")).ToString("dd MMMM yyyy"),
                stringDateTime = stringDateTime,
                humidityAlertWarning = IsHumidityAlertWarning(item.main.humidity),
                humidityAlertAlarm = IsHumidityAlertAlarm(item.main.humidity),
                windAlertWarning = IsWindWarning(item.wind.speed),
                windAlertAlarm = IsWindAlarm(item.wind.speed)
            };
            if (++count >= 6)
                yield break;
        }
    }

    private static async Task<T> PerformOperation<T>(string city, string operation) where T : class
    {
        var country = "br";
        var lang = "pt";
        var unit = "metric";
        var apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY");

        var request =
            $"data/2.5/{operation}?q={city},{country}&APPID={apiKey}&lang={lang}&units={unit}";

        var client = new HttpClient()
        {
            BaseAddress = new Uri(Environment.GetEnvironmentVariable("OPENWEATHER_API_URL"))
        };
        var response = await client.GetAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<T>();
    }
}