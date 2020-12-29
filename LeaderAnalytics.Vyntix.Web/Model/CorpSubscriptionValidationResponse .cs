using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Web.Domain;

namespace LeaderAnalytics.Vyntix.Web.Model
{
    public class CorpSubscriptionValidationResponse: CorpSubscriptionInfoResponse
    {
        public string SubscriberEmail { get; set; }
    }
}
