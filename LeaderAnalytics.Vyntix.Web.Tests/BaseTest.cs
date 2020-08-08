using LeaderAnalytics.Vyntix.Web.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LeaderAnalytics.Vyntix.Web.Tests
{
    public class BaseTest
    {
        

        public BaseTest()
        {
        

        }

        protected GraphClientCredentials GetGraphCredentials()
        {
            string configFilePath = "C:\\Users\\sam\\OneDrive\\LeaderAnalytics\\Config\\Vyntix.Web";
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(configFilePath, $"appsettings.Development.json"), false)
                .Build();

            IConfigurationSection graphSection = Configuration.GetSection("AzureGraph");
            GraphClientCredentials graphCredentials = new GraphClientCredentials
            {
                ClientID = graphSection.GetValue<string>("ClientID"),
                TenantID = graphSection.GetValue<string>("TenantID"),
                ClientSecret = graphSection.GetValue<string>("ClientSecret")
            };

            return graphCredentials;
        }

    }
}
