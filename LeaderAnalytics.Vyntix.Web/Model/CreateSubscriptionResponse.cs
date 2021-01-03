using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Model
{
    public class CreateSubscriptionResponse : Domain.AsyncResult
    {
        public string SessionID { get; set; }
        public string SubscriptionID { get; set; }
    }
}
