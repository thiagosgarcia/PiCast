using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PiCast.Model;
using PiCast.Service;

namespace PiCast.Controllers
{
    [Route("api/[controller]")]
    public class ConfigurationController : CrudController<Configuration>
    {
        public ConfigurationController(IService<Configuration> service) : base(service)
        {
        }

        [HttpGet("Context")]
        public async Task<IEnumerable<Configuration>> GetByContex(string name, string context)
        {
            return _service.Get().Where(x =>
                (!string.IsNullOrWhiteSpace(name) && x.Name.ToLower().Contains(name.ToLower())) ||
                (!string.IsNullOrWhiteSpace(context) && x.Context.ToLower().Contains(context.ToLower())));
        }

        [HttpGet("")]
        public override async Task<IEnumerable<Configuration>> Get(string filter)
        {
            return _service.Get().Where(x =>
                (!string.IsNullOrWhiteSpace(filter) && x.Name.ToLower().Contains(filter.ToLower())) ||
                (!string.IsNullOrWhiteSpace(filter) && x.Context.ToLower().Contains(filter.ToLower())));
        }
    }
}
