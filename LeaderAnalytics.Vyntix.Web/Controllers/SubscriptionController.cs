using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaderAnalytics.Core;
using LeaderAnalytics.Vyntix.Web.Services;

namespace LeaderAnalytics.Vyntix.Web.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SubscriptionController : Controller
    {
        private List<SubscriptionPlan> subscriptonPlans;
        private SubscriptionService subscriptionService;


        public SubscriptionController(List<SubscriptionPlan> subscriptonPlans, SubscriptionService subscriptionService)
        {
            this.subscriptonPlans = subscriptonPlans ?? throw new ArgumentNullException("subscriptionPlans");
            this.subscriptionService = subscriptionService ?? throw new ArgumentNullException("subscriptionService");
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

        [HttpPost]
        public async Task<ActionResult> ApproveSubscriptionOrder(SubscriptionOrder order) 
        {
            string errorMsg = await subscriptionService.ApproveSubscriptionOrder(order);

            if (string.IsNullOrEmpty(errorMsg))
                return Ok();
            else
                return StatusCode(300, errorMsg);

            
        } 
    }
}
