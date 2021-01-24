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
using Autofac;
using Stripe;

namespace LeaderAnalytics.Vyntix.Web.Tests
{
    [TestClass]
    public class SubscriptionServiceTests : BaseTest
    {
        private const string user1email = "samspam92841@gmail.com";
        private const string user2email = "samspam92842@gmail.com";
        private string updatedBillingID;

        public SubscriptionServiceTests()
        {
            Setup();
            Delete_Customer_and_prior_subscriptions().Wait();
        }

        private async Task Delete_Customer_and_prior_subscriptions()
        {
            ISubscriptionService subService = Container.Resolve<ISubscriptionService>();
            Customer customer = await subService.GetCustomerByEmailAddress(user2email);

            if (customer != null)
                await subService.DeleteCustomer(customer.Id);
        }

        [TestMethod]
        public async Task Create_subscription_with_trial_period()
        {
            ISubscriptionService subService = Container.Resolve<ISubscriptionService>();
            SubscriptionPlan plan = subService.GetSubscriptionPlans().First(x => x.PlanDescription == Domain.Constants.FREE_PLAN_DESC);
            Customer customer = await subService.CreateCustomer(user2email);
            SubscriptionOrder order = new SubscriptionOrder { CustomerID = customer.Id, UserEmail = user2email, UserID = "2", PaymentProviderPlanID = plan.PaymentProviderPlanID };
            CreateSubscriptionResponse response = await subService.CreateSubscription(order, Domain.Constants.LOCALHOST);
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.SubscriptionID);
        }

        [TestMethod]
        public async Task Create_paid_subscription_without_trial_period()
        {
            ISubscriptionService subService = Container.Resolve<ISubscriptionService>();
            SubscriptionPlan plan = subService.GetSubscriptionPlans().First(x => x.PlanDescription == Domain.Constants.FREE_PLAN_DESC);
            Customer customer = await subService.CreateCustomer(user2email);
            SubscriptionOrder order = new SubscriptionOrder { CustomerID = customer.Id, UserEmail = user2email, UserID = "2", PaymentProviderPlanID = plan.PaymentProviderPlanID };
            // Create and save the sub here because we want to force trial days to zero.
            Stripe.SubscriptionCreateOptions options = new Stripe.SubscriptionCreateOptions();
            Stripe.SubscriptionService stripeSubService = Container.Resolve<Stripe.SubscriptionService>();

            options.Customer = order.CustomerID;
            options.CollectionMethod = "send_invoice"; // Do not auto-charge customers account
            options.TrialPeriodDays = 0;
            options.Items = new List<SubscriptionItemOptions>(2);
            options.Items.Add(new SubscriptionItemOptions { Price = order.PaymentProviderPlanID, Quantity = 1 });
            options.DaysUntilDue = 1;
            Stripe.Subscription sub = await stripeSubService.CreateAsync(options);
            Assert.IsNotNull(sub);
            Assert.IsNotNull(sub.Id);
        }


        /// <summary>
        /// Create a free subscription for a user with no prior subscriptions.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Create_free_subscription_for_first_time_subscriber()
        {
            await RecreateStripeCustomers();
            ISubscriptionService subService = Container.Resolve<ISubscriptionService>();
            IGraphService graphService = Container.Resolve<IGraphService>();
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
            Stripe.Customer customer = await subService.GetCustomerByEmailAddress(subscriber.EMailAddress); // Customer object includes subscriptions
            Assert.IsNotNull(customer);
            List<Model.Subscription> subs = await subService.GetSubscriptionsForCustomer(customer);
            Assert.AreEqual(1, subs.Count);
            Model.Subscription sub = subs.First();
            Assert.AreEqual("active", sub.Status);
            Assert.AreEqual(freePlan.PaymentProviderPlanID, sub.PaymentProviderPlanID);
        }

