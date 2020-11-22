using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
namespace LeaderAnalytics.Vyntix.Web.Model
{
    public class UserRecord // Named to not conflict with Graph.User
    {
        public string ID { get; set; }
        public string Email { get; set; }
        public string BillingID { get; set; }
        public bool IsBanned { get; set; }
        public bool IsCorporateAdmin { get; set; }
        public bool IsOptIn { get; set; }
        public string PaymentProviderCustomerID { get; set; }
        public DateTime? SuspendedUntil { get; set; } // UTC

        public UserRecord()
        { 
        
        }

        public UserRecord(User user)
        {
            ID = user.Id;
            Email = user.Mail;

            if (user.AdditionalData == null)
                return;
            
            object obj = null;

            if (user.AdditionalData.TryGetValue(UserAttributes.BillingID, out obj))
                BillingID = obj.ToString();
            if (user.AdditionalData.TryGetValue(UserAttributes.IsBanned, out obj))
                IsBanned = Convert.ToBoolean(obj);
            if (user.AdditionalData.TryGetValue(UserAttributes.IsCorporateAdmin, out obj))
                IsCorporateAdmin = Convert.ToBoolean(obj);
            if (user.AdditionalData.TryGetValue(UserAttributes.IsOptIn, out obj))
                IsOptIn = Convert.ToBoolean(obj);
            if (user.AdditionalData.TryGetValue(UserAttributes.PaymentProviderCustomerID, out obj))
                PaymentProviderCustomerID = obj.ToString();
            if (user.AdditionalData.TryGetValue(UserAttributes.SuspendedUntil, out obj))
                SuspendedUntil = obj == null ? (DateTime?)null  : Convert.ToDateTime(obj.ToString().Substring(1)).ToUniversalTime();

        }

        public User ToGraphUser()
        {
            return new User
            {
                Id = ID,
                Mail = Email,
                AdditionalData = new Dictionary<string, object> {
                    { UserAttributes.BillingID,BillingID},
                    { UserAttributes.IsBanned, IsBanned },
                    { UserAttributes.IsCorporateAdmin, IsCorporateAdmin},
                    { UserAttributes.IsOptIn, IsOptIn },
                    { UserAttributes.PaymentProviderCustomerID, PaymentProviderCustomerID },
                    { UserAttributes.SuspendedUntil, SuspendedUntil.HasValue? "z" + SuspendedUntil.Value.ToString("o") : null }
                }
            };
        }
    }
}
