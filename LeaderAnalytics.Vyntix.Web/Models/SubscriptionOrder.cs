using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Models
{
    public class SubscriptionOrder
    {
        public string UserID { get; set; }                  // User ID maintained in Azure
        public string UserEmail { get; set; }               // User email maintained in Azure
        public string CustomerID { get; set; }              // Created and maintained in Stripe -
        public Subscription PriorSubscription { get; set; } // Prior subscription for same product, if any.  Prior subscription may be active or expired.
        public string PaymentProviderPlanID { get; set; }   // Created and maintained in Stripe. This is the pricing identifier of the subscription the user wishes to purchase.
        public string PromoCodes { get; set; }              // Comma delimited list of coupon, discount, promo etc. codes.
    }
}
