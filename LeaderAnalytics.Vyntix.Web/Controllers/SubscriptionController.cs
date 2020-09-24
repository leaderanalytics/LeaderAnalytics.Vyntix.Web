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
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace LeaderAnalytics.Vyntix.Web.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SubscriptionController : Controller
    {
        private SubscriptionService subscriptionService;

        public SubscriptionController(SubscriptionService subscriptionService)
        {
            this.subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
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
            CreateSessionResponse response = await subscriptionService.ApproveSubscriptionOrder(order);

            if (string.IsNullOrEmpty(response.ErrorMessage))
                response = await subscriptionService.CreateSession(order, uri);
            
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
            string errorMsg = await subscriptionService.ConfirmOrderCreationFromSession(sessionID);

            if (string.IsNullOrEmpty(errorMsg))
                redirect += ($"/SubActivationSuccess");  
            else
                redirect += ($"/SubActivationFailure");

            return new RedirectResult(redirect, false);
        }

        [HttpPost]
        public async Task<ActionResult> ManageSubscription([FromBody] string customerID) {
            string host = $"{this.Request.Scheme}://{this.Request.Host}";
            AsyncResult<string> result = await subscriptionService.CreateStripePortalSession(customerID, host);

            if (!result.Success)
                return BadRequest(JsonSerializer.Serialize(result.ErrorMessage));
            else
                return new JsonResult(result.Result);
        }

        [HttpPost]
        public async Task<ActionResult<SubscriptionInfoResponse>> GetSubscriptionInfo([FromBody] string userEmail)
        {
            SubscriptionInfoResponse response = await subscriptionService.GetSubscriptionInfo(userEmail);
            return response;
        }
    }
}
