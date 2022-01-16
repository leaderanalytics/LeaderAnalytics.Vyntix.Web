namespace LeaderAnalytics.Vyntix.Web.Model;

public class CorpSubscriptionInfoResponse : AsyncResult
{
    public string AdminEmail { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; }
}
