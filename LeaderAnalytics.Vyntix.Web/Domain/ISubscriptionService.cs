namespace LeaderAnalytics.Vyntix.Web.Domain;

public interface ISubscriptionService
{
    Task<CreateSubscriptionResponse> ApproveSubscriptionOrder(SubscriptionOrder order);
    Task<string> ConfirmSubscription(string sessionID);
    Task<AsyncResult> AllocateCorporateSubscription(string adminID, string subscriberID, bool isApproved, string hostURL, bool sendConfirmation = true);
    Task<Customer> CreateCustomer(string email);
    Task CreateDelegateSubscription(string adminID, string subscriberID);
    Task<CreateSubscriptionResponse> CreateSubscription(SubscriptionOrder order, string hostURL);
    Task<bool> DeleteCustomer(string customerID);
    Task ExtendSubscription(List<string> customerIDs, int daysToExtend);
    List<SubscriptionPlan> GetActiveSubscriptionPlans();
    Task<CorpSubscriptionInfoResponse> GetCorpSubscriptionInfo(string corpAdminUserID);
    Task<Customer> GetCustomerByEmailAddress(string customerEmail);
    Task<SubscriptionInfoResponse> GetSubscriptionInfo(string userEmail);
    List<SubscriptionPlan> GetSubscriptionPlans();
    Task<List<Model.Subscription>> GetSubscriptionsForCustomer(Customer customer);
    int GetTrialPeriodDays(SubscriptionOrder order);
    bool IsPrepaymentRequired(SubscriptionOrder order);
    Task<AsyncResult<string>> ManageSubscriptions(string customerID, string hostUrl);
    void SendCorpSubscriptionNotice(string subscriberEmail, bool isApproved, string msg2, string hostURL);
    Task SendCorpSubscriptionRequestEmail(string adminID, string subscriberID, string hostURL);
    Task<CorpSubscriptionValidationResponse> ValidateCorpSubscription(string corpAdminUserID, string subscriberUserID, bool isApproved);
}
