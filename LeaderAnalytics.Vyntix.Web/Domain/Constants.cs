using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Domain
{
    public static class Constants
    {
        // A plan with this PlanDescription must exist in the subscriptions.json file.
        public const string FREE_PLAN_DESC = "Free non-business subscription.  Can not be used for any business activity or purpose.";
        public const string LOCALHOST = "https://localhost:5031";
    }
}
