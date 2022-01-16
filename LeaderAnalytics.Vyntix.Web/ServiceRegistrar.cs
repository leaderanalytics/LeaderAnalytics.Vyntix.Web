namespace LeaderAnalytics.Vyntix.Web;

public class ServiceRegistrar
{
    private bool isBuilt;
    private string _appSettingsFilePath;
    private string _appSettingsFileName;
    protected IConfiguration config;
    public readonly string EnvironmentName;


    public ServiceRegistrar(IConfiguration config, string environmentName)
    {
        this.config = config ?? throw new ArgumentNullException(nameof(config));
        this.EnvironmentName = environmentName ?? throw new ArgumentNullException(nameof(environmentName));
    }

    public string AppSettingsFilePath
    {
        get
        {
            if (!string.IsNullOrEmpty(_appSettingsFilePath))
                return _appSettingsFilePath;

            _appSettingsFilePath = EnvironmentName == "Development" ? config["AuthConfig"] : string.Empty;
            return _appSettingsFilePath;
        }
    }

    public string AppSettingsFileName
    {
        get
        {
            if (!string.IsNullOrEmpty(_appSettingsFileName))
                return _appSettingsFileName;

            _appSettingsFileName = Path.Combine(AppSettingsFilePath, $"appsettings.{EnvironmentName}.json");
            return _appSettingsFileName;
        }
    }

    public IConfiguration BuildConfiguration()
    {
        if (isBuilt)
            return config;

        isBuilt = true;

        config = new ConfigurationBuilder()
            .AddConfiguration(config)
            .AddJsonFile(AppSettingsFileName, false)
            .Build();

        return config;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        BuildConfiguration();
        // Configuration to sign-in users with Azure AD B2C - Not MSAL.  MSAL is used to call APIs.
        services.AddMicrosoftIdentityWebAppAuthentication(config, "AzureADB2C");

        services.AddControllersWithViews()
            .AddMicrosoftIdentityUI();

        // In production, the React files will be served from this directory
        services.AddSpaStaticFiles(configuration =>
        {
            configuration.RootPath = "ClientApp/build";
        });

        //Configuring appsettings section AzureAdB2C, into IOptions
        services.AddOptions();
        services.Configure<OpenIdConnectOptions>(config.GetSection("AzureADB2C"));

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
        services.AddApplicationInsightsTelemetry(config["APPINSIGHTS_INSTRUMENTATIONKEY"]);
    }
}
