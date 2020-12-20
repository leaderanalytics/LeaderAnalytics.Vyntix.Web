using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
namespace LeaderAnalytics.Vyntix.Web.Model
{
    public class UserRecord // Named to not conflict with Graph.User
    {
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
            get {
                
                if ((User?.Identities ?? null) == null)
                    return null;

                string email = User.Identities.FirstOrDefault(x => x.SignInType == "emailAddress")?.IssuerAssignedId;
                return email;
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
}
