namespace LeaderAnalytics.Vyntix.Web.Model;

public class UserRecord // Named to not conflict with Graph.User
{
    private string _email;
    public User User { get; set; }

    public string BillingID
    {
        get
        {
            if (User.AdditionalData.TryGetValue(UserAttributes.BillingID, out object obj))
                return obj?.ToString();

            return null;
        }
        set { User.AdditionalData[UserAttributes.BillingID] = value; }
    }

    public bool IsBanned
    {
        get
        {
            if (User.AdditionalData.TryGetValue(UserAttributes.IsBanned, out object obj))
                return Convert.ToBoolean(obj);

            return false;
        }

        set { User.AdditionalData[UserAttributes.IsBanned] = value; }
    }

    public bool IsCorporateAdmin
    {
        get
        {
            if (User.AdditionalData.TryGetValue(UserAttributes.IsCorporateAdmin, out object obj))
                return Convert.ToBoolean(obj);

            return false;
        }

        set { User.AdditionalData[UserAttributes.IsCorporateAdmin] = value; }
    }

    public bool IsOptIn
    {
        get
        {
            if (User.AdditionalData.TryGetValue(UserAttributes.IsOptIn, out object obj))
                return Convert.ToBoolean(obj);

            return false;
        }
        set
        {
            User.AdditionalData[UserAttributes.IsOptIn] = value;
        }
    }

    public string PaymentProviderCustomerID
    {
        get
        {
            if (User.AdditionalData.TryGetValue(UserAttributes.PaymentProviderCustomerID, out object obj))
                return obj?.ToString();

            return null;
        }
        set
        {
            User.AdditionalData[UserAttributes.PaymentProviderCustomerID] = value;
        }
    }

    public DateTime? SuspendedUntil
    {
        get
        {
            if (User.AdditionalData.TryGetValue(UserAttributes.SuspendedUntil, out object obj))
                return obj == null ? (DateTime?)null : Convert.ToDateTime(obj.ToString().Substring(1)).ToUniversalTime();

            return null;
        }
        set
        {
            User.AdditionalData[UserAttributes.SuspendedUntil] = value.HasValue ? "z" + value.Value.ToString("o") : null;
        }
    } // UTC

    public string EMailAddress
    {
        get
        {

            if (!string.IsNullOrEmpty(_email))
                return _email;

            if ((User?.Identities ?? null) != null)
                _email = User.Identities.FirstOrDefault(x => x.SignInType == "emailAddress")?.IssuerAssignedId;

            if (string.IsNullOrEmpty(_email))
                _email = User?.OtherMails?.FirstOrDefault();

            return _email;
        }
    }

    public bool IsLocalAccount
    {
        // https://docs.microsoft.com/en-us/graph/api/resources/user?view=graph-rest-1.0
        //
        // creationType String  
        //
        // Indicates whether the user account was created as a regular school or work account(null), 
        // an external account(Invitation), 
        // a local account for an Azure Active Directory B2C tenant(LocalAccount) 
        // or self - service sign - up using email verification (EmailVerified). Read - only.

        // 
        get
        {
            return User.CreationType == "LocalAccount";
        }
    }

    public UserRecord()
    {
        User = new User
        {
            PasswordProfile = new PasswordProfile(),
            Identities = new List<ObjectIdentity>(5),
            OtherMails = new List<string>(5),
            AdditionalData = new Dictionary<string, object>()
        };
    }

    public UserRecord(User user)
    {
        if (user == null)
            throw new ArgumentNullException("user");

        User = user;

        if (User.AdditionalData == null)
            User.AdditionalData = new Dictionary<string, object>();
    }
}
