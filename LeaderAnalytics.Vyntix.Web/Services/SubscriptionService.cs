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
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Net.Http;
using LeaderAnalytics.Core.Azure;

namespace LeaderAnalytics.Vyntix.Web.Services
{
    public class SubscriptionService
    {
        private GraphService graphService;
        private StripeClient stripeClient;
        private SessionCache sessionCache;
        private string subscriptionFile;
        private List<SubscriptionPlan> subscriptionPlans;
        private static HttpClient apiClient;
        private IActionContextAccessor accessor;

        public SubscriptionService(AzureADConfig config, IActionContextAccessor accessor, GraphService graphService, StripeClient stripeClient, SessionCache sessionCache, string subscriptionFilePath)
        {
            this.accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            this.graphService = graphService;
            this.stripeClient = stripeClient;
            this.sessionCache = sessionCache;
            this.subscriptionFile = subscriptionFilePath ?? throw new ArgumentNullException("subscriptionFilePath");
            subscriptionPlans = new List<SubscriptionPlan>();

            if (apiClient == null)
            {
                ClientCredentialsHelper helper = new ClientCredentialsHelper(config);
                apiClient = helper.AuthorizedClient();
            }
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
            Stripe.SubscriptionService subscriptionService = new Stripe.SubscriptionService(stripeClient);
            Customer customer = (await customerService.ListAsync(new CustomerListOptions { Email = customerEmail })).FirstOrDefault();
            
            if (customer != null)
                customer.Subscriptions = await subscriptionService.ListAsync(new SubscriptionListOptions { Customer = customer.Id, Status = "all" });
            
            return customer;
        }

        public async Task<List<Model.Subscription>> GetSubscriptionsForCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            List<Model.Subscription> subs = new List<Model.Subscription>(10);

            if (customer.Subscriptions?.Any() ?? false)
            {
                // Find prior subscription, if any
                foreach (var stripeSub in customer.Subscriptions.Where(x => x != null))
                {
                    // A subscription must have exactly one item.
                    
                    if (stripeSub.Items == null || stripeSub.Items.Count() != 1)
                        throw new Exception($"Subscription with ID {stripeSub.Id} has an invalid number of items.  Customer email is {customer.Email}, Customer ID is {customer.Id}");
                        
                    Stripe.SubscriptionItem stripeSubItem = stripeSub.Items.First();

                    if (stripeSubItem.Plan == null)
                        throw new Exception($"Plan is null for Stripe.SubscriptionItem with ID {stripeSubItem.Id}. Subscription ID is {stripeSub.Id}, Customer email is {customer.Email}, Customer ID is {customer.Id}.");

                    SubscriptionPlan plan = GetSubscriptionPlans().FirstOrDefault(x => x.PaymentProviderPlanID == stripeSubItem.Plan.Id);

                    if (plan == null)
                        throw new Exception($"SubscriptionPlan with ID {stripeSubItem.Plan.Id} was not found on the subscriptionPlans list.");

                    Model.Subscription sub = new Model.Subscription();
                    sub.SubscriptionID = stripeSub.Id;
                    sub.PaymentProviderPlanID = stripeSub.Items.FirstOrDefault()?.Plan.Id ?? "";
                    sub.PlanDescription = plan.PlanDescription;
                    sub.StartDate = stripeSub.CurrentPeriodStart;
                    sub.EndDate = stripeSub.CurrentPeriodEnd;
                    sub.Status = stripeSub.Status;
                    subs.Add(sub);
                }
            }
            return subs;
        }

        /// <summary>
        /// Common logic for validating a subscription order.
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<CreateSubscriptionResponse> ApproveSubscriptionOrder(SubscriptionOrder order) 
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            CreateSubscriptionResponse response = new CreateSubscriptionResponse();

            // Check the user
            if (string.IsNullOrEmpty(order.UserEmail))
                response.ErrorMessage = "User Email cannot be null.  Please log in before purchasing a subscription.";
            else if (string.IsNullOrEmpty(order.UserID))
                response.ErrorMessage = "User ID cannot be null.  Please log in before purchasing a subscription.";
            else if (!await graphService.VerifyUser(order.UserID))
                response.ErrorMessage = "Invalid User ID.";
            if (string.IsNullOrEmpty(order.PaymentProviderPlanID))
                response.ErrorMessage = "Invalid subscription identifier.  PaymentProviderPlanID can not be null.";

            if (!String.IsNullOrEmpty(response.ErrorMessage))
                return response;

            SubscriptionPlan plan = null;

