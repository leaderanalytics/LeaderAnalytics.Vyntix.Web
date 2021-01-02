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
using System.Threading;

namespace LeaderAnalytics.Vyntix.Web.Tests
{
    [TestClass]
    public class SubscriptionServiceTests : BaseTest
    {
        // subscriber: samspam92841@gmail.com //4ca22b1d-5299-4cae-ab35-f23ef0f59343
        // admin email: samspam92842@gmail // fe184e4e-0c7d-494f-9b07-fc3bd51eacba
        private const string LOCALHOST = "https://localhost:5031";
        private string updatedBillingID;


        [TestMethod]
        public async Task Send_coporate_subscription_request_email()
        {
            // Create mock admin and subscriber and send credentials request email

            CreateMocks();
            IServiceProvider serviceProvider = Container.BuildServiceProvider();
            SubscriptionService subService = (SubscriptionService)serviceProvider.GetService(typeof(SubscriptionService));

            IGraphService graphService = (IGraphService)serviceProvider.GetService(typeof(IGraphService));
            UserRecord one = await graphService.GetUserRecordByID("1");

            await subService.SendCorpSubscriptionRequestEmail("1", "2", LOCALHOST);
        }


        [TestMethod]
        public async Task Send_coporate_subscription_notice_email()
        {
            // Create mock admin and subscriber

            CreateMocks();
            IServiceProvider serviceProvider = Container.BuildServiceProvider();
            SubscriptionService subService = (SubscriptionService)serviceProvider.GetService(typeof(SubscriptionService));

            IGraphService graphService = (IGraphService)serviceProvider.GetService(typeof(IGraphService));
            UserRecord subscriber = await graphService.GetUserRecordByID("2");
            subService.SendCorpSubscriptionNotice(subscriber.EMailAddress, true, null, LOCALHOST);
            subService.SendCorpSubscriptionNotice(subscriber.EMailAddress, false, "Your request was denied.", LOCALHOST);
        }


        [TestMethod]
        public async Task Create_corporate_subscription_passes()
        {
            // Create mock admin and subscriber
            this.updatedBillingID = null;
            CreateMocks();
            IServiceProvider serviceProvider = Container.BuildServiceProvider();
            SubscriptionService subService = (SubscriptionService)serviceProvider.GetService(typeof(SubscriptionService));
            IGraphService graphService = (IGraphService)serviceProvider.GetService(typeof(IGraphService));
            AsyncResult result = await subService.ModifyCorporateSubscription("1", "2", true, LOCALHOST, false);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("1", this.updatedBillingID);
        }


        private void CreateMocks()
        {
            UserRecord admin = new UserRecord { IsCorporateAdmin = true};
            admin.User.Id = "1";
            admin.User.Identities = admin.User.Identities.Append(new ObjectIdentity { IssuerAssignedId = "samspam92842@gmail.com", SignInType = "emailAddress" });

            UserRecord subscriber = new UserRecord { IsCorporateAdmin = false };
            subscriber.User.Id = "2";
            subscriber.User.Identities = subscriber.User.Identities.Append(new ObjectIdentity { IssuerAssignedId = "samspam92841@gmail.com", SignInType = "emailAddress" });
            subscriber.User.DisplayName = "Bob Smith";

            Mock<IGraphService> graphServiceMock = new Mock<IGraphService>();
            graphServiceMock.Setup(x => x.GetUserRecordByID(It.Is<string>(b => b == "1"))).ReturnsAsync(admin);
            graphServiceMock.Setup(x => x.GetUserRecordByID(It.Is<string>(b => b == "2"))).ReturnsAsync(subscriber);
            graphServiceMock.Setup(x => x.UpdateUser(It.IsAny<UserRecord>())).Callback((UserRecord x) =>
            {
                updatedBillingID = x.BillingID;
            });
            Container.AddSingleton<IGraphService>(graphServiceMock.Object);

            Mock<Stripe.SubscriptionService> subscriptionServiceMock = new Mock<Stripe.SubscriptionService>();
            Stripe.StripeList<Stripe.Subscription> subs = new Stripe.StripeList<Stripe.Subscription>() { Data = new List<Stripe.Subscription>() };
            subs.Data.Add(new Stripe.Subscription { Status = "active", Items = new Stripe.StripeList<Stripe.SubscriptionItem> { Data = new List<Stripe.SubscriptionItem> { new Stripe.SubscriptionItem { Plan = new Stripe.Plan { Id = "price_1Hca2ILYasEZvdvrfHPI35tB" } } } } });
            subscriptionServiceMock.Setup(x => x.ListAsync(It.IsAny<Stripe.SubscriptionListOptions>(), It.IsAny<Stripe.RequestOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(subs);
            Container.AddSingleton(subscriptionServiceMock.Object);
        }
    }
}
