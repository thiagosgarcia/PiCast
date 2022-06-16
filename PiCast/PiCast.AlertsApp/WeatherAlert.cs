using System;
using System.Collections.Generic;
using System.Globalization;
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
        var data = PrepareData(log);
        var emailBody = string.Empty;
        await foreach (var item in data)
        {
            log.LogInformation(item);
            emailBody += $"{item}\n<br>";
        }

        var sendEmails = bool.Parse(Environment.GetEnvironmentVariable("EMAILS_ENABLED") ?? "false");
        var tasks = new List<Task>();
        var names = Environment.GetEnvironmentVariable("EMAILS_TO_NAME").Split(",").GetEnumerator();
        foreach (var email in Environment.GetEnvironmentVariable("EMAILS_TO").Split(","))
        {
            if (!names.MoveNext())
                break;

            var name = names.Current!.ToString();
            log.LogInformation($"Sending email [{sendEmails}] to {email.Trim()}, {name.Trim()}");

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

    private static async IAsyncEnumerable<string> PrepareData(ILogger log)
    {
        var weatherTask = GetWeather();
        var forecastTask = GetForecast();
        var count = 0;

        var weather = await weatherTask;
        yield return
            $"Agora em {weather.name} \t- {weather.weather[0].description} - {weather.main.temp:00}ºC - {weather.main.humidity}% - {weather.wind.speed:00.00}km/h - ~{weather.main.feels_like:00}ºC";
        var forecast = await forecastTask;
        foreach (var f in forecast.list)
        {
            yield return
                $"{TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(f.dt).DateTime, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")):G}" +
                $" \t- {f.weather[0].description} - {f.main.temp:00}ºC - {f.main.humidity}% - {f.wind.speed:00.00}km/h - ~{f.main.feels_like:00}ºC";
            if (++count >= 6)
                break;
        }
    }

    static async Task<WeatherPrediction> GetWeather(string operation = "weather")
    {
        var city = "Saquarema";
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

        return await response.Content.ReadFromJsonAsync<WeatherPrediction>();
    }

    static async Task<Forecast> GetForecast(string operation = "forecast")
    {
        var city = "Saquarema";
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

        return await response.Content.ReadFromJsonAsync<Forecast>();
    }
}