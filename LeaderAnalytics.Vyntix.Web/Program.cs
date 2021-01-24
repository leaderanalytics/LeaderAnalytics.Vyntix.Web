using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Autofac.Extensions.DependencyInjection;

namespace LeaderAnalytics.Vyntix.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string logRoot = null;

            if (env == "Development")
                logRoot = "c:\\serilog\\Vyntix.Web\\log";
            else
                logRoot = "..\\..\\LogFiles\\Vyntix.Web\\log";   // Create logs in D:\home\LogFiles

            // https://docs.microsoft.com/en-us/answers/questions/224685/app-service-log-files-are-not-being-flushed-to-dis.html

            // Note UseSerilog() in CreateHostBuilder below.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(logRoot, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, buffered:true)
                .CreateLogger();
            
            try
            {
                Log.Information("Vyntix.Web - Program.Main started.");
                Log.Information("Environment is: {env}", env);
                CreateHostBuilder(args).Build().Run();
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            }).UseServiceProviderFactory(new AutofacServiceProviderFactory());
    }
}
