using LeaderAnalytics.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Model
{
    public class Subscription : ITemporal
    {
        public string SubscriptionID { get; set; }          // Created and maintained in Stripe.  
        public string PaymentProviderPlanID { get; set; }   // Created and maintained in Stripe.
        public string PlanDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public bool IsActive => Status == "active" || Status == "trialing";
    }
}
