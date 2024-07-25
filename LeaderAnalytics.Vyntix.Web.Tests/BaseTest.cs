namespace LeaderAnalytics.Vyntix.Web.Tests;

public class BaseTest
{
    protected GraphClientCredentials GraphClientCredentials { get; private set; }
    protected ContainerBuilder ContainerBuilder { get; set; }
    protected IContainer Container { get; set; }
    protected IConfiguration Configuration { get; set; }
    protected IServiceCollection ServiceCollection { get; set; }
    protected ServiceRegistrar registrar { get; set; }


    public BaseTest()
    {
        ConfigHelper.CopyConfigFromSource(Core.EnvironmentName.development, Program.configFileSourceFolder, AppContext.BaseDirectory);
        ServiceCollection = new ServiceCollection();
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.development.json", false)
            .Build();
        registrar = new ServiceRegistrar(config, "Development");
        registrar.ConfigureServices(ServiceCollection);
    }

    protected GraphClientCredentials GetGraphCredentials()
    {
        IConfigurationSection graphSection = Configuration.GetSection("AzureGraph");
        GraphClientCredentials = new GraphClientCredentials
        {
            ClientID = graphSection.GetValue<string>("ClientID"),
            TenantID = graphSection.GetValue<string>("TenantID"),
            ClientSecret = graphSection.GetValue<string>("ClientSecret")
        };

        return GraphClientCredentials;
    }

    protected virtual void Setup()
    {
        CreateContainer();
        CreateMocks();
        Container = ContainerBuilder.Build();
    }


    protected virtual void CreateMocks()
    {

    }

    protected virtual void CreateContainer()
    {
        ContainerBuilder = new ContainerBuilder();
        ContainerBuilder.RegisterModule(new AutofacModule(registrar));
        
    }

    protected ILifetimeScope GetLifetimeScope()
    {
        return Container.BeginLifetimeScope();
    }
}
