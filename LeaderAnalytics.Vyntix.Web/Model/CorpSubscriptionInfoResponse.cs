using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Model
{
    public class CorpSubscriptionInfoResponse
    {
        public string AdminEmail { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
    }
}
