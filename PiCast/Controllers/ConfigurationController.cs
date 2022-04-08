using Microsoft.AspNetCore.Mvc;
using PiCast.Model;
using PiCast.Service;

namespace PiCast.Controllers;

[Route("api/[controller]")]
public class ConfigurationController : CrudController<Configuration>
{
    public ConfigurationController(IService<Configuration> service) : base(service)
    {
    }

    [HttpGet("Context")]
    public Task<IEnumerable<Configuration>> GetByContex(string name, string context)
    {
        return Task.FromResult<IEnumerable<Configuration>>(_service.Get().Where(x =>
            (!string.IsNullOrWhiteSpace(name) && x.Name.ToLower().Contains(name.ToLower())) ||
            (!string.IsNullOrWhiteSpace(context) && x.Context.ToLower().Contains(context.ToLower()))));
    }

    [HttpGet("")]
    public override Task<IEnumerable<Configuration>> Get(string filter)
    {
        return Task.FromResult<IEnumerable<Configuration>>(_service.Get().Where(x =>
            (!string.IsNullOrWhiteSpace(filter) && x.Name.ToLower().Contains(filter.ToLower())) ||
            (!string.IsNullOrWhiteSpace(filter) && x.Context.ToLower().Contains(filter.ToLower()))));
    }
}