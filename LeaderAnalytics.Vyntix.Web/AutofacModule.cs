﻿using Autofac.Core;

namespace LeaderAnalytics.Vyntix.Web;

public class AutofacModule : Autofac.Module
{
    private ServiceRegistrar registrar;

    public AutofacModule(ServiceRegistrar registrar) => this.registrar = registrar ?? throw new ArgumentNullException(nameof(registrar));

    protected override void Load(ContainerBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        IConfiguration config = registrar.config;
        string subscriptionsFilePath = Path.Combine(AppContext.BaseDirectory, $"subscriptions.{registrar.EnvironmentName}.json");
        SubscriptionFilePathParameter subscriptionFilePathParameter = new SubscriptionFilePathParameter() { Value = subscriptionsFilePath };
        ConfigFilePathParameter configFilePathParameter = new ConfigFilePathParameter() { Value = Path.Combine(AppContext.BaseDirectory, $"appsettings.{registrar.EnvironmentName}.json") };
        builder.RegisterInstance(subscriptionFilePathParameter).SingleInstance();
        builder.RegisterInstance(configFilePathParameter).SingleInstance();

        ClientCredentialsHelper helper = new ClientCredentialsHelper(AzureADConfig.ReadFromConfig(config,"AzureAD"));
        builder.RegisterInstance<HttpClient>(helper.AuthorizedClient()).SingleInstance(); // Registers HttpClient that is injected into EMailController - do not use for GraphService
        builder.Register<Func<HttpClient>>(c => () => new HttpClient()).SingleInstance();

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
        builder.RegisterType<LeaderAnalytics.Vyntix.Web.Services.SubscriptionService>().SingleInstance();
        builder.RegisterType<LeaderAnalytics.Vyntix.Web.Services.SubscriptionService>().As<ISubscriptionService>().SingleInstance();
        builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>();
        builder.Register(c =>
        {
            var fp = c.Resolve<ConfigFilePathParameter>();
            return new EMailClient(fp.Value);

        }).SingleInstance();
    }
}
