using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Model
{
    public static class UserAttributes
    {
        // The guid in the prefix column is the client ID of an application named "b2c-extensions-app. Do not modify. Used by AADB2C for storing user data"
        // This app is in the b2c directory.  The dashes have been removed from the client ID.  Underscore is added before and after client ID.
        
        private const string prefix = "extension_b0d39858f7814b429dcf4e52d243296a_";

        public const string BillingID                   = prefix + "BillingID";
        public const string IsBanned                    = prefix + "IsBanned";
        public const string IsCorporateAdmin            = prefix + "IsCorporateAdmin";
        public const string IsOptIn                     = prefix + "IsOptIn";
        public const string PaymentProviderCustomerID   = prefix + "PaymentProviderCustomerID";
        public const string SuspendedUntil              = prefix + "SuspendedUntil";
    }
}
