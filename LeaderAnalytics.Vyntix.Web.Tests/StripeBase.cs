﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using NUnit;
using NUnit.Framework;
using Stripe;
using Moq;
using Microsoft.AspNetCore.Hosting;
using LeaderAnalytics.Vyntix.Web;
using Microsoft.Extensions.DependencyInjection;

namespace LeaderAnalytics.Vyntix.Web.Tests
{
    
    public abstract class StripeBase
    {
        protected IConfiguration Configuration { get; private set; }
        protected ServiceProvider Container { get; private set; }
        protected StripeClient StripeClient { get; private set; }

        public StripeBase()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json");
            IConfiguration config = builder.Build();
            Mock<IWebHostEnvironment> envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(x => x.EnvironmentName).Returns("Development");
            IWebHostEnvironment env = envMock.Object;
            Startup startup = new Startup(env, config);
            IServiceCollection serviceCollection = new ServiceCollection();
            startup.ConfigureServices(serviceCollection);
            Container = serviceCollection.BuildServiceProvider();
            StripeClient = Container.GetService<StripeClient>();
        }

        [SetUp]
        public void Setup()
        {
        }
    }
}
