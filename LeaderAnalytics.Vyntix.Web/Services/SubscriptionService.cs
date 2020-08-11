using LeaderAnalytics.Vyntix.Web.Models;
using Microsoft.AspNetCore.Http;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace LeaderAnalytics.Vyntix.Web.Services
{
    public class SubscriptionService
    {
        private GraphService graphService;
        private StripeClient stripeClient;
        private SessionCache sessionCache;

        public SubscriptionService(GraphService graphService, StripeClient stripeClient, SessionCache sessionCache)
        {
            this.graphService = graphService;
            this.stripeClient = stripeClient;
            this.sessionCache = sessionCache;
        }

        public async Task<CreateSessionResponse> ApproveSubscriptionOrder(SubscriptionOrder order, string hostURL) 
        {
            if (order == null)
                throw new ArgumentNullException("order");
            if (string.IsNullOrEmpty(hostURL))
                throw new ArgumentNullException("hostURL");

            CreateSessionResponse response = new CreateSessionResponse();

            // Check the user

            if (string.IsNullOrEmpty(order.UserID))
                response.ErrorMessage = "User ID cannot be null.  Please log in before purchasing a subscription.";
            else if (!await graphService.VerifyUser(order.UserID))
                response.ErrorMessage = "Invalid User ID.";

            if (!String.IsNullOrEmpty(response.ErrorMessage))
                return response;

            // To do: In Renewals, check for an existing subscription and customerID.
            // If found, update the existing subscription
            // User cannot have two subscriptions for same product
            // User must be admin to modify subscription
            // Check CanSubscribe and CanRenew flag on subscription

            response = await CreateSession(order, hostURL);
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
                CustomerEmail = order.UserEmail,
                Mode = "subscription",
                SuccessUrl = hostURL + "/Subscription/CreateSubscription?session_id={CHECKOUT_SESSION_ID}",    // Called AFTER user submits payment
                CancelUrl = hostURL + "/SubActivationFailure",                                                  // Called only if user cancels
            };

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
            Subscription subscription = null;
            
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
