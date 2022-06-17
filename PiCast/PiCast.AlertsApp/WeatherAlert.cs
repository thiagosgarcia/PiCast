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
    public static async Task RunAsync([TimerTrigger("0 0 9 * * *", RunOnStartup = false)] TimerInfo myTimer,
        ILogger log)
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        log.LogInformation($"Initializing alerts at {DateTime.UtcNow}");
        
        var cities = Environment.GetEnvironmentVariable("CITIES").Split(",").GetEnumerator();
        var cityData = new Dictionary<string, IAsyncEnumerable<string>>();
        while(cities.MoveNext())
        {
            var c = cities.Current!.ToString()!.Trim()!;
            if (cityData.ContainsKey(c))
                continue;
            cityData.Add(c, PrepareData(log, c));
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
            
            var emailBody = string.Empty;
            await foreach (var item in cityData[city])
            {
                log.LogInformation(item);
                emailBody += $"{item}\n<br>";
            }

            log.LogInformation($"Sending email [{sendEmails}] to {email.Trim()}, {name}, at {city}");

            if (sendEmails)
                tasks.Add(SendMail(new EmailAddress(email.Trim(), name), emailBody));
        }

        Task.WaitAll(tasks.ToArray(), new CancellationTokenSource(TimeSpan.FromMinutes(2)).Token);
        log.LogInformation($"Process finsihed {DateTime.UtcNow}");
    }

    static async Task SendMail(EmailAddress to, string body)
    {
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(Environment.GetEnvironmentVariable("EMAIL_FROM"),
            Environment.GetEnvironmentVariable("EMAIL_FROM_NAME"));
        var subject = $"Seu relatório diário do clima para {DateTime.Now:dddd, dd MMMM}";
        var plainTextContent = "Aqui você receberá os alertas do clima diariamente";
        var htmlContent = $"<p>Aqui você receberá os alertas do clima <strong>diariamente</strong></p><br>" +
                          $"<p>{body}</p>";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);
    }

    private static async IAsyncEnumerable<string> PrepareData(ILogger log, string city)
    {
        var weatherTask = PerformOperation<WeatherPrediction>(city, "weather");
        var forecastTask = PerformOperation<Forecast>(city, "forecast");
        var count = 0;

        var weather = await weatherTask;
        yield return
            $"Agora em {weather.name} \t- {weather.main.temp:00}ºC - {weather.main.humidity}% - {weather.wind.speed:00.00}km/h - ~{weather.main.feels_like:00}ºC - {weather.weather[0].description}";
        var forecast = await forecastTask;
        foreach (var f in forecast.list)
        {
            yield return
                $"{TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(f.dt).DateTime, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")):G}" +
                $" \t- {f.main.temp:00}ºC - {f.main.humidity}% - {f.wind.speed:00.00}km/h - ~{f.main.feels_like:00}ºC - {f.weather[0].description}";
            if (++count >= 6)
                yield break;
        }
    }

    private static async Task<T> PerformOperation<T>(string city, string operation) where T :class
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