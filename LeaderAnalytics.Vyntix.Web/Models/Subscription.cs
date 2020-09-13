using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Models
{
    public class Subscription
    {
        public string SubscriptionID { get; set; }          // Created and maintained in Stripe.  
        public string PaymentProviderPlanID { get; set; }   // Created and maintained in Stripe.
        public string PlanDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
