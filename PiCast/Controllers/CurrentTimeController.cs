using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace PiCast.Controllers;

[Route("api/[controller]")]
public class CurrentTimeController
{
    public CurrentTimeController()
    {
    }
    [HttpGet("")]
    [HttpPost("")]
    public Task<dynamic> GetCurrentTime()
    {
        return Task.FromResult<dynamic>(new
        {
            Date = DateTime.Today.ToString("d"),
            Time = DateTime.Now.ToString("HH:mm"),
            FullTime = DateTime.Now.ToString("HH:mm:ss"),
            ReadableDate = DateTime.Now.ToString("dd MMM", CultureInfo.GetCultureInfo("pt-BR")),
        });
    }

}