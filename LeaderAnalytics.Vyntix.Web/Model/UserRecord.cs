namespace LeaderAnalytics.Vyntix.Web.Model;

public class UserRecord // Named to not conflict with Graph.User
{
    private string _email;
    private User User { get; set; }


    private string _BillingID;
    public string BillingID
    {
        get
        {
            if (_BillingID is null)
                if (User.AdditionalData.TryGetValue(UserAttributes.BillingID, out object obj))
                    _BillingID = ((JsonElement)obj).Deserialize<string>();
            
            return _BillingID;
        }
        set
        {
            _BillingID = value;
            User.AdditionalData[UserAttributes.BillingID] = JsonSerializer.SerializeToElement(value);
        }
    }

    private bool? _IsBanned;
    public bool IsBanned
    {
        get
        {
            if (_IsBanned is null)
                if (User.AdditionalData.TryGetValue(UserAttributes.IsBanned, out object obj))
                    _IsBanned = ((JsonElement)obj).Deserialize<bool>();
            
            return _IsBanned.HasValue ? _IsBanned.Value : false;
        }

        set
        {
            _IsBanned = value;
            User.AdditionalData[UserAttributes.IsBanned] = JsonSerializer.SerializeToElement(value);
        }
    }

    private bool? _IsCorporateAdmin;
    public bool IsCorporateAdmin
    {
        get
        {
            if (_IsCorporateAdmin is null)
                if (User.AdditionalData.TryGetValue(UserAttributes.IsCorporateAdmin, out object obj))
                    _IsCorporateAdmin = ((JsonElement)obj).Deserialize<bool>();

            return _IsCorporateAdmin.HasValue ? _IsCorporateAdmin.Value : false;
        }

        set
        {
            _IsCorporateAdmin = value;
            User.AdditionalData[UserAttributes.IsCorporateAdmin] = JsonSerializer.SerializeToElement(value);
        }
    }

    private bool? _IsOptIn;
    public bool IsOptIn
    {
        get
        {
            if (_IsOptIn is null)
                if (User.AdditionalData.TryGetValue(UserAttributes.IsOptIn, out object obj))
                    _IsOptIn = ((JsonElement)obj).Deserialize<bool>();

            return _IsOptIn.HasValue ? _IsOptIn.Value : false;
        }
        set
        {
            _IsOptIn = value;
            User.AdditionalData[UserAttributes.IsOptIn] = JsonSerializer.SerializeToElement(value);
        }
        
    }

    private string _PaymentProviderCustomerID;
    public string PaymentProviderCustomerID
    {
        get
        {
            if (_PaymentProviderCustomerID is null)
                if (User.AdditionalData.TryGetValue(UserAttributes.PaymentProviderCustomerID, out object obj))
                    _PaymentProviderCustomerID = ((JsonElement)obj).Deserialize<string>();

            return _PaymentProviderCustomerID;
        }
        set
        {
            _PaymentProviderCustomerID = value;
            User.AdditionalData[UserAttributes.PaymentProviderCustomerID] = JsonSerializer.SerializeToElement(value);
        }
    }

    private DateTime? _SuspendedUntil;
    public DateTime? SuspendedUntil
    {
        get
        {
            if(_SuspendedUntil is null)
                if (User.AdditionalData.TryGetValue(UserAttributes.SuspendedUntil, out object obj))
                    _SuspendedUntil = ((JsonElement)obj).Deserialize<DateTime>().ToUniversalTime();

            return _SuspendedUntil;
        }
        set
        {
            _SuspendedUntil = value.HasValue ? value.Value.ToUniversalTime() : null;
            User.AdditionalData[UserAttributes.SuspendedUntil] = JsonSerializer.SerializeToElement(value.HasValue ? "z" + value.Value.ToString("o") : null);
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
        get => User.CreationType == "LocalAccount";
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

    public User GraphUser() => User;
}
