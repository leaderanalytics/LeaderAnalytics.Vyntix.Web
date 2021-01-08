using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

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
                logRoot = "..\\..\\serilog\\Vyntix.Web\\log";   // Create logs in D:\home\serilog

            // Note UseSerilog() in CreateHostBuilder below.
            Log.Logger = new LoggerConfiguration()
               .WriteTo.File(logRoot, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
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
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
