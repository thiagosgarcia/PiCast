using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PiCast.Model;
using PiCast.Repository;
using PiCast.Service;
using Swashbuckle.AspNetCore.Swagger;

namespace WebApplication2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddScoped<IService<Configuration>, Service<Configuration>>();
            services.AddScoped<IRepository<Configuration>, Repository<Configuration>>();

            services.AddScoped<EntityContext>();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
        {
            configuration.RootPath = "ClientApp/dist";
        });

            services.AddScoped<EntityContext>();

            SQLitePCL.Batteries.Init();

            using (var db = new EntityContext())
                db.Database.Migrate();

            services.ConfigureSwaggerGen(x =>
            {
                x.IncludeXmlComments("apiDocs.xml");
            });

            services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(x => x.FullName);
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "Swagger docs for PiCast",
                    Version = "vAny",
                    Contact = new Contact()
                    {
                        Name = "Thiago Garcia"
                    }
                });

            });
            services.AddCors(builder =>
            {
                builder.AddPolicy("AllowAllPolicy",
                    options => options.AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowAnyOrigin()
                        .AllowCredentials());
            });

            services.AddMvc()
                .AddJsonOptions(x =>
                {
                    x.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
