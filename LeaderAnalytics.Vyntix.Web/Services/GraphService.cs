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

namespace LeaderAnalytics.Vyntix.Web.Services
{

    // https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/755
    // https://docs.microsoft.com/en-us/answers/questions/65589/how-to-look-up-a-user-by-personal-email-address.html
    // https://docs.microsoft.com/en-us/answers/questions/76716/request-for-documenatation-how-is-user-record-popu.html
    // https://docs.microsoft.com/en-us/power-bi/admin/service-admin-alternate-email-address-for-power-bi
    
    
    public class GraphService
    {
        private GraphServiceClient client;
        private const string _endpoint = "https://graph.microsoft.com/beta";
        private string _accessToken;
        

        public GraphService(GraphClientCredentials credentials)
        {
            this.client = GetGraphServiceClient(credentials ?? throw new ArgumentNullException("credentials"));
            _accessToken = GetToken(credentials).GetAwaiter().GetResult();
        
        }

        public async Task<List<User>> FindUser(string id)
        {
            var page = client.Users.Request()
                 .Select($"id, mail, identities, othermails, {UserAttributes.BillingID}, {UserAttributes.IsBanned}, {UserAttributes.IsCorporateAdmin}, {UserAttributes.IsOptIn}, {UserAttributes.PaymentProviderCustomerID}, {UserAttributes.SuspendedUntil}");

            if (!string.IsNullOrEmpty(id))
                page = page.Filter($"id eq '{id}'");

            List<User> users =(await page.GetAsync()).ToList();
            return users;
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
            User user = (await FindUser(id)).FirstOrDefault();

            if (user != null)
                record = new UserRecord(user);

            return record;
        }

        public async Task UpdateUser(UserRecord user)
        {
            User graphUser = user.ToGraphUser();
            
            var result = await client.Users[graphUser.Id]
            .Request()
            .UpdateAsync(graphUser);
            
        }

        public async Task<User> GetUserByEmailAddress2(string email)
        {
            IGraphServiceUsersCollectionPage users = await client.Users.Request()
                .Filter($"otherMails/any(id:id eq '{email}')")
                // .Filter($"userPrincipalName eq '{email}'") does not work
                .Select(x => new { x.Id, x.OtherMails, x.Identities })
                .GetAsync();

            return users[0];
        }

        public async Task<List<User>> GetUsers()
        {
            // Does not work
            //var users = (await client.Users.Request()
            //    .Select(x => new { x.Id, x.OtherMails, x.Identities })
            //    .Filter($"otherMails/any(id:id eq 'samspam92841@gmail.com') or identities/any(ids:ids/issuerassignedid eq 'samspam92841@gmail.com')")
            //    .GetAsync());

            // Read all users into memory
            IGraphServiceUsersCollectionPage users = await client.Users.Request()
                .Select(x => new { x.Id, x.OtherMails, x.Identities, x.Mail })
                .GetAsync();

            // find a user
            var user = users.First(x => x.Identities.Any(y => y.IssuerAssignedId == "samspam92841@gmail.com"));
            
            // set extension property to a dummy value
            user.AdditionalData = new Dictionary<string, object> { { UserAttributes.BillingID, "Test" } };
            
            // updateResponse is null
            var updateResponse = await client.Users[user.Id].Request().UpdateAsync(user);

            // Read users a second time
            users = await client.Users.Request()
                .Select($"id, identities, othermails, {UserAttributes.BillingID}")
                .GetAsync();

            // find the user we just updated
            user = users.First(x => x.Identities.Any(y => y.IssuerAssignedId == "samspam92841@gmail.com"));
            
            // Nope.  No data.  This throws.
            var field = user.AdditionalData[UserAttributes.BillingID];

            return users.ToList();
        }

        private GraphServiceClient GetGraphServiceClient(GraphClientCredentials credentials)
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

            var tokenResult = ccab.AcquireTokenForClient(new List<string> { "https://graph.microsoft.com/.default" });
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
