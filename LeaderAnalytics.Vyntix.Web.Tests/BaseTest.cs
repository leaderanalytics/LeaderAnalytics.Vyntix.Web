using LeaderAnalytics.Vyntix.Web.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LeaderAnalytics.Vyntix.Web.Tests
{
    public class BaseTest
    {
        protected GraphClientCredentials GraphClientCredentials { get; private set; }
        protected ContainerBuilder ContainerBuilder { get; set; }
        protected IContainer Container { get; set; }
        
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
            ContainerBuilder = new ContainerBuilder();
            CreateContainer();
            CreateMocks();
            Container = ContainerBuilder.Build();
        }


        protected virtual void CreateMocks()
        {
            
        }

        protected virtual void CreateContainer()
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json").Build();
            new AutofacModule().ConfigureServices(ContainerBuilder, config, "Development");
        }

        protected ILifetimeScope GetLifetimeScope()
        {
            return Container.BeginLifetimeScope();
        }
    }
}
