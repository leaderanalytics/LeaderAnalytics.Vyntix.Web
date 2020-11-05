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
        public IConfiguration Configuration { get; }
        private string subscriptionFilePath;
        private const string CORS_Origins = "CORS_Origins";

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            string configFilePath = string.Empty;

            if (env.EnvironmentName == "Development")
                configFilePath = "C:\\Users\\sam\\OneDrive\\LeaderAnalytics\\Config\\Vyntix.Web";

            Configuration = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .AddJsonFile(Path.Combine(configFilePath, $"appsettings.{env.EnvironmentName}.json"), false)
                .Build();
            
            subscriptionFilePath = Path.Combine(configFilePath, $"subscriptions.{env.EnvironmentName}.json");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AzureADConfig config = AzureADConfig.ReadFromConfig(Configuration);
            services.AddSingleton(config);
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Configuration to sign-in users with Azure AD B2C - Not MSAL.  MSAL is used to call APIs.

            services.AddMicrosoftIdentityWebAppAuthentication(Configuration, "AzureADB2C");
            services.AddControllersWithViews()
                .AddMicrosoftIdentityUI();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            //Configuring appsettings section AzureAdB2C, into IOptions
            services.AddOptions();
            services.Configure<OpenIdConnectOptions>(Configuration.GetSection("AzureADB2C"));

            services.AddCors(options =>
            {
                options.AddPolicy(name: CORS_Origins,builder =>
                {
                    builder
                    .WithOrigins(new string[]
                    {
                        "http://www.vyntix.com",
                        "https://www.vyntix.com",
                        "http://vyntix.com",
                        "https://vyntix.com",
                        "http://localhost",
                        "http://dev.vyntix.com",
                        "http://vyntix.azurewebsites.net",
                        "https://vyntix.azurewebsites.net",
                        "http://localhost:5032",
                        "https://localhost:5031",
                        "https://vyntix-staging.azurewebsites.net",
                        "https://billing.stripe.com",
                        "http://billing.stripe.com"
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });


            // Stripe
            Stripe.StripeConfiguration.ApiKey = Configuration["StripeApiKey"];
            Stripe.StripeClient stripeClient = new Stripe.StripeClient(Stripe.StripeConfiguration.ApiKey);
            services.AddSingleton(stripeClient);

            // Graph Credentials
            IConfigurationSection graphSection = Configuration.GetSection("AzureGraph");
            GraphClientCredentials graphCredentials = new GraphClientCredentials
            {
                ClientID = graphSection.GetValue<string>("ClientID"),
                TenantID = graphSection.GetValue<string>("TenantID"),
                ClientSecret = graphSection.GetValue<string>("ClientSecret")
            };
            GraphService graphService = new GraphService(graphCredentials);
            SessionCache sessionCache = new SessionCache();
            SubscriptionService subscriptionService = new Services.SubscriptionService(graphService, stripeClient, sessionCache, subscriptionFilePath);

            services.AddSingleton(graphCredentials);
            services.AddSingleton(graphService);
            services.AddSingleton(subscriptionService);
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
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
