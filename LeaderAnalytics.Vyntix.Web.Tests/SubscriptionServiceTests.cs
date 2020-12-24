using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using System.IO;
using LeaderAnalytics.Vyntix.Web.Model;
using LeaderAnalytics.Vyntix.Web.Services;
using System.Threading.Tasks;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Identity.Client;
using Microsoft.Graph.Auth;
using System.Linq;
using System;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using LeaderAnalytics.Vyntix.Web.Domain;

namespace LeaderAnalytics.Vyntix.Web.Tests
{
    [TestClass]
    public class SubscriptionServiceTests : BaseTest
    {
        
        [TestMethod]
        public async Task SendCoporateSubscriptionApprovalEmail()
        {
            // Create mock admin and subscriber and send credentials request email

            await CreateMocks();
            IServiceProvider serviceProvider = Container.BuildServiceProvider();
            SubscriptionService subService = (SubscriptionService)serviceProvider.GetService(typeof(SubscriptionService));


            IGraphService graphService = (IGraphService)serviceProvider.GetService(typeof(IGraphService));
            UserRecord one = await graphService.GetUserRecordByID("1");



            await subService.SendCorpSubscriptionApprovalEmail("1", "2", "https://localhost");
        }


        private async Task CreateMocks()
        {
            UserRecord admin = new UserRecord { IsCorporateAdmin = true};
            admin.User.Id = "1";
            admin.User.Identities = admin.User.Identities.Append(new ObjectIdentity { IssuerAssignedId = "sam.wheat@outlook.com", SignInType = "emailAddress" });

            UserRecord subscriber = new UserRecord { IsCorporateAdmin = false };
            subscriber.User.Id = "2";
            subscriber.User.Identities = subscriber.User.Identities.Append(new ObjectIdentity { IssuerAssignedId = "subscriber@domain.com", SignInType = "emailAddress" });
            subscriber.User.DisplayName = "Bob Smith";

            Mock<IGraphService> graphServiceMock = new Mock<IGraphService>();
            graphServiceMock.Setup(x => x.GetUserRecordByID(It.Is<string>(b => b == "1"))).ReturnsAsync(admin);
            graphServiceMock.Setup(x => x.GetUserRecordByID(It.Is<string>(b => b == "2"))).ReturnsAsync(subscriber);
            Container.AddSingleton<IGraphService>(graphServiceMock.Object);
        }
    }
}
