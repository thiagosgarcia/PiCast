using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PiCast.Model;
using PiCast.Repository;
using PiCast.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<EntityContext>();
builder.Services.AddSingleton<IConfiguration>(
    new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build());

builder.Services.AddScoped<IService<Configuration>, Service<Configuration>>();
builder.Services.AddScoped<IRepository<Configuration>, Repository<Configuration>>();

builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
builder.Services.AddScoped<EntityContext>();

SQLitePCL.Batteries.Init();

using (var db = new EntityContext())
    db.Database.Migrate();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(x =>
{
    x.AllowAnyOrigin();
    x.AllowAnyHeader();
    x.AllowAnyMethod();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");
;

app.Run();