using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace LeaderAnalytics.Vyntix.Web.Controllers
{
    public class SubscriptionController : Controller
    {
        private List<SubscriptionPlan> subscriptionPlans;

        public SubscriptionController(List<SubscriptionPlan> subscriptionPlans)
        {
            this.subscriptionPlans = subscriptionPlans;
        }

        
        public IActionResult Subscribe()
        {
            ViewData["stripe_pk"] = StripeConfiguration.ApiKey;
            return View(subscriptionPlans);
        }


        public IActionResult AccountSetup(string priceID)
        {
            return View(subscriptionPlans);
        }
    }
}
