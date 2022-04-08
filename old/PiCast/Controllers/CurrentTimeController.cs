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
    public class CurrentTimeController
    {
        public CurrentTimeController()
        {
        }
        [HttpGet("")]
        [HttpPost("")]
        public async Task<dynamic> GetCurrentTime()
        {
            return new
            {
                Date = DateTime.Today.ToString("d"),
                Time = DateTime.Now.ToString("HH:mm"),
                FullTime = DateTime.Now.ToString("HH:mm:ss"),
                ReadableDate = DateTime.Now.ToString("dd MMM"),
            };
        }

    }
}