            if (string.IsNullOrEmpty(order.CorpSubscriptionID))
                plan = GetActiveSubscriptionPlans().FirstOrDefault(x => x.PaymentProviderPlanID == order.PaymentProviderPlanID);
            else
            {
                CorpSubscriptionInfoResponse corpResponse = await GetCorpSubscriptionInfo(order.CorpSubscriptionID);

                if (corpResponse == null || string.IsNullOrEmpty(corpResponse.AdminEmail))
                {
                    response.ErrorMessage = "Invalid Corporate Subscription ID.";
                    return response;
                }

                if (corpResponse.SubscriptionPlan == null)
                {
                    response.ErrorMessage = "No active corporate subscription was found.";
                    return response;
                }
                plan = corpResponse.SubscriptionPlan;

                if (order.UserEmail.Split('@')[1]?.ToLower() != corpResponse.AdminEmail.Split('@')[1]?.ToLower())
                {
                    response.ErrorMessage = "User email domain must match email domain of corporate admin.";
                    return response;
                }
            }

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

            // Do not allow creating a free sub if any other sub exists.  It is unnecessary and probably an error.
            if(order.SubscriptionPlan.PlanDescription.StartsWith("Free non-business") && order.PriorSubscriptions.Any())
                response.ErrorMessage = $"A Free non-business subscription can not be created because another subscription already exists.";

