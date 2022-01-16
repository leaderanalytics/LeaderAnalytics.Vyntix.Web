namespace LeaderAnalytics.Vyntix.Web.Model;

public class OrderSession
{
    public DateTime TimeStamp { get; private set; }
    public string SessionID { get; private set; }
    public SubscriptionOrder Order { get; private set; }

    public OrderSession(string sessionID, SubscriptionOrder order)
    {
        SessionID = string.IsNullOrEmpty(sessionID) ? throw new ArgumentNullException("sessionID") : sessionID;
        Order = order ?? throw new ArgumentNullException("order");
        TimeStamp = DateTime.UtcNow;
    }
}
