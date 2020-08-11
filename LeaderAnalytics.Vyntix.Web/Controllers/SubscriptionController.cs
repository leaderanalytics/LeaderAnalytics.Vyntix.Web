using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Web.Models;
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
        private List<SubscriptionPlan> subscriptonPlans;
        private SubscriptionService subscriptionService;
        private ConcurrentDictionary<string, OrderApprovalResponse> Sessions { get; set; }

        public SubscriptionController(List<SubscriptionPlan> subscriptonPlans, SubscriptionService subscriptionService)
        {
            this.subscriptonPlans = subscriptonPlans ?? throw new ArgumentNullException("subscriptionPlans");
            this.subscriptionService = subscriptionService ?? throw new ArgumentNullException("subscriptionService");
            Sessions = new ConcurrentDictionary<string, OrderApprovalResponse>();
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


        // This method is called when user clicks "Proceed to checkout".  It calls the payment processor and requests a session be created.
        [HttpPost]
        public async Task<ActionResult<OrderApprovalResponse>> ApproveSubscriptionOrder(SubscriptionOrder order) 
        {
            string uri = this.Request.Scheme + "://" + this.Request.Host;
            OrderApprovalResponse response = await subscriptionService.ApproveSubscriptionOrder(order, uri);

            if (string.IsNullOrEmpty(response.ErrorMessage))
            {
                // Add the Session to the open session list.  At this point the user is at the payment page for the payment provider.
                // When the user actually makes payment the payment provider will call CreateSubscription.
                Sessions.TryAdd(response.SessionID, response);
                return Ok(response);
            }
            else
            {
                return StatusCode(300, response);
            }
        }

        // This method is called by the payment processor after the user has submitted payment.
        [HttpGet]
        public async Task<ActionResult> CreateSubscription()
        {
            string qs = Request.Query["session_id"];
            string redirect = $"{this.Request.Scheme}://{this.Request.Host}";

            // Clear the Session list of expired sessions.  We will get an expired session if user navigates to payment page than bails.
            ClearExpiredSessions();

            // check the session ID in the cache...
            if (!Sessions.TryGetValue(qs, out OrderApprovalResponse oar))
            {
                // If we get here its probably a bad thing. 
                // It can mean someone is maliciously calling this method or worst case it can mean that 
                // someone has paid for a subscription and we have no session ID by which to track it.

                // Do detailed logging here
                redirect += ($"/SubActivationFailure?session_id={qs}");
            }
            else
            {
                // We have a paying customer.  Create subscription in Renewals...
                redirect += ($"/SubActivationSuccess?session_id={qs}");
            }
            
            return new RedirectResult(redirect, false);
        }

        private void ClearExpiredSessions()
        {
            DateTime now = DateTime.UtcNow;
            List<string> expired = Sessions.Where(x => x.Value.TimeStamp.AddHours(1) < now).Select(x => x.Key).ToList();

            if (expired.Any())
                expired.ForEach(x => Sessions.Remove(x, out OrderApprovalResponse v));

        }
    }
}
