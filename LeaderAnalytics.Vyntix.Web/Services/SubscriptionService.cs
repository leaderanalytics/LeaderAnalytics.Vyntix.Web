using LeaderAnalytics.Vyntix.Web.Model;
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

        public async Task<Customer> GetCustomerByEmailAddress(string customerEmail)
        {
            CustomerService customerService = new CustomerService(stripeClient);
            Customer customer = (await customerService.ListAsync(new CustomerListOptions { Email = customerEmail })).FirstOrDefault();
            return customer;
        }

        public async Task<List<Model.Subscription>> GetSubscriptionsForCustomer(Customer customer)
        {
            List<Model.Subscription> subs = new List<Model.Subscription>(10);
            
            if (customer == null)
                return subs;

            if (customer.Subscriptions?.Any() ?? false)
            {
                // Find prior subscription, if any
                foreach (var stripeSub in customer.Subscriptions)
                {
                    if (stripeSub != null)
                    {
                        Model.Subscription sub = new Model.Subscription();
                        sub.SubscriptionID = stripeSub.Id;
                        sub.PaymentProviderPlanID = stripeSub.Plan.Id;
                        sub.PlanDescription = GetSubscriptionPlans().FirstOrDefault(x => x.PaymentProviderPlanID == stripeSub.Plan.Id)?.PlanDescription;
                        sub.StartDate = stripeSub.CurrentPeriodStart;
                        sub.EndDate = stripeSub.CurrentPeriodEnd;
                        subs.Add(sub);
                    }
                }
            }
            return subs;
        }

        public async Task<CreateSessionResponse> ApproveSubscriptionOrder(SubscriptionOrder order) 
        {
            if (order == null)
                throw new ArgumentNullException("order");

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
            
            SubscriptionPlan plan = GetActiveSubscriptionPlans().FirstOrDefault(x => x.PaymentProviderPlanID == order.PaymentProviderPlanID);

            if (plan == null)
            {
                response.ErrorMessage = $"Subscription plan {order.PaymentProviderPlanID} was not found in the active subscription plan table.";
                return response;
            }

            order.SubscriptionPlan = plan;
            Customer customer = await GetCustomerByEmailAddress(order.UserEmail);
            
            if (customer != null)
            {
                order.CustomerID = customer.Id;
                order.PriorSubscriptions = await GetSubscriptionsForCustomer(customer);
            }
            
            return response;
        }

        public async Task<CreateSessionResponse> CreateSession(SubscriptionOrder order, string hostURL)
        {
            if (string.IsNullOrEmpty(hostURL))
                throw new ArgumentNullException("hostURL");

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
                CancelUrl = hostURL + "/SubActivationFailure"                                                  // Called only if user cancels
                
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

        public async Task<string> ConfirmOrderCreationFromSession(string sessionID)
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
            
            


            return errorMsg;
        }

        public async Task<AsyncResult<Stripe.Subscription>> CreateSubscription(SubscriptionOrder order) 
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            AsyncResult<Stripe.Subscription> result = new AsyncResult<Stripe.Subscription>();
            CreateSessionResponse response = await ApproveSubscriptionOrder(order);

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                result.ErrorMessage = response.ErrorMessage;
                return result;
            }

            Stripe.SubscriptionCreateOptions options = new Stripe.SubscriptionCreateOptions();
            Stripe.SubscriptionService stripeSubService = new Stripe.SubscriptionService(stripeClient);
            //trialPeriodDays is zero (if the sub is business and the customer has previously purchased or used any other subscription) OR (the price for the sub is zero i.e. non business or promo) 
            int trialPeriodDays = (order.PriorSubscriptions?.Any() ?? false) || (order.SubscriptionPlan.Cost == 0) ? 0 : 30; 

            options.Customer = order.CustomerID;
            options.CollectionMethod = "send_invoice"; // Do not auto-charge customers account
            options.TrialPeriodDays = trialPeriodDays;
            options.Items = new List<SubscriptionItemOptions>(2);
            options.Items.Add(new SubscriptionItemOptions { Price = order.PaymentProviderPlanID, Quantity = 1 });
            options.DaysUntilDue = 0;
            options.CollectionMethod = order.SubscriptionPlan.Cost == 0 ? "charge_automatically" : "send_invoice";
            options.CancelAtPeriodEnd = false;

            try
            {
                Stripe.Subscription sub = await stripeSubService.CreateAsync(options);
                result.Result = sub;
                result.Success = true;
            }
            catch (Exception ex)
            {
                response.ErrorMessage = "Subscription creation failed.  Please try again later.";
                Log.Error("Error creating subscription. UserID: {u}, Ex: {e} ", order.UserID, ex.ToString());
            }
            return result;
        }

        public async Task<Customer> CreateCustomer(string email)
        {
            Customer customer = await GetCustomerByEmailAddress(email);
            
            if (customer != null)
                return customer;

            CustomerService customerService = new CustomerService(stripeClient);
            await customerService.CreateAsync(new CustomerCreateOptions { Email = email });
            return await GetCustomerByEmailAddress(email);
        }

        public async Task<bool> DeleteCustomer(string customerID)
        {

            if (string.IsNullOrEmpty(customerID))
                throw new ArgumentNullException(nameof(customerID));

            CustomerService customerService = new CustomerService();
            await customerService.DeleteAsync(customerID);
            return true;
        }

        public async Task ExtendSubscription(List<string> customerIDs, int daysToExtend) 
        { 
            // ToDo: Extend the subscription of each passed customerID by daysToExtend
            // May to use this if there is a site outage 
        }

        public async Task<AsyncResult<string>> CreateStripePortalSession(string customerID, string hostUrl)
        {
            AsyncResult<string> result = new AsyncResult<string>();
            Customer customer = await new CustomerService(stripeClient).GetAsync(customerID);

            if (customer == null)
                throw new Exception($"Bad customerID: {customerID}");

            List<Model.Subscription> subscriptions = await GetSubscriptionsForCustomer(customer);

            if (!subscriptions.Any())
            {
                result.ErrorMessage = "No subscriptions.  Subscribe to a subscription plan first.";
                return result;
            }

            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customerID,
                ReturnUrl = hostUrl + (hostUrl.EndsWith('/') ? "" : "/") + "lsi/1"
            };
            

            var service = new Stripe.BillingPortal.SessionService(stripeClient);

            try
            {
                var session = service.Create(options);
                result.Result = session.Url;
                result.Success = true;
            }
            catch (Exception ex)
            {
                Log.Error("Error creating portal session: {d}", ex.ToString());
                result.ErrorMessage = "An error occurred creating the portal session.";
            }
            return result;
        }

        public async Task<SubscriptionInfoResponse> GetSubscriptionInfo(string userEmail)
        {
            if (string.IsNullOrEmpty(userEmail))
                throw new ArgumentNullException(nameof(userEmail));
            
            SubscriptionInfoResponse response = new SubscriptionInfoResponse();
            Customer customer = await GetCustomerByEmailAddress(userEmail);

            if (customer == null)
                return response;

            response.CustomerID = customer.Id;

            if (customer.Subscriptions?.Any() ?? false)
            {
                var sub = customer.Subscriptions.FirstOrDefault(x => x.Status == "active");

                if (sub != null)
                {
                    response.SubscriptionID = sub.Items?.FirstOrDefault()?.Plan.Id;
                    response.IsSuscriptionActive = true;
                }
            }
            return response;
        }

        
    }
}
