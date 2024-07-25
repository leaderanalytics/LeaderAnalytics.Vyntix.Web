namespace LeaderAnalytics.Vyntix.Web;

public class Program
{
    public static readonly string configFileSourceFolder = "C:\\Users\\sam\\OneDrive\\LeaderAnalytics\\Config\\Vyntix.Web";

    public static async Task Main(string[] args)
    {
        LeaderAnalytics.Core.EnvironmentName environmentName = LeaderAnalytics.Core.RuntimeEnvironment.GetEnvironmentName();
        string logRoot = "logs/log.txt";
        string configFilePath = AppContext.BaseDirectory;

        // This is just a bare minium logger.  Logger is configured again from config in startup.
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(logRoot, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, buffered: true)
            .CreateLogger();

        try
        {
            ConfigHelper.CopyConfigFromSource(environmentName, configFileSourceFolder, configFilePath);
            ConfigHelper.CopySubscriptionsFromSource(environmentName, configFileSourceFolder, configFilePath);
            IConfigurationRoot config = await ConfigHelper.BuildConfig(environmentName, string.Empty);
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(config).CreateLogger();

            Log.Information("Environment is: {env}", environmentName);

            ServiceRegistrar registrar = new ServiceRegistrar(config, environmentName.ToString());
            WebApplicationBuilder builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());            // Host
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterModule(new AutofacModule(registrar));
            });
            builder.Services.AddLogging(x => x.AddConsole().AddSerilog());
            registrar.ConfigureServices(builder.Services);
            Microsoft.AspNetCore.Builder.WebApplication app = builder.Build();

            if (environmentName == Core.EnvironmentName.development)
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            // Don't need this since files in StatHTML are stored as embedded resources in the assembly.  Saving this this for reference.
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "StaticHTML")),
            //    RequestPath = "/StaticHTML"
            //});
            app.UseSpaStaticFiles();
            app.UseRouting();

            // With endpoint routing, the CORS middleware must be configured to execute between the calls to UseRouting and UseEndpoints.
            // The call to UseCors must be placed after UseRouting, but before UseAuthorization.
            // Middleware order: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-7.0

            app.UseCors(x => x.WithOrigins(new string[] {
                "http://www.vyntix.com",
                "https://www.vyntix.com",
                "http://vyntix.com",
                "https://vyntix.com",
                "http://localhost",
                "https://localhost",
                "http://localhost:5031",
                "https://localhost:5031",
                "http://dev.vyntix.com",
                "http://vyntixweb.azurewebsites.net",
                "https://vyntixweb.azurewebsites.net",
                "https://vyntixweb-staging.azurewebsites.net"
            }).AllowAnyMethod().AllowAnyHeader());

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

                if (environmentName == Core.EnvironmentName.development)
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

            Log.Information("Vyntix.Web - Program.Main started.");

            await app.RunAsync();
            
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }
        finally
        {
            Log.CloseAndFlush();
            System.Threading.Thread.Sleep(2000);
        }
    }

    
}
