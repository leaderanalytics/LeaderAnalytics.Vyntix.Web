using LeaderAnalytics.Vyntix.Web.Models;
using Microsoft.AspNetCore.Http;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Services
{
    public class SubscriptionService
    {
        private GraphService graphService;
        private StripeClient stripeClient;

        public SubscriptionService(GraphService graphService, StripeClient stripeClient)
        {
            this.graphService = graphService;
            this.stripeClient = stripeClient;
        }

        public async Task<OrderApprovalResponse> ApproveSubscriptionOrder(SubscriptionOrder order, string hostURL) 
        {
            OrderApprovalResponse response = new OrderApprovalResponse();

            if (order == null)
                throw new ArgumentNullException("order");

            // Check the user

            if (string.IsNullOrEmpty(order.UserID))
                response.ErrorMessage = "User ID cannot be null.  Please log in before purchasing a subscription.";
            else if (!await graphService.VerifyUser(order.UserID))
                response.ErrorMessage = "Invalid User ID.";

            if (!String.IsNullOrEmpty(response.ErrorMessage))
                return response;


            // To do: In Renewals, check for an existing subscription and customerID.
            // If found, update the existing subscription


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
                SuccessUrl = hostURL +  "/Subscription/CreateSubscription?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = hostURL + "/SubActivationFailure",
            };

            // Call Stripe and create the session
            Session session = await new SessionService().CreateAsync(options);
            response.SessionID = session.Id;
            return response;
        }
    }
}
