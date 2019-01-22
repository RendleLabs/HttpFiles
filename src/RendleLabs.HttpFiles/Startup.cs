using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RendleLabs.HttpFiles.Options;
using RendleLabs.HttpFiles.Services;

namespace RendleLabs.HttpFiles
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
            services.Configure<StorageOptions>(Configuration.GetSection("Storage"));
            services.Configure<SecurityOptions>(Configuration.GetSection("Security"));
            services.AddSingleton<IDirectories, Directories>();
            services.AddSingleton<IFiles, Files>();
            services.AddSingleton<IHmacVerification, HmacVerification>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<AuthMiddleware>();

            app.UseRouter(builder =>
            {
                builder.MapPut("/put/{**file}", async (request, response, data) =>
                {
                    var file = data.GetString("file");
                });
            });

            app.UseMvc();
        }
    }

    public static class RouteDataExtensions
    {
        public static string GetString(this RouteData data, string key)
        {
            if (data.Values.TryGetValue(key, out var obj) && obj is string str)
            {
                return str;
            }

            return null;
        }
    }
}
