using LeaderAnalytics.Vyntix.Web.Model;
using LeaderAnalytics.Vyntix.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Tests
{
    class StripeTests : StripeBase
    {
        private const string CUSTOMER_EMAIL = "samspam92842@gmail.com";
        private const string PLAN_DESC = "Daily Subscription Plan";
        private const string USER_ID = "0216a845-3d8e-45c9-a67f-fcc04accded2";

        [TestCase]
        public async Task CreateSubscriptionIfOneDoesNotAlreadyExist()
        {
            await Delete_Customer_and_prior_subscriptions();
            await Create_subscription_with_trial_period();
            
            
        }

        private async Task Delete_Customer_and_prior_subscriptions()
        {
            LeaderAnalytics.Vyntix.Web.Services.SubscriptionService subService = Container.GetService<LeaderAnalytics.Vyntix.Web.Services.SubscriptionService>();
            Customer customer = await subService.GetCustomerByEmailAddress(CUSTOMER_EMAIL);
            
            if(customer != null)
                await subService.DeleteCustomer(customer.Id);
        }

        private async Task Create_subscription_with_trial_period()
        {
            LeaderAnalytics.Vyntix.Web.Services.SubscriptionService subService = Container.GetService<LeaderAnalytics.Vyntix.Web.Services.SubscriptionService>();
            SubscriptionPlan plan = subService.GetSubscriptionPlans().First(x => x.PlanDescription == PLAN_DESC);
            Customer customer = await subService.CreateCustomer(CUSTOMER_EMAIL);
            SubscriptionOrder order = new SubscriptionOrder {CustomerID = customer.Id, UserEmail = CUSTOMER_EMAIL, UserID = USER_ID, PaymentProviderPlanID = plan.PaymentProviderPlanID };
            AsyncResult<Stripe.Subscription> sub = await subService.CreateSubscription(order);

        }
    }
}
