using LeaderAnalytics.Vyntix.Web.Model;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using System.IO;
using LeaderAnalytics.Caching;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Text;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using LeaderAnalytics.Vyntix.Web.Domain;

namespace LeaderAnalytics.Vyntix.Web.Services
{
    // user by email query bug:
    // https://github.com/microsoftgraph/microsoft-graph-docs/issues/11094

    public class GraphService : IGraphService
    {
        private GraphServiceClient client;
        private const string _endpoint = "https://graph.microsoft.com/beta";
        private const string _ADendpoint = "https://graph.windows.net/LeaderAnalytics.onmicrosoft.com/users?api-version=1.6";
        private string _accessToken;
        private string USER_FIELDS = $"id, userprincipalname, mail, displayname, mailnickname, accountenabled, identities, othermails, {UserAttributes.BillingID}, {UserAttributes.IsBanned}, {UserAttributes.IsCorporateAdmin}, {UserAttributes.IsOptIn}, {UserAttributes.PaymentProviderCustomerID}, {UserAttributes.SuspendedUntil}";

        public GraphService(GraphClientCredentials credentials)
        {
            this.client = CreateGraphServiceClient(credentials ?? throw new ArgumentNullException("credentials"));
            _accessToken = GetToken(credentials).GetAwaiter().GetResult();
        }

        public async Task<List<User>> FindUser(string id)
        {
            var page = client.Users.Request()
                 .Select(USER_FIELDS);

            if (!string.IsNullOrEmpty(id))
                page = page.Filter($"id eq '{id}'");

            List<User> users = null;
            try
            {
                users = (await page.GetAsync())?.ToList();
            }
            catch (Microsoft.Graph.ServiceException)
            {

            }
            return users;
        }

        public async Task<List<User>> GetAllUsers()
        {
            return (await client.Users.Request().Select(x => new
            {
                x.Id,
                // x.ODataType, Parsing OData Select and Expand failed: Term '@odata.type' is not valid in a $select or $expand expression.
                x.AdditionalData,
                x.TransitiveMemberOf,
                x.ScopedRoleMemberOf,
                x.RegisteredDevices,
                x.OwnedObjects,
                x.OwnedDevices,
                x.Oauth2PermissionGrants,
                x.MemberOf,
                x.Manager,
                x.LicenseDetails,
                x.DirectReports,
                //x.Skills,
                //x.Schools,
                //x.Responsibilities,
                //x.PreferredName,
                //x.Interests,
                //x.HireDate,
                //x.Birthday,
                //x.AboutMe,

                //x.DeviceEnrollmentLimit,
                //x.Activities,
                x.Settings,
                //x.DeviceManagementTroubleshootingEvents,
                //x.ManagedAppRegistrations,
                //x.ManagedDevices,
                x.Extensions,
                //x.FollowedSites,
                //x.Photos,
                x.Photo,
                //x.People,
                //x.InferenceClassification,
                x.UserType,
                //x.UsageLocation,
                x.LastPasswordChangeDateTime,

                x.JobTitle,
                x.IsResourceAccount,
                x.ImAddresses,
                x.Identities,
                x.GivenName,
                x.FaxNumber,
                x.ExternalUserStateChangeDateTime,
                x.ExternalUserState,
                x.EmployeeId,
                x.DisplayName,
                x.Department,
                x.CreationType,
                x.CreatedDateTime,
                x.Country,
                x.ConsentProvidedForMinor,
                x.CompanyName,
                x.City,
                x.BusinessPhones,
                x.AssignedPlans,
                x.AssignedLicenses,
                x.AgeGroup,
                x.AccountEnabled,
                x.LegalAgeGroupClassification,
                x.LicenseAssignmentStates,
                x.MailNickname,
                x.Surname,
                x.StreetAddress,
                x.State,
                x.SignInSessionsValidFromDateTime,
                x.ProxyAddresses,
                x.ProvisionedPlans,
                x.PreferredLanguage,
                x.PostalCode,
                x.PasswordProfile,
                x.PasswordPolicies,
                x.UserPrincipalName,
                x.OtherMails,
                x.OnPremisesSyncEnabled,
                x.OnPremisesSecurityIdentifier,
                x.OnPremisesSamAccountName,
                x.OnPremisesProvisioningErrors,
                x.OnPremisesLastSyncDateTime,
                x.OnPremisesImmutableId,
                x.OnPremisesExtensionAttributes,
                x.OnPremisesDomainName,
                x.OnPremisesDistinguishedName,
                x.OfficeLocation,
                x.MobilePhone,
                x.OnPremisesUserPrincipalName
            }

            ).GetAsync()).ToList();
        }

