using LeaderAnalytics.Vyntix.Web.Model;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using System.Text.Json;
using LeaderAnalytics.Core;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Net.Http;
using LeaderAnalytics.Core.Azure;
using LeaderAnalytics.Vyntix.Web.Domain;
using System.Net.Mail;

namespace LeaderAnalytics.Vyntix.Web.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private IGraphService graphService;
        private Stripe.SubscriptionService stripeSubscriptionService;
        private Stripe.CustomerService stripeCustomerService;
        private Stripe.BillingPortal.SessionService stripeSessionService;
        private SessionCache sessionCache;
        private string subscriptionFile;
        private List<SubscriptionPlan> subscriptionPlans;
        private static HttpClient apiClient;
        private IActionContextAccessor accessor;
        private EMailClient emailClient;

        public SubscriptionService(AzureADConfig config, IActionContextAccessor accessor, IGraphService graphService, Stripe.SubscriptionService stSubService, Stripe.CustomerService stCustomerService, Stripe.BillingPortal.SessionService stSessionService,  SessionCache sessionCache, SubscriptionFilePathParameter subscriptionFilePath, EMailClient eMailClient)
        {
            this.accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            this.graphService = graphService;
            this.stripeSubscriptionService = stSubService;
            this.stripeCustomerService = stCustomerService;
            this.stripeSessionService = stSessionService;
            this.sessionCache = sessionCache;
            this.subscriptionFile = subscriptionFilePath?.Value ?? throw new ArgumentNullException("subscriptionFilePath");
            this.emailClient = eMailClient;
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
            Customer customer = (await stripeCustomerService.ListAsync(new CustomerListOptions { Email = customerEmail })).FirstOrDefault();

            if (customer != null)
                customer.Subscriptions = await stripeSubscriptionService.ListAsync(new SubscriptionListOptions { Customer = customer.Id, Status = "all" });

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
                CorpSubscriptionValidationResponse corpResponse = await ValidateCorpSubscription(order.CorpSubscriptionID, order.UserID);

                if (!corpResponse.Success)
                {
                    response.ErrorMessage = corpResponse.ErrorMessage;
                    return response;
                }
                plan = corpResponse.SubscriptionPlan;
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
            if (order.SubscriptionPlan.PlanDescription.StartsWith("Free non-business") && order.PriorSubscriptions.Any())
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

            if (order == null)
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
                CustomerEmail = string.IsNullOrEmpty(order.CustomerID) ? order.UserEmail : null,
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
                await SendCorpSubscriptionRequestEmail(order.CorpSubscriptionID, order.UserID, hostURL);
                return response;
            }

            Stripe.SubscriptionCreateOptions options = new Stripe.SubscriptionCreateOptions();
            int trialPeriodDays = GetTrialPeriodDays(order);
            options.Customer = order.CustomerID;
            // Do not auto-charge customers account if the cost of the subscription is greater than zero.
            options.CollectionMethod = order.SubscriptionPlan.Cost > 0 ? "send_invoice" : "charge_automatically";

            if (options.CollectionMethod == "send_invoice")
                options.DaysUntilDue = 1; // Can not be zero. Can only be set if CollectionMethod is "send_invoice"

            options.TrialPeriodDays = trialPeriodDays;
            options.Items = new List<SubscriptionItemOptions>(2);
            options.Items.Add(new SubscriptionItemOptions { Price = order.PaymentProviderPlanID, Quantity = 1 });
            options.CancelAtPeriodEnd = false;

            try
            {
                Stripe.Subscription sub = await stripeSubscriptionService.CreateAsync(options);
            }
            catch (Exception ex)
            {
                response.ErrorMessage = "Subscription creation failed.  Please try again later.";
                Log.Error("Error creating subscription. UserID: {u}, Ex: {e} ", order.UserID, ex.ToString());
            }

            if (string.IsNullOrEmpty(response.ErrorMessage))
                Log.Information("CreateInvoicedSubscription: An invoiced subscription was created for user {userid}.  The plan is {plan}.", order.UserID, order.PaymentProviderPlanID);

            return response;
        }

        public async Task<Customer> CreateCustomer(string email)
        {
            Customer customer = await GetCustomerByEmailAddress(email);

            if (customer != null)
                return customer;

            await stripeCustomerService.CreateAsync(new CustomerCreateOptions { Email = email });
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
            Customer customer = await stripeCustomerService.GetAsync(customerID, new CustomerGetOptions { Expand = new List<string> { "subscriptions" } });

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

            try
            {
                var session = stripeSessionService.Create(options);
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

            CorpSubscriptionInfoResponse response = new CorpSubscriptionInfoResponse();

            UserRecord user = await graphService.GetUserRecordByID(corpAdminUserID);

            if (user == null)
            {
                response.ErrorMessage = $"User with ID {corpAdminUserID} was not found.";
                return response;
            }
            else if (!user.IsCorporateAdmin)
            {
                response.ErrorMessage = $"User with ID {corpAdminUserID} is not designated as a corporate admin.";
                return response;
            }

            response.AdminEmail = user.EMailAddress;
            Customer customer = await GetCustomerByEmailAddress(user.EMailAddress);

            if (customer == null)
            {
                response.ErrorMessage = $"A customer record was not found for email address {user.EMailAddress}";
                return response;
            }

            List<Model.Subscription> subs = await GetSubscriptionsForCustomer(customer);


            if (subs?.Any() ?? false)
            {
                Model.Subscription sub = subs.FirstOrDefault(x => x.IsActive);

                if (sub != null)
                {
                    var plan = GetSubscriptionPlans().FirstOrDefault(x => x.PaymentProviderPlanID == sub.PaymentProviderPlanID);

                    if (plan != null)
                        response.SubscriptionPlan = plan;
                    else
                        response.ErrorMessage = $"An active subscription plan was not found for user {corpAdminUserID}";
                }
                else
                    response.ErrorMessage = $"An active subscription plan was not found for user {corpAdminUserID}";
            }
            else
            {
                response.ErrorMessage = $"No subscription plans found for user {corpAdminUserID}";
            }
            response.Success = string.IsNullOrEmpty(response.ErrorMessage);
            return response;
        }

        public async Task<CorpSubscriptionValidationResponse> ValidateCorpSubscription(string corpAdminUserID, string subscriberUserID)
        {
            if (string.IsNullOrEmpty(corpAdminUserID))
                throw new ArgumentNullException(nameof(corpAdminUserID));
            if (string.IsNullOrEmpty(subscriberUserID))
                throw new ArgumentNullException(nameof(subscriberUserID));

            CorpSubscriptionValidationResponse response = new CorpSubscriptionValidationResponse();
            CorpSubscriptionInfoResponse infoResponse = await GetCorpSubscriptionInfo(corpAdminUserID);

            if (!infoResponse.Success)
            {
                response.ErrorMessage = infoResponse.ErrorMessage;
                return response;
            }

            UserRecord subscriber = await graphService.GetUserRecordByID(subscriberUserID);

            if (subscriber == null)
            {
                response.ErrorMessage = $"User with subscriberUserID {subscriberUserID} was not found.";
                return response;
            }
            response.SubscriberEmail = subscriber.EMailAddress;
            response.AdminEmail = infoResponse.AdminEmail;
            response.SubscriptionPlan = infoResponse.SubscriptionPlan;

            if (response.AdminEmail.Split('@')[1]?.ToLower() != response.SubscriberEmail.Split('@')[1]?.ToLower())
            {
                response.ErrorMessage = "Subscriber email domain must match email domain of corporate admin.";
                return response;
            }

            response.Success = string.IsNullOrEmpty(response.ErrorMessage);
            return response;
        }

        public async Task SendCorpSubscriptionRequestEmail(string adminID, string subscriberID, string hostURL)
        {
            // https://www.campaignmonitor.com/dev-resources/guides/coding-html-emails/
            // https://stackoverflow.com/questions/32825779/gmail-not-showing-inline-images-cid-im-sending-with-system-net-mail
            // We can't serialize MailMessage so we have to inject and use SMTP client here 


            if (string.IsNullOrEmpty(adminID))
                throw new ArgumentNullException(nameof(adminID));
            if (string.IsNullOrEmpty(subscriberID))
                throw new ArgumentNullException(nameof(subscriberID));

            UserRecord record = await graphService.GetUserRecordByID(subscriberID);
            UserRecord adminRecord = await graphService.GetUserRecordByID(adminID);

            if (record == null)
                throw new Exception($"User (subscriber) with ID {subscriberID} was not found.");

            if (adminRecord == null)
                throw new Exception($"User (admin) with ID {adminID} was not found.");

            StringBuilder sb = new StringBuilder();
            // Build Action property of file SubApprovalEmailTemplate.html must be "Embedded Resource"
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LeaderAnalytics.Vyntix.Web.StaticHTML.CorpSubRequestEmailTemplate.html"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    sb.Append(reader.ReadToEnd());
                }
            }

            if (sb.Length == 0)
                throw new Exception("Error retrieving email template.");

            string responseUrl = $"{hostURL}/Subscription/CorpCredentialAction?a={adminID}&u={subscriberID}";

            sb.Replace("%USER_NAME%", record.User.DisplayName);
            sb.Replace("%USER_EMAIL%", record.EMailAddress);
            sb.Replace("%URL%", responseUrl);
            sb.Replace("%ADMIN_ID%", adminID); // This is titled "Corporate Subscription ID" in the UI.
            sb.Replace("%SUB_ID%", subscriberID);
            string emailContent = sb.ToString();

            MailMessage mail = new MailMessage();
            mail.BodyEncoding = mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = true;
            mail.To.Add(adminRecord.EMailAddress);
            mail.Bcc.Add("leaderanalytics@outlook.com");
            mail.From = new MailAddress("leaderanalytics@outlook.com");
            mail.Subject = "Vyntix subscription request";
            mail.Body = emailContent;
            AttachLogoImage(mail);
            emailClient.Send(mail);

            Log.Information("SendCorpSubscriptionRequestEmail: A Corporate Subscription was emailed for User {userid}. The BillingID is {corpSubscriptionID}.", subscriberID, adminID);
        }

        private void AttachLogoImage(MailMessage mail)
        {
            LinkedResource image = new LinkedResource("ClientApp\\public\\VyntixLogo.png", "image/png");
            image.ContentId = "VyntixLogo.png@71236720.91827344";
            image.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            image.ContentType.Name = "VyntixLogo.png@71236720.91827344";
            image.ContentLink = new Uri("cid:VyntixLogo.png@71236720.91827344");
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mail.Body, System.Text.Encoding.UTF8, "text/html");
            htmlView.LinkedResources.Add(image);
            htmlView.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable;
            mail.AlternateViews.Add(htmlView);
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

        public async Task<AsyncResult> CreateCorporateSubscription(string adminID, string subscriberID, bool isApproved, string hostURL, bool sendConfirmation = true)
        {
            AsyncResult result = new AsyncResult();
            CorpSubscriptionValidationResponse validationResponse = new CorpSubscriptionValidationResponse();
            string msg = null;

            if (isApproved)
                validationResponse = await ValidateCorpSubscription(adminID, subscriberID);

            if (validationResponse.Success)
            {
                // Create the subscription by updating the BillingID field on the MSGraph User table. Thats it.
                UserRecord user = await graphService.GetUserRecordByID(subscriberID);
                user.BillingID = adminID;
                await graphService.UpdateUser(user);
                Log.Information("Corporate subscription was created for subscriber {u}, adminID is {a},", subscriberID, adminID);
            }
            else if (isApproved)
            {
                // The request was approved but validation failed.  Let the user know.
                msg = $"Your request for login credentials was approved, however the subscription validation process failed.";

                if (!string.IsNullOrEmpty(validationResponse.ErrorMessage))
                    msg += $"  The error message is: {validationResponse.ErrorMessage}";

                Log.Error("Corporate subscription was approved however validation failed.  Error message is: {e}", msg);
            }

            if (sendConfirmation && !string.IsNullOrEmpty(validationResponse.SubscriberEmail))
                SendCorpSubscriptionNotice(validationResponse.SubscriberEmail, isApproved, msg, hostURL);

            result.Success = true;
            return result;
        }


        public void SendCorpSubscriptionNotice(string subscriberEmail, bool isApproved, string msg2, string hostURL)
        {
            StringBuilder sb = new StringBuilder();
            // Build Action property of file SubApprovalEmailTemplate.html must be "Embedded Resource"
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LeaderAnalytics.Vyntix.Web.StaticHTML.CorpSubNoticeEmailTemplate.html"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    sb.Append(reader.ReadToEnd());
                }
            }

            if (sb.Length == 0)
                throw new Exception("Error retrieving email template.");

            string msg1 = "";

            if (isApproved)
                msg1 = $"Your request to access a corporate Vyntix subscription has been approved.  You can login <a href='{hostURL}' target='_blank'>here</a> to begin using the subscription.";
            else
                msg1 = "Your request to access a corporate Vyntix subscription has been declined.  Please contact your company administrator for more information.";

            sb.Replace("%MSG1%", msg1);
            sb.Replace("%MSG2%", msg2);
            string emailContent = sb.ToString();

            MailMessage mail = new MailMessage();
            mail.BodyEncoding = mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = true;
            mail.To.Add(subscriberEmail);
            mail.Bcc.Add("leaderanalytics@outlook.com");
            mail.From = new MailAddress("leaderanalytics@outlook.com");
            mail.Subject = "Vyntix subscription request";
            mail.Body = emailContent;
            AttachLogoImage(mail);
            emailClient.Send(mail);
        }
    }
}
