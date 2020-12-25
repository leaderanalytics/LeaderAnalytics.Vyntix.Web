using LeaderAnalytics.Core;
using LeaderAnalytics.Core.Azure;
using LeaderAnalytics.Vyntix.Web.Domain;
using LeaderAnalytics.Vyntix.Web.Model;
using LeaderAnalytics.Vyntix.Web.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web
{
    public class ServiceCollectionCreator
    {

        public void ConfigureServices(IServiceCollection services, IConfiguration config, string environmentName)
        {
            string configFilePath = string.Empty;

            if (environmentName == "Development")
                configFilePath = config["AuthConfig"];

            configFilePath = Path.Combine(configFilePath, $"appsettings.{environmentName}.json");
            SubscriptionFilePathParameter subscriptionFilePathParameter = new SubscriptionFilePathParameter() { Value = Path.Combine(configFilePath, $"subscriptions.{environmentName}.json") };
            ConfigFilePathParameter configFilePathParameter = new ConfigFilePathParameter() { Value = configFilePath };
            services.AddSingleton(subscriptionFilePathParameter);
            services.AddSingleton(configFilePathParameter);

            IConfiguration Configuration = new ConfigurationBuilder()
           .AddConfiguration(config)
           .AddJsonFile(configFilePath, false)
           .Build();

            AzureADConfig azureConfig = AzureADConfig.ReadFromConfig(Configuration);
            services.AddSingleton(azureConfig);
            IActionContextAccessor accessor = new ActionContextAccessor();
            services.AddSingleton<IActionContextAccessor>(accessor);

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
                options.AddPolicy(name: "CORS_Origins", builder =>
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
            IGraphService graphService = new GraphService(graphCredentials);

            //SessionCache sessionCache = new SessionCache();
            //SubscriptionService subscriptionService = new Services.SubscriptionService(azureConfig, accessor, graphService, stripeClient, sessionCache, subscriptionFilePath);
            //services.AddSingleton(subscriptionService);

            services.AddSingleton(graphCredentials);
            services.AddSingleton<IGraphService>(graphService);
            services.AddSingleton(typeof(SessionCache));
            services.AddSingleton(typeof(SubscriptionService));
            services.AddSingleton<EMailClient>(x => new EMailClient(x.GetService<ConfigFilePathParameter>().Value));
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
        }
    }
}
