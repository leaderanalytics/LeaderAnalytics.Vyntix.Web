using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Model
{
    public class CorpSubscriptionAllocationRequest
    {
        public string AdminID { get; set; }
        public string SubscriberID { get; set; }
        public bool IsApproved { get; set; }
    }
}
