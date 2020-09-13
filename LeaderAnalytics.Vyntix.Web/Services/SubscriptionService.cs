using LeaderAnalytics.Vyntix.Web.Models;
using Microsoft.AspNetCore.Http;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using System.Text.Json;
using LeaderAnalytics.Core;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;

namespace LeaderAnalytics.Vyntix.Web.Services
{
    public class SubscriptionService
    {
        private GraphService graphService;
        private StripeClient stripeClient;
        private SessionCache sessionCache;
        private string subscriptionFile;
        private List<SubscriptionPlan> subscriptionPlans;

        public SubscriptionService(GraphService graphService, StripeClient stripeClient, SessionCache sessionCache, string subscriptionFilePath)
        {
            this.graphService = graphService;
            this.stripeClient = stripeClient;
            this.sessionCache = sessionCache;
            this.subscriptionFile = subscriptionFilePath ?? throw new ArgumentNullException("subscriptionFilePath");
            subscriptionPlans = new List<SubscriptionPlan>();
        }


        public List<SubscriptionPlan> GetActiveSubscriptionPlans() => GetSubscriptionPlans().Where(x => x.IsCurrent()).ToList();

        public List<SubscriptionPlan> GetSubscriptionPlans()
        {
            if (subscriptionPlans.Any())
                return subscriptionPlans;


            if (!System.IO.File.Exists(subscriptionFile))
                throw new Exception($"Subscription File {subscriptionFile} was not found.");

            subscriptionPlans = JsonSerializer.Deserialize<List<SubscriptionPlan>>(System.IO.File.ReadAllBytes(subscriptionFile));

            if (subscriptionPlans == null || !subscriptionPlans.Any())
                throw new Exception($"Subscription File {subscriptionFile} was not parsed correctly.  No subscription plans were found.");

            return subscriptionPlans;
        }

        public async Task<SubscriptionOrder> GetPriorSubscriptions(SubscriptionOrder order)
        {
            LeaderAnalytics.Vyntix.Web.Models.Subscription sub = null;

            CustomerService customerService = new CustomerService();
            Customer customer = (await customerService.ListAsync(new CustomerListOptions { Email = order.UserEmail })).FirstOrDefault();
            
            if (customer == null)
                return order;

            order.CustomerID = customer.Id;

            if (customer.Subscriptions?.Any() ?? false)
            {
                // Find prior subscription, if any
                var stripeSub = customer.Subscriptions.FirstOrDefault(x => x.Plan.Id == order.PaymentProviderPlanID);
                
                if (stripeSub != null)
                {
                    sub = new Models.Subscription();
                    sub.SubscriptionID = stripeSub.Id;
                    sub.PaymentProviderPlanID = stripeSub.Plan.Id;
                    sub.PlanDescription = GetSubscriptionPlans().FirstOrDefault(x => x.PaymentProviderPlanID == order.PaymentProviderPlanID)?.PlanDescription;
                    sub.StartDate = stripeSub.CurrentPeriodStart;
                    sub.EndDate = stripeSub.CurrentPeriodEnd;
                    order.PriorSubscription = sub;
                }
            }
            return order;
        }


        public async Task<CreateSessionResponse> ApproveSubscriptionOrder(SubscriptionOrder order, string hostURL) 
        {
            if (order == null)
                throw new ArgumentNullException("order");
            if (string.IsNullOrEmpty(hostURL))
                throw new ArgumentNullException("hostURL");

            CreateSessionResponse response = new CreateSessionResponse();

            // Check the user
            if (string.IsNullOrEmpty(order.UserEmail))
                response.ErrorMessage = "User Email cannot be null.  Please log in before purchasing a subscription.";
            else if (string.IsNullOrEmpty(order.UserID))
                response.ErrorMessage = "User ID cannot be null.  Please log in before purchasing a subscription.";
            else if (!await graphService.VerifyUser(order.UserID))
                response.ErrorMessage = "Invalid User ID.";

            if (!String.IsNullOrEmpty(response.ErrorMessage))
                return response;

            // To do: In Renewals, check for an existing subscription and customerID.
            // If found, update the existing subscription
            // User cannot have two subscriptions for same product (todo: why not)
            // User must be admin to modify subscription
            // Check CanSubscribe and CanRenew flag on subscription
            SubscriptionPlan plan = GetActiveSubscriptionPlans().FirstOrDefault(x => x.PaymentProviderPlanID == order.PaymentProviderPlanID);

            if (plan == null)
            {
                response.ErrorMessage = $"Subscription plan {order.PaymentProviderPlanID} was not found in the active subscription plan table.";
                return response;
            }

            if (plan.Cost > 0)
            {
                order = await GetPriorSubscriptions(order);
                response = await CreateSession(order, hostURL);
            }
            return response;
        }