        public async Task<bool> VerifyUser(string userID)
        {
            User user;
            try
            {
                user = await client.Users[userID].Request().GetAsync();
            }
            catch (Microsoft.Graph.ServiceException)
            {
                return false;
            }
            return user != null;
        }

        public async Task<UserRecord> GetUserRecordByID(string id)
        {
            UserRecord record = null;
            User user = (await FindUser(id))?.FirstOrDefault();

            if (user != null)
                record = new UserRecord(user);

            return record;
        }

        public async Task UpdateUser(UserRecord user)
        {
            User graphUser = user.User;
            await UpdateUser(graphUser);
        }

        public async Task UpdateUser(User user)
        {
            var result = await client.Users[user.Id]
            .Request()
            .UpdateAsync(user);
        }


        public async Task<User> CreateUser(UserRecord userRecord)
        {
            User user = userRecord?.User ?? throw new ArgumentNullException("userRecord");
            return await CreateUser(user);
        }

        public async Task<User> CreateUser(User user)
        {
            var result = await client.Users.Request().AddAsync(user);
            return result;
        }

        public async Task DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new Exception("id is required.");

            await client.Users[id].Request().DeleteAsync();
        }

        public async Task<UserRecord> GetUserByEmailAddress(string email)
        {
            IGraphServiceUsersCollectionPage users = await client.Users.Request()
                .Select(USER_FIELDS)
                .Filter($"identities/any(ids:ids/issuerassignedid eq '{email}' and ids/issuer eq 'x')")
                .GetAsync();

            if (users.Any())
                return new UserRecord(users[0]);

            return null;
        }

        public async Task<List<UserRecord>> GetDelegateUsers(string ownerID)
        {
            List<User> users = await GetAllUsers();
            List<UserRecord> records = new List<UserRecord>(users.Count);

            foreach (User user in users.Where(x => x.AdditionalData != null))
            {
                if (user.AdditionalData.TryGetValue(UserAttributes.BillingID, out object userOwner) && userOwner.ToString() == ownerID)
                    records.Add(new UserRecord(user));
            }

            return records;
        }

        private GraphServiceClient CreateGraphServiceClient(GraphClientCredentials credentials)
        {
            var ccab = ConfidentialClientApplicationBuilder
                .Create(credentials.ClientID)
                .WithClientSecret(credentials.ClientSecret)
                .WithTenantId(credentials.TenantID)
                .Build();

            GraphServiceClient client = new GraphServiceClient(new ClientCredentialProvider(ccab));
            return client;
        }

        private async Task<string> GetToken(GraphClientCredentials credentials)
        {
            var ccab = ConfidentialClientApplicationBuilder
                .Create(credentials.ClientID)
                .WithClientSecret(credentials.ClientSecret)
                .WithTenantId(credentials.TenantID)
                .Build();

            //var tokenResult = ccab.AcquireTokenForClient(new List<string> { "https://graph.microsoft.com/.default" });
            var tokenResult = ccab.AcquireTokenForClient(new List<string> { "https://graph.windows.net/.default" });
            var token = await tokenResult.ExecuteAsync();
            return token.AccessToken;
        }

        private async Task<Stream> GetFromGraph(string action)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, _endpoint + action))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                var client = new HttpClient();
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var stuff = await response.Content.ReadAsStreamAsync();
                    return stuff;
                }
                return Stream.Null;
            }
        }
    }
}
