using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using LeaderAnalytics.Core;

namespace LeaderAnalytics.Vyntix.Web.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SubscriptionController : Controller
    {
        private List<SubscriptionPlan> subscriptonPlans;

        public SubscriptionController(List<SubscriptionPlan> subscriptonPlans)
        {
            this.subscriptonPlans = subscriptonPlans;
        }


        [HttpGet]
        public ActionResult<string> Identity()
        {
            return "Leader Analytics Vyntix Web API";
        }

        [HttpGet]
        public async Task<JsonResult> GetActiveSubscriptionPlans()
        {
            return Json(subscriptonPlans.Where(x => x.IsCurrent()));
        }

    }
}