        /// <summary>
        /// Create a paid subscription for a user with no prior subscriptions.  
        /// This will be an invoiced subscription since the user has a free trial period.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Create_paid_subscription_for_first_time_subscriber()
        {
            await RecreateStripeCustomers();
            ISubscriptionService subService = Container.Resolve<ISubscriptionService>();
            IGraphService graphService = Container.Resolve<IGraphService>();
            UserRecord subscriber = await graphService.GetUserRecordByID("1");
            Assert.IsNotNull(subscriber);
            SubscriptionPlan subPlan = subService.GetSubscriptionPlans().First(x => x.PlanDescription != Domain.Constants.FREE_PLAN_DESC); // Get first paid plan
            Assert.IsNotNull(subPlan);

            SubscriptionOrder order = new SubscriptionOrder
            {
                UserEmail = subscriber.EMailAddress,
                UserID = subscriber.User.Id,
                PaymentProviderPlanID = subPlan.PaymentProviderPlanID
            };
            CreateSubscriptionResponse response = await subService.CreateSubscription(order, Domain.Constants.LOCALHOST);
            Assert.IsNotNull(response);
            Assert.IsNull(response.ErrorMessage);
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.SubscriptionID);
            Stripe.Customer customer = await subService.GetCustomerByEmailAddress(subscriber.EMailAddress); // Customer object includes subscriptions
            Assert.IsNotNull(customer);
            List<Model.Subscription> subs = await subService.GetSubscriptionsForCustomer(customer);
            Assert.AreEqual(1, subs.Count);
            Model.Subscription sub = subs.First();
            Assert.AreEqual("trialing", sub.Status);
            Assert.AreEqual(subPlan.PaymentProviderPlanID, sub.PaymentProviderPlanID);
        }

        /// <summary>
        /// Create a second paid subscription while the free one is still active.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Create_paid_subscription_as_upgrade_from_free()
        {
            // Create the free subscription
            await Create_free_subscription_for_first_time_subscriber();

            // Create a second paid subscription while the free one is still active.
            ISubscriptionService subService = Container.Resolve<ISubscriptionService>();
            IGraphService graphService = Container.Resolve<IGraphService>();
            UserRecord subscriber = await graphService.GetUserRecordByID("1");
            Assert.IsNotNull(subscriber);
            SubscriptionPlan subPlan = subService.GetSubscriptionPlans().First(x => x.PlanDescription != Domain.Constants.FREE_PLAN_DESC); // Get first paid plan
            Assert.IsNotNull(subPlan);

            SubscriptionOrder order = new SubscriptionOrder
            {
                UserEmail = subscriber.EMailAddress,
                UserID = subscriber.User.Id,
                PaymentProviderPlanID = subPlan.PaymentProviderPlanID
            };
            CreateSubscriptionResponse response = await subService.CreateSubscription(order, Domain.Constants.LOCALHOST);
            Assert.IsNotNull(response);
            Assert.IsNull(response.ErrorMessage);
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.SessionID); // We don't have a subscription yet - the user has to log in and pay for it to create it.
            
            // Create a payment just as the customer would do using Stripe checkout
            Stripe.Checkout.SessionService sessionService = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = await sessionService.GetAsync(response.SessionID);
            Assert.IsNotNull(session);

            // Create a payment session
            
            Stripe.PaymentIntent paymentIntent = new Stripe.PaymentIntent();
            paymentIntent.Amount = session.AmountTotal.Value;
            paymentIntent.Currency = "usd";

            //Stripe.Card
            Stripe.PaymentIntentService paymentIntentService = new Stripe.PaymentIntentService();
            // This is as far as we can go with testing.  See this:
            // https://github.com/stripe/stripe-dotnet/issues/2270
            // Coming straight to the point here, it's not possible to complete a Checkout Session payment with only code. 
            // You need to actively visit the Checkout page in a browser to complete the payment.

            Stripe.Customer customer = await subService.GetCustomerByEmailAddress(subscriber.EMailAddress); // Customer object includes subscriptions
            Assert.IsNotNull(customer);
            List<Model.Subscription> subs = await subService.GetSubscriptionsForCustomer(customer);
            
            // We only have one subscription because we have not submitted payment for the second.
            // In the future if Stripe allows submitting payment for a checkout session via code we
            // can do this.  For now, only one subscription will exist.  When this is fixed there
            // should be two:
            Assert.AreEqual(1, subs.Count); 
            
            Model.Subscription sub = subs.Last();
            Assert.AreEqual("active", sub.Status); // Prepaid subscription should not be "trialing" if it is paid for.
            // The only sub we have is the free one:
            //Assert.AreEqual(subPlan.PaymentProviderPlanID, sub.PaymentProviderPlanID);
        }




        [TestMethod]
        public async Task Send_coporate_subscription_request_email()
        {
            ISubscriptionService subService = Container.Resolve<ISubscriptionService>();
            IGraphService graphService = Container.Resolve<IGraphService>();
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
            ISubscriptionService subService = Container.Resolve<ISubscriptionService>();
            IGraphService graphService = Container.Resolve<IGraphService>();
            UserRecord subscriber = await graphService.GetUserRecordByID("2");
            subService.SendCorpSubscriptionNotice(subscriber.EMailAddress, true, null, Domain.Constants.LOCALHOST);
            subService.SendCorpSubscriptionNotice(subscriber.EMailAddress, false, "Your request was denied.", Domain.Constants.LOCALHOST);
        }


        [TestMethod]
        public async Task Create_corporate_subscription()
        {
            this.updatedBillingID = null;
            ISubscriptionService subService = Container.Resolve<ISubscriptionService>();
            IGraphService graphService = Container.Resolve<IGraphService>();
            AsyncResult result = await subService.AllocateCorporateSubscription("1", "2", true, Domain.Constants.LOCALHOST, false);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("1", this.updatedBillingID);
        }

        protected async Task RecreateStripeCustomers()
        { 
            ISubscriptionService subService = Container.Resolve<ISubscriptionService>();
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
            base.CreateMocks();

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
            
            ContainerBuilder.RegisterInstance(graphServiceMock.Object).As<IGraphService>();
            
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
            ContainerBuilder.RegisterInstance(actionContextAccessorMock.Object).As<IActionContextAccessor>();
            // HttpClient

            Mock<HttpMessageHandler> httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",ItExpr.IsAny<HttpRequestMessage>(),ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
                .Verifiable();

            HttpClient httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri(Domain.Constants.LOCALHOST) }; // BaseAddress is required here.
            
            ContainerBuilder.RegisterInstance(httpClient).SingleInstance();
            // Stripe subscription service

            //Mock<Stripe.SubscriptionService> stripeSubscriptionServiceMock = new Mock<Stripe.SubscriptionService>();
            //Stripe.StripeList<Stripe.Subscription> subs = new Stripe.StripeList<Stripe.Subscription>() { Data = new List<Stripe.Subscription>() };
            //subs.Data.Add(new Stripe.Subscription { Status = "active", Items = new Stripe.StripeList<Stripe.SubscriptionItem> { Data = new List<Stripe.SubscriptionItem> { new Stripe.SubscriptionItem { Plan = new Stripe.Plan { Id = "price_1Hca2ILYasEZvdvrfHPI35tB" } } } } });
            //stripeSubscriptionServiceMock.Setup(x => x.ListAsync(It.IsAny<Stripe.SubscriptionListOptions>(), It.IsAny<Stripe.RequestOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(subs);
            
            //Container.AddSingleton(stripeSubscriptionServiceMock.Object);
        }
    }
}
