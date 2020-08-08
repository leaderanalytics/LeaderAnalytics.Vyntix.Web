using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Models
{
    public class SubscriptionOrder
    {
        public string UserID { get; set; }                  // User ID maintained in Azure
        public string CustomerID { get; set; }              // Created and maintained in Stripe -
        public string SubscriptionID { get; set; }          // Created and maintained in Stripe.  Users existing subscription ID, if any.
        public string PlanPaymentProviderID { get; set; }   // Created and maintained in Stripe. This is the pricing identifier of the subscription the user wishes to purchase.
        public string PromoCodes { get; set; }              // Comma delimited list of coupon, discount, promo etc. codes.
    }
}
