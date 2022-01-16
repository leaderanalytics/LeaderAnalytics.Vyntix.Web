namespace LeaderAnalytics.Vyntix.Web.Model;

public class CreateSubscriptionResponse : Domain.AsyncResult
{
    public string SessionID { get; set; }
    public string SubscriptionID { get; set; }
}
