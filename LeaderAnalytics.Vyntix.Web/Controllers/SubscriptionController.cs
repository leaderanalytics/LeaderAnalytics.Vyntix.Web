using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Web.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaderAnalytics.Core;
using LeaderAnalytics.Vyntix.Web.Services;
using System.Collections.Concurrent;

namespace LeaderAnalytics.Vyntix.Web.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SubscriptionController : Controller
    {
        private SubscriptionService subscriptionService;

        public SubscriptionController(SubscriptionService subscriptionService)
        {
            this.subscriptionService = subscriptionService ?? throw new ArgumentNullException("subscriptionService");
        }

        [HttpGet]
        public ActionResult<string> Identity()
        {
            return "Leader Analytics Vyntix Web API";
        }

        [HttpGet]
        public JsonResult GetActiveSubscriptionPlans()
        {
            return Json(subscriptionService.GetActiveSubscriptionPlans());
        }


        // This method is called when user clicks "Proceed to checkout".  It calls the payment processor and requests a session be created.
        [HttpPost]
        public async Task<ActionResult<CreateSessionResponse>> ApproveSubscriptionOrder(SubscriptionOrder order) 
        {
            string uri = this.Request.Scheme + "://" + this.Request.Host;
            CreateSessionResponse response = await subscriptionService.ApproveSubscriptionOrder(order, uri);

            if (string.IsNullOrEmpty(response.ErrorMessage))
                return Ok(response);
            else
                return StatusCode(300, response);
        }

        // This method is called by the payment processor after the user has submitted payment.
        [HttpGet]
        public async Task<ActionResult> CreateSubscription()
        {
            string sessionID = Request.Query["session_id"];
            string redirect = $"{this.Request.Scheme}://{this.Request.Host}";
            string errorMsg = await subscriptionService.CreateSubscription(sessionID);

            if (string.IsNullOrEmpty(errorMsg))
                redirect += ($"/SubActivationSuccess");  
            else
                redirect += ($"/SubActivationFailure");

            return new RedirectResult(redirect, false);
        }
    }
}
