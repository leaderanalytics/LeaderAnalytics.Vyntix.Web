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
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Moq.Protected;

namespace LeaderAnalytics.Vyntix.Web.Tests
{
    [TestClass]
    public class SubscriptionServiceTests : BaseTest
    {
        // subscriber: samspam92841@gmail.com //4ca22b1d-5299-4cae-ab35-f23ef0f59343
        // admin email: samspam92842@gmail // fe184e4e-0c7d-494f-9b07-fc3bd51eacba
        private const string user1email = "samspam92841@gmail.com";
        private const string user2email = "samspam92842@gmail.com";
        private string updatedBillingID;

        [TestMethod]
        public async Task Create_free_subscription()
        {
            // Create a free subscription for a user with no prior subscriptions.

            await RecreateStripeCustomers();
            ISubscriptionService subService = (ISubscriptionService)serviceProvider.GetService(typeof(ISubscriptionService));
            IGraphService graphService = (IGraphService)serviceProvider.GetService(typeof(IGraphService));
            UserRecord subscriber = await graphService.GetUserRecordByID("1");
            Assert.IsNotNull(subscriber);
            SubscriptionPlan freePlan = subService.GetSubscriptionPlans().First(x => x.PlanDescription == Domain.Constants.FREE_PLAN_DESC);
            Assert.IsNotNull(freePlan);

            SubscriptionOrder order = new SubscriptionOrder
            {
                UserEmail = subscriber.EMailAddress,
                UserID = subscriber.User.Id,
                PaymentProviderPlanID = freePlan.PaymentProviderPlanID
            };
            CreateSubscriptionResponse response = await subService.CreateSubscription(order, Domain.Constants.LOCALHOST);
            Assert.IsNotNull(response);
            Assert.IsNull(response.ErrorMessage);
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.SubscriptionID);
        }

        [TestMethod]
        public async Task Send_coporate_subscription_request_email()
        {
            ISubscriptionService subService = (ISubscriptionService)serviceProvider.GetService(typeof(ISubscriptionService));
            IGraphService graphService = (IGraphService)serviceProvider.GetService(typeof(IGraphService));
            UserRecord one = await graphService.GetUserRecordByID("1");
            await subService.SendCorpSubscriptionRequestEmail("1", "2", Domain.Constants.LOCALHOST);
        }

        /// <summary>
        /// Sends emails to a mock subscriber notifying them that their subscription request has been approved and denied.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Send_coporate_subscription_notice_email()
        {
            SubscriptionService subService = (SubscriptionService)serviceProvider.GetService(typeof(SubscriptionService));
            IGraphService graphService = (IGraphService)serviceProvider.GetService(typeof(IGraphService));
            UserRecord subscriber = await graphService.GetUserRecordByID("2");
            subService.SendCorpSubscriptionNotice(subscriber.EMailAddress, true, null, Domain.Constants.LOCALHOST);
            subService.SendCorpSubscriptionNotice(subscriber.EMailAddress, false, "Your request was denied.", Domain.Constants.LOCALHOST);
        }


        [TestMethod]
        public async Task Create_corporate_subscription()
        {
            this.updatedBillingID = null;
            SubscriptionService subService = (SubscriptionService)serviceProvider.GetService(typeof(SubscriptionService));
            IGraphService graphService = (IGraphService)serviceProvider.GetService(typeof(IGraphService));
            AsyncResult result = await subService.ModifyCorporateSubscription("1", "2", true, Domain.Constants.LOCALHOST, false);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("1", this.updatedBillingID);
        }

        protected async Task RecreateStripeCustomers()
        { 
            SubscriptionService subService = (SubscriptionService)serviceProvider.GetService(typeof(SubscriptionService));
            Stripe.Customer c = await subService.GetCustomerByEmailAddress(user1email);
            
            if (c != null)
                await subService.DeleteCustomer(c.Id);

            c = await subService.GetCustomerByEmailAddress(user2email);

            if (c != null)
                await subService.DeleteCustomer(c.Id);

            await subService.CreateCustomer(user1email);
            await subService.CreateCustomer(user2email);
        }




        protected override void CreateMocks()
        {
            UserRecord admin = new UserRecord { IsCorporateAdmin = true};
            admin.User.Id = "1";
            admin.User.Identities = admin.User.Identities.Append(new ObjectIdentity { IssuerAssignedId = user1email, SignInType = "emailAddress" });

            UserRecord subscriber = new UserRecord { IsCorporateAdmin = false };
            subscriber.User.Id = "2";
            subscriber.User.Identities = subscriber.User.Identities.Append(new ObjectIdentity { IssuerAssignedId = user2email, SignInType = "emailAddress" });
            subscriber.User.DisplayName = "Bob Smith";

            Mock<IGraphService> graphServiceMock = new Mock<IGraphService>();
            graphServiceMock.Setup(x => x.GetUserRecordByID(It.Is<string>(b => b == "1"))).ReturnsAsync(admin);
            graphServiceMock.Setup(x => x.VerifyUser(It.Is<string>(x => x == "1" || x == "2"))).ReturnsAsync(true);
            graphServiceMock.Setup(x => x.GetUserRecordByID(It.Is<string>(b => b == "2"))).ReturnsAsync(subscriber);
            graphServiceMock.Setup(x => x.UpdateUser(It.IsAny<UserRecord>())).Callback((UserRecord x) =>
            {
                updatedBillingID = x.BillingID;
            });
            Container.AddSingleton<IGraphService>(graphServiceMock.Object);

            // HttpContext

            Mock<ConnectionInfo> connectionInfoMock = new Mock<ConnectionInfo>();
            connectionInfoMock.Setup(x => x.RemoteIpAddress).Returns(new System.Net.IPAddress(0x2414188f));
            

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(x => x.Connection).Returns(connectionInfoMock.Object);

            Mock<ActionContext> actionContextMock = new Mock<Microsoft.AspNetCore.Mvc.ActionContext>();
            ActionContext actionContext = actionContextMock.Object;
            actionContext.HttpContext = httpContextMock.Object; // actionContext.HttpContext is not virtual so we cannot use Setup
            

            Mock<IActionContextAccessor> actionContextAccessorMock = new Mock<IActionContextAccessor>();
            actionContextAccessorMock.Setup(x => x.ActionContext).Returns(actionContext);
            Container.AddSingleton<IActionContextAccessor>(actionContextAccessorMock.Object);

            // HttpClient

            Mock<HttpMessageHandler> httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",ItExpr.IsAny<HttpRequestMessage>(),ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
                .Verifiable();

            HttpClient httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri(Domain.Constants.LOCALHOST) }; // BaseAddress is required here.
            Container.AddSingleton(httpClient);

            // Stripe subscription service

            //Mock<Stripe.SubscriptionService> stripeSubscriptionServiceMock = new Mock<Stripe.SubscriptionService>();
            //Stripe.StripeList<Stripe.Subscription> subs = new Stripe.StripeList<Stripe.Subscription>() { Data = new List<Stripe.Subscription>() };
            //subs.Data.Add(new Stripe.Subscription { Status = "active", Items = new Stripe.StripeList<Stripe.SubscriptionItem> { Data = new List<Stripe.SubscriptionItem> { new Stripe.SubscriptionItem { Plan = new Stripe.Plan { Id = "price_1Hca2ILYasEZvdvrfHPI35tB" } } } } });
            //stripeSubscriptionServiceMock.Setup(x => x.ListAsync(It.IsAny<Stripe.SubscriptionListOptions>(), It.IsAny<Stripe.RequestOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(subs);
            
            //Container.AddSingleton(stripeSubscriptionServiceMock.Object);
        }
    }
}
