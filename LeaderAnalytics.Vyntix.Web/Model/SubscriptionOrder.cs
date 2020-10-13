using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Model
{
    public class SubscriptionOrder
    {
        public string UserID { get; set; }                  // User ID maintained in Azure
        public string UserEmail { get; set; }               // User email maintained in Azure
        public string CustomerID { get; set; }              // Created and maintained in Stripe -
        public List<Subscription> PriorSubscriptions { get; set; } // Prior subscriptions, if any.  Prior subscriptions may be active or expired.
        public string PaymentProviderPlanID { get; set; }   // Created and maintained in Stripe. This is the pricing identifier of the subscription the user wishes to purchase.
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public string PromoCodes { get; set; }              // Comma delimited list of coupon, discount, promo etc. codes.
        public string Captcha { get; set; }

        public SubscriptionOrder()
        {
            PriorSubscriptions = new List<Subscription>(10);
        }
    }
}
