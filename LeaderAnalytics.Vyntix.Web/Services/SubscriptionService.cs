using LeaderAnalytics.Vyntix.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Services
{
    public class SubscriptionService
    {
        private GraphService graphService;


        public SubscriptionService(GraphService graphService)
        {
            this.graphService = graphService;
        }

        public async Task<string> ApproveSubscriptionOrder(SubscriptionOrder order) 
        {

            if (order == null)
                throw new ArgumentNullException("order");

            string response = null;

            // Check the user

            if (string.IsNullOrEmpty(order.UserID))
                response = "User ID cannot be null.  Please log in before purchasing a subscription.";
            else if (!await graphService.VerifyUser(order.UserID))
                response = "Invalid User ID.";

            // To do: In Renewals, check for an existing subscription and customerID.
            
            return response;
        }
    }
}
