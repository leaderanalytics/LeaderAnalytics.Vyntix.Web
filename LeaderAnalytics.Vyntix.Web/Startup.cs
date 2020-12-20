using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using LeaderAnalytics.Vyntix.Web.Model;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using LeaderAnalytics.Vyntix.Web.Services;
using LeaderAnalytics.Core.Azure;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace LeaderAnalytics.Vyntix.Web
{
    public class Startup
    {
        private IConfiguration configuration { get; }
        private IWebHostEnvironment environment;
        private const string CORS_Origins = "CORS_Origins";

        public Startup(IWebHostEnvironment env, IConfiguration config)
        {
            environment = env;
            configuration = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            new ServiceCollectionCreator().ConfigureServices(services, configuration, environment.EnvironmentName);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseRouting();

            // With endpoint routing, the CORS middleware must be configured to execute between the calls to UseRouting and UseEndpoints.
            // The call to UseCors must be placed after UseRouting, but before UseAuthorization.
            // Middleware order: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-3.1#middleware-order

            app.UseCors(CORS_Origins);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
