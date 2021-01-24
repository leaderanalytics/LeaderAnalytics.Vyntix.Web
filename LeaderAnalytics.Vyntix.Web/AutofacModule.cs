using Autofac;
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
using System.Net.Http;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web
{
    public class AutofacModule
    {

        public void ConfigureServices(ContainerBuilder builder, ServiceRegistrar registrar)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (registrar == null)
                throw new ArgumentNullException(nameof(registrar));

            IConfiguration config = registrar.BuildConfiguration();
            string subscriptionsFilePath = Path.Combine(registrar.AppSettingsFilePath, $"subscriptions.{registrar.EnvironmentName}.json");
            SubscriptionFilePathParameter subscriptionFilePathParameter = new SubscriptionFilePathParameter() { Value = subscriptionsFilePath };
            ConfigFilePathParameter configFilePathParameter = new ConfigFilePathParameter() { Value = registrar.AppSettingsFileName };
            builder.RegisterInstance(subscriptionFilePathParameter).SingleInstance();
            builder.RegisterInstance(configFilePathParameter).SingleInstance();

            ClientCredentialsHelper helper = new ClientCredentialsHelper(AzureADConfig.ReadFromConfig(config));
            builder.RegisterInstance<HttpClient>(helper.AuthorizedClient()).SingleInstance(); // Registers HttpClient that is injected into EMailController

            // Stripe
            Stripe.StripeConfiguration.ApiKey = config["StripeApiKey"];
            Stripe.StripeClient stripeClient = new Stripe.StripeClient(Stripe.StripeConfiguration.ApiKey);
            builder.RegisterInstance(stripeClient).SingleInstance();
            builder.RegisterType<Stripe.CustomerService>().SingleInstance();
            builder.RegisterType<Stripe.SubscriptionService>().SingleInstance();
            builder.RegisterType<Stripe.BillingPortal.SessionService>().SingleInstance();

            // Graph Credentials
            IConfigurationSection graphSection = config.GetSection("AzureGraph");
            GraphClientCredentials MSgraphCredentials = new GraphClientCredentials
            {
                ClientID = graphSection.GetValue<string>("ClientID"),
                TenantID = graphSection.GetValue<string>("TenantID"),
                ClientSecret = graphSection.GetValue<string>("ClientSecret")
            };

            builder.RegisterInstance(MSgraphCredentials).Named(Domain.Constants.MS_GRAPH_CREDENTIALS, typeof(GraphClientCredentials));
            

            builder.Register<Func<string, GraphClientCredentials>>(c =>
            {
                IComponentContext cxt = c.Resolve<IComponentContext>();
                return credentialsName => cxt.ResolveNamed<GraphClientCredentials>(credentialsName);
            });
            builder.RegisterType<GraphService>().As<IGraphService>();
            builder.RegisterType<SessionCache>().SingleInstance();
            builder.RegisterType<SubscriptionService>().SingleInstance();
            builder.RegisterType<SubscriptionService>().As<ISubscriptionService>().SingleInstance();
            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>();
            builder.Register(c => {
                var fp = c.Resolve<ConfigFilePathParameter>();
                return new EMailClient(fp.Value);
            
            }).SingleInstance();
        }
    }
}