            return response;
        }
        
        /// <summary>
        /// Entry point for subscription creation process.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="hostURL"></param>
        /// <returns></returns>
        public async Task<CreateSubscriptionResponse> CreateSubscription(SubscriptionOrder order, string hostURL)
        {
            if (string.IsNullOrEmpty(hostURL))
                throw new ArgumentNullException(nameof(hostURL));

            if(order == null)
                throw new ArgumentNullException(nameof(order));

            CreateSubscriptionResponse response = new CreateSubscriptionResponse();
            string ipaddress = accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();
            var captchaResult = await apiClient.GetAsync($"api/Captcha/Submit?ipaddress={ipaddress}&code={order.Captcha}");

            if (captchaResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                response.ErrorMessage = JsonSerializer.Deserialize<string>(await captchaResult.Content.ReadAsStringAsync());
                return response;
            }

            response = await ApproveSubscriptionOrder(order);

            if (!string.IsNullOrEmpty(response.ErrorMessage))
                return response;

            int trialPeriodDays = GetTrialPeriodDays(order);

            if (IsPrepaymentRequired(order))
                response = await CreatePrepaidSubscription(order, hostURL);
            else
                response = await CreateInvoicedSubscription(order, hostURL);
            
            return response;
        }

        /// <summary>
        /// Creates a subscription where the subscriber must pay immediately to activate the subscription.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="hostURL"></param>
        /// <returns></returns>
        private async Task<CreateSubscriptionResponse> CreatePrepaidSubscription(SubscriptionOrder order, string hostURL)
        {
            CreateSubscriptionResponse response = new CreateSubscriptionResponse();
         
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
                SuccessUrl = hostURL + "/Subscription/ConfirmSubscription?session_id={CHECKOUT_SESSION_ID}",    // Called AFTER user submits payment
                CancelUrl = hostURL + "/SubActivationFailure"
            };

            // Call Stripe and create the session
            Session session = null;
            Log.Information("CreateSession: Creating session for order options: {$options}", options);

            try
            {
                session = await new SessionService().CreateAsync(options);
                response.SessionID = session.Id;
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                Log.Fatal("A call to SessionService.CreateAsync failed.  The error msg is {e}", s);
                response.ErrorMessage = s;
                return response;
            }

            if (!sessionCache.AddSession(new OrderSession(session.Id, order)))
            {
                response.ErrorMessage = $"Unable to add {session.Id} to SessionCache.  Possible dupe.";
                Log.Error(response.ErrorMessage);
                return response;
            }
            
            Log.Information("CreateSession: Session {s} was created.", session.Id);
            return response;
        }

        /// <summary>
        /// Callback called by the payment provider after payment is made for a prepaid subscription. 
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public async Task<string> ConfirmSubscription(string sessionID)
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
            Stripe.SubscriptionService subscriptionService = new Stripe.SubscriptionService();
            try
            {
                 subscription = await subscriptionService.GetAsync(subscriptionID);
            }
            catch (Exception ex)
            {
                errorMsg = $"A call to SubscriptionService.GetAsync failed. The subscriptionID is {subscriptionID}  The error msg is {ex.ToString()}";
                Log.Fatal(errorMsg);
                return errorMsg;
            }

            // Compare the itemID to the order that was put into session.  --------------------------------
            if (orderSession.Order.PaymentProviderPlanID != subscription.Items.First().Plan.Id)
            {
                errorMsg = $"The Plan Id on the subscription ({subscription.Items.First().Plan.Id}) does not match the PlanId selected by the user on the order ({orderSession.Order.PaymentProviderPlanID}).";
                Log.Fatal(errorMsg);
                return errorMsg;
            }

            // Update the collection method to send invoice.  Stripe does not allow this setting in the session object that is sent to the Checkout screen.
            try
            {
                await subscriptionService.UpdateAsync(subscription.Id, new SubscriptionUpdateOptions { CollectionMethod = "send_invoice", DaysUntilDue = 1 });
            }
            catch (Exception ex)
            {
                errorMsg = $"Failed to update subscription with ID ({subscription.Id}).  Customer ID is {customerID}.";
                Log.Fatal(errorMsg);
            }
            return errorMsg;
        }

        /// <summary>
        /// Creates a subscription that is billed to the customer and paid at a future date (typically after a trial period).
        /// Also creates a Corporate Subscription.
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<CreateSubscriptionResponse> CreateInvoicedSubscription(SubscriptionOrder order, string hostURL) 
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            CreateSubscriptionResponse response = new CreateSubscriptionResponse();

            if (!string.IsNullOrEmpty(order.CorpSubscriptionID))
            {
                // We don't actually create the corporate subscription here - we send out an email so the corp admin can approve.
                await SendCorpSubscriptionApprovalEmail(order.CorpSubscriptionID, order.UserID, hostURL);
                return response;
            }

            Stripe.SubscriptionCreateOptions options = new Stripe.SubscriptionCreateOptions();
            Stripe.SubscriptionService stripeSubService = new Stripe.SubscriptionService(stripeClient);
            int trialPeriodDays = GetTrialPeriodDays(order);
            options.Customer = order.CustomerID;
            // Do not auto-charge customers account if the cost of the subscription is greater than zero.
            options.CollectionMethod = order.SubscriptionPlan.Cost > 0 ? "send_invoice" : "charge_automatically";
            
            if(options.CollectionMethod == "send_invoice")
                options.DaysUntilDue = 1; // Can not be zero. Can only be set if CollectionMethod is "send_invoice"
            
            options.TrialPeriodDays = trialPeriodDays;
            options.Items = new List<SubscriptionItemOptions>(2);
            options.Items.Add(new SubscriptionItemOptions { Price = order.PaymentProviderPlanID, Quantity = 1 });
            options.CancelAtPeriodEnd = false;
            
            try
            {
                Stripe.Subscription sub = await stripeSubService.CreateAsync(options);
            }
            catch (Exception ex)
            {
                response.ErrorMessage = "Subscription creation failed.  Please try again later.";
                Log.Error("Error creating subscription. UserID: {u}, Ex: {e} ", order.UserID, ex.ToString());
            }

            if(string.IsNullOrEmpty(response.ErrorMessage))
                Log.Information("CreateInvoicedSubscription: An invoiced subscription was created for user {userid}.  The plan is {plan}.", order.UserID, order.PaymentProviderPlanID);

            return response;
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

        public async Task<AsyncResult<string>> ManageSubscriptions(string customerID, string hostUrl)
        {
            AsyncResult<string> result = new AsyncResult<string>();
            Customer customer = await new CustomerService(stripeClient).GetAsync(customerID, new CustomerGetOptions {Expand = new List<string> { "subscriptions" } } );

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

            // Check to see if we are logging in under a corporate login.  
            // When logging in as a corporate login, Azure BillingID field contains UserID of
            // a corporate admin.  We then use the email address of that admin to get billing info.

            string subscriberEmail = userEmail;
            UserRecord userRecord = await graphService.GetUserByEmailAddress(userEmail);

            if (userRecord != null)
            {
                response.BillingID = userRecord.BillingID;
                
                if (!string.IsNullOrEmpty(response.BillingID)) // BillingID will be set if user is a corp user
                {
                    UserRecord adminRecord = await graphService.GetUserRecordByID(response.BillingID);

                    if (adminRecord == null)
                        throw new Exception($"Could not load admin user record with ID {response.BillingID}.  The referencing user ID is {userRecord.User.Id}");

                    subscriberEmail = adminRecord.EMailAddress;  // use the email address of the admin to look up subscription in stripe.
                }
            }
             

            // Create a customer if one does not exist.  Every email address in Azure should have a corresponding customer in Stripe.
            // In dev customer accounts get zapped so we need to recreate them here.
            Customer customer = await CreateCustomer(subscriberEmail);

            if (customer == null)
                return response;

            response.CustomerID = customer.Id;

            if (customer.Subscriptions?.Any() ?? false)
            {
                response.SubscriptionCount = customer.Subscriptions.Count(); 
                response.SubscriptionID = customer.Subscriptions.FirstOrDefault(x => x.Status == "active" || x.Status == "trialing")?.Items?.FirstOrDefault()?.Plan?.Id;
            }
            return response;
        }

        public async Task<CorpSubscriptionInfoResponse> GetCorpSubscriptionInfo(string corpAdminUserID)
        {
            if (string.IsNullOrEmpty(corpAdminUserID))
                throw new ArgumentNullException(corpAdminUserID);
            
            UserRecord user = await graphService.GetUserRecordByID(corpAdminUserID);

            if (user == null || ! user.IsCorporateAdmin)
                return null;

            CorpSubscriptionInfoResponse response = new CorpSubscriptionInfoResponse();
            response.AdminEmail = user.EMailAddress;
            Customer customer = await GetCustomerByEmailAddress(user.EMailAddress);

            if (customer == null)
                return response;

            List<Model.Subscription> subs = await GetSubscriptionsForCustomer(customer);
            

            if (subs?.Any() ?? false)
            {
                Model.Subscription sub = subs.FirstOrDefault(x => x.IsActive);

                if (sub != null)
                    response.SubscriptionPlan = GetSubscriptionPlans().FirstOrDefault(x => x.PaymentProviderPlanID == sub.PaymentProviderPlanID);
            }
            return response;
        }

        public async Task SendCorpSubscriptionApprovalEmail(string adminID, string subscriberID, string hostURL)
        {
            if (string.IsNullOrEmpty(adminID))
                throw new ArgumentNullException(nameof(adminID));
            if(string.IsNullOrEmpty(subscriberID))
                throw new ArgumentNullException(nameof(subscriberID));

            UserRecord record = await graphService.GetUserRecordByID(subscriberID);
            UserRecord adminRecord = await graphService.GetUserRecordByID(adminID);

            if (record == null)
                throw new Exception($"User (subscriber) with ID {subscriberID} was not found.");

            if (adminRecord == null)
                throw new Exception($"User (admin) with ID {adminID} was not found.");


            StringBuilder sb = new StringBuilder();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LeaderAnalytics.Vyntix.Web.Services.SubApprovalEmailTemplate.html"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    sb.Append(reader.ReadToEnd());
                }
            }

            if (sb.Length == 0)
                throw new Exception("Error retrieving email template.");

            sb.Replace("%USER_NAME%", record.User.DisplayName);
            sb.Replace("%USER_EMAIL%", record.EMailAddress);
            sb.Replace("%URL%", hostURL);
            sb.Replace("%ADMIN_ID%", adminID); // This is titled "Corporate Subscription ID" in the UI.
            sb.Replace("%SUB_ID%", subscriberID);
            string emailContent = sb.ToString();
            
            EmailMessage email = new EmailMessage
            {
                To = new string[] { adminRecord.EMailAddress, "leaderanalytics@outlook.com" },
                From = "leaderanalytics@outlook.com",
                Subject = "Request for Vyntix Login Credentials",
                Msg = emailContent
            };

            var apiResult = await apiClient.PostAsync("api/Message/SendMessage", new StringContent(JsonSerializer.Serialize(email), Encoding.UTF8, "application/json"));
            Log.Information("CreateInvoicedSubscription: A Corporate Subscription was created for User {userid}. The BillingID is {corpSubscriptionID}.", subscriberID, adminID);
        }

        public async Task CreateDelegateSubscription(string adminID, string subscriberID)
        {
            // Todo:  validate admin and user.
            UserRecord record = await graphService.GetUserRecordByID(subscriberID);
            record.BillingID = adminID;
            await graphService.UpdateUser(record);
            Log.Information("CreateDelegateSubscription: A Corporate Delegate Subscription was created for User {userID}. The BillingID is {corpSubscriptionID}.", subscriberID, adminID);
        }

        public int GetTrialPeriodDays(SubscriptionOrder order) => (!string.IsNullOrEmpty(order.CorpSubscriptionID)) || (order.PriorSubscriptions?.Any() ?? false) || (order.SubscriptionPlan.Cost == 0) ? 0 : 30;

        public bool IsPrepaymentRequired(SubscriptionOrder order) => string.IsNullOrEmpty(order.CorpSubscriptionID) && GetTrialPeriodDays(order) == 0 && order.SubscriptionPlan.Cost > 0;
    }
}
