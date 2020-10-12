using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Model
{
    public class SubscriptionInfoResponse
    {
        public string CustomerID { get; set; }
        public string SubscriptionID { get; set; }
        public int SubscriptionCount { get; set; }
    }
}