        private async Task<CreateSessionResponse> CreateSession(SubscriptionOrder order, string hostURL)
        {
            CreateSessionResponse response = new CreateSessionResponse();

            // Create stripe session options
            SessionCreateOptions options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions { Price = order.PaymentProviderPlanID, Quantity = 1 }
                },
                Customer = order.CustomerID,
                CustomerEmail = string.IsNullOrEmpty(order.CustomerID)? order.UserEmail : null,
                Mode = "subscription",
                SuccessUrl = hostURL + "/Subscription/CreateSubscription?session_id={CHECKOUT_SESSION_ID}",    // Called AFTER user submits payment
                CancelUrl = hostURL + "/SubActivationFailure",                                                  // Called only if user cancels
            };
            
            // causes stripe to throw
            //if (order.PriorSubscription != null)
            //{
            //    options.SubscriptionData = new SessionSubscriptionDataOptions
            //    {
            //        Items = new List<SessionSubscriptionDataItemOptions> { new SessionSubscriptionDataItemOptions { Plan = order.PaymentProviderPlanID, Quantity = 1 } }
            //    };
            
            //}
            // Call Stripe and create the session
            Session session = null;
            Log.Information("CreateSession: Creating session for order options: {$options}", options);

            try
            {
                session = await new SessionService().CreateAsync(options);
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                Log.Fatal("A call to SessionService.CreateAsync failed.  The error msg is {e}", s);
                response.ErrorMessage = s;
                return response;
            }

            response.SessionID = session.Id;

            if (!sessionCache.AddSession(new OrderSession(session.Id, order)))
            {
                response.ErrorMessage = $"Unable to add {session.Id} to SessionCache.  Possible dupe.";
                Log.Error(response.ErrorMessage);
                return response;
            }
            
            Log.Information("CreateSession: Session {s} was created.", session.Id);
            return response;
        }

        public async Task<string> CreateSubscription(string sessionID)
        {
            if (string.IsNullOrEmpty(sessionID))
                return "sessionID cannot be null.";

            string errorMsg = null;

            // Get the order from SessionCache --------------------------------
            OrderSession orderSession = sessionCache.GetSession(sessionID);

            if (orderSession == null)
            {
                errorMsg = $"SessionID {sessionID} could not be retrieved from SessionCache.  The session possibly expired.";
                Log.Error(errorMsg);
                return errorMsg;
            }

            // Get the session from Stripe --------------------------------
            Session stripeSession = null;
            try
            {
                stripeSession = await new SessionService().GetAsync(sessionID);
            }
            catch (Exception ex)
            {
                errorMsg = $"A call to SessionService.GetAsync failed. The SessionID is {sessionID}  The error msg is {ex.ToString()}";
                Log.Fatal(errorMsg);
                return errorMsg;
            }


            // Get the subscription from Stripe  --------------------------------
            string customerID = stripeSession.CustomerId;
            string subscriptionID = stripeSession.SubscriptionId;
            Stripe.Subscription subscription = null;
            
            try
            {
                 subscription = await new Stripe.SubscriptionService().GetAsync(subscriptionID);
            }
            catch (Exception ex)
            {
                errorMsg = $"A call to SubscriptionService.GetAsync failed. The subscriptionID is {subscriptionID}  The error msg is {ex.ToString()}";
                Log.Fatal(errorMsg);
                return errorMsg;
            }

            // Compare the itemID to the order that was put into session.  --------------------------------
            if (orderSession.Order.PaymentProviderPlanID != subscription.Plan.Id)
            {
                errorMsg = $"The Plan Id on the subscription ({subscription.Plan.Id}) does not match the PlanId selected by the user on the order ({orderSession.Order.PaymentProviderPlanID}).";
                Log.Fatal(errorMsg);
                return errorMsg;
            }
         

            
            // Write it to renewals here.


            return errorMsg;
        }
    }
}
