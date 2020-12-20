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


using Microsoft.AspNetCore.WebUtilities;
using Serilog;

namespace LeaderAnalytics.Vyntix.Web.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SubscriptionController : Controller
    {
        private SubscriptionService subscriptionService;
        private string Host => $"{this.Request.Scheme}://{this.Request.Host}";

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
        
        [HttpPost]
        public async Task<JsonResult> IsPrepaymentRequired(SubscriptionOrder order)
        {
            CreateSubscriptionResponse response = await subscriptionService.ApproveSubscriptionOrder(order);
            
            if(string.IsNullOrEmpty(response.ErrorMessage))
                return Json(subscriptionService.IsPrepaymentRequired(order));

            return Json(null);
        }

        [HttpPost]
        public async Task<ActionResult> CreateSubscription(SubscriptionOrder order)
        {
            try
            {
                CreateSubscriptionResponse response = await subscriptionService.CreateSubscription(order, Host);

                if (string.IsNullOrEmpty(response.ErrorMessage))
                    return Ok(response);
                else
                    return StatusCode(300, response);
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                Log.Error(s);
                return StatusCode(500, s);
            }
        }

        // This method is called by the payment processor after the user has submitted payment.
        [HttpGet]                      
        public async Task<ActionResult> ConfirmSubscription()
        {
            string sessionID = Request.Query["session_id"];
            string errorMsg = await subscriptionService.ConfirmSubscription(sessionID);
            return RedirectToStatus(string.IsNullOrEmpty(errorMsg));
        }

        private ActionResult RedirectToStatus(bool success)
        {
            string redirect = $"{Host}/SubActivation" + (success ? "Success" : "Failure");
            return new RedirectResult(redirect, false);
        }

        [HttpPost]
        public async Task<ActionResult> ManageSubscription([FromBody] string customerID) {
            AsyncResult<string> result = await subscriptionService.ManageSubscriptions(customerID, Host);

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

        [HttpPost]
        public async Task<ActionResult<CorpSubscriptionInfoResponse>> GetCorpSubscriptionInfo([FromBody] string userEmail)
        {
            CorpSubscriptionInfoResponse response = await subscriptionService.GetCorpSubscriptionInfo(userEmail);
            return response;
        }

        [HttpPost]
        public void LogInfo([FromBody] string msg)
        {
            var request = Request;
            Log.Information(msg);
        }
        
        [HttpPost]
        public void CorpCredentialApproval([FromBody] (string,string,bool) args)
        {
            string admin = args.Item1;
            string sub = args.Item2;
            bool isApproved = args.Item3;
        }
    }
}
