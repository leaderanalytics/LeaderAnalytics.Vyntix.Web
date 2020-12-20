using LeaderAnalytics.Vyntix.Web.Model;
using LeaderAnalytics.Vyntix.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Tests
{
    [TestClass]
    public class StripeTests : StripeBase
    {
        private const string CUSTOMER_EMAIL = "samspam92842@gmail.com";
        private const string PLAN_DESC = "Daily Subscription Plan";
        private const string USER_ID = "0216a845-3d8e-45c9-a67f-fcc04accded2";

        [TestMethod]
        public async Task CreateSubscriptionIfOneDoesNotAlreadyExist()
        {
            await Delete_Customer_and_prior_subscriptions();
            await Create_paid_subscription_without_trial_period();
        }

        private async Task Delete_Customer_and_prior_subscriptions()
        {
            LeaderAnalytics.Vyntix.Web.Services.SubscriptionService subService = Container.GetService<LeaderAnalytics.Vyntix.Web.Services.SubscriptionService>();
            Customer customer = await subService.GetCustomerByEmailAddress(CUSTOMER_EMAIL);

            if (customer != null)
                await subService.DeleteCustomer(customer.Id);
        }

        private async Task Create_subscription_with_trial_period()
        {
            LeaderAnalytics.Vyntix.Web.Services.SubscriptionService subService = Container.GetService<LeaderAnalytics.Vyntix.Web.Services.SubscriptionService>();
            SubscriptionPlan plan = subService.GetSubscriptionPlans().First(x => x.PlanDescription == PLAN_DESC);
            Customer customer = await subService.CreateCustomer(CUSTOMER_EMAIL);
            SubscriptionOrder order = new SubscriptionOrder { CustomerID = customer.Id, UserEmail = CUSTOMER_EMAIL, UserID = USER_ID, PaymentProviderPlanID = plan.PaymentProviderPlanID };
            CreateSubscriptionResponse response = await subService.CreateInvoicedSubscription(order,"https://localhost");

        }

        private async Task Create_paid_subscription_without_trial_period()
        {
            LeaderAnalytics.Vyntix.Web.Services.SubscriptionService subService = Container.GetService<LeaderAnalytics.Vyntix.Web.Services.SubscriptionService>();
            SubscriptionPlan plan = subService.GetSubscriptionPlans().First(x => x.PlanDescription == PLAN_DESC);
            Customer customer = await subService.CreateCustomer(CUSTOMER_EMAIL);
            SubscriptionOrder order = new SubscriptionOrder { CustomerID = customer.Id, UserEmail = CUSTOMER_EMAIL, UserID = USER_ID, PaymentProviderPlanID = plan.PaymentProviderPlanID };
            // Create and save the sub here because we want to force trial days to zero.
            Stripe.SubscriptionCreateOptions options = new Stripe.SubscriptionCreateOptions();
            Stripe.SubscriptionService stripeSubService = new Stripe.SubscriptionService(StripeClient);

            options.Customer = order.CustomerID;
            options.CollectionMethod = "send_invoice"; // Do not auto-charge customers account
            options.TrialPeriodDays = 0;
            options.Items = new List<SubscriptionItemOptions>(2);
            options.Items.Add(new SubscriptionItemOptions { Price = order.PaymentProviderPlanID, Quantity = 1 });
            options.DaysUntilDue = 1;
            Stripe.Subscription sub = await stripeSubService.CreateAsync(options);

        }
    }
}
