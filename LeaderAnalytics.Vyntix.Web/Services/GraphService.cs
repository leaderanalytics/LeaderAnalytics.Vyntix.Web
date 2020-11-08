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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeaderAnalytics.Vyntix.Web.Services
{

    // https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/755
    // https://docs.microsoft.com/en-us/answers/questions/65589/how-to-look-up-a-user-by-personal-email-address.html
    // https://docs.microsoft.com/en-us/answers/questions/76716/request-for-documenatation-how-is-user-record-popu.html
    // https://docs.microsoft.com/en-us/power-bi/admin/service-admin-alternate-email-address-for-power-bi
    public class GraphService
    {
        private GraphClientCredentials credentials;
        private const string _endpoint = "https://graph.microsoft.com/beta";
        private string _accessToken;

        public GraphService(GraphClientCredentials credentials)
        {
            this.credentials = credentials ?? throw new ArgumentNullException("credentials");
            _accessToken = GetToken().GetAwaiter().GetResult();
        }


        public async Task<bool> VerifyUser(string userID)
        {
            GraphServiceClient client = GetGraphServiceClient();
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


        public async Task<bool> GetUserByEmailAddress(string email)
        {
            GraphServiceClient client = GetGraphServiceClient();
            IGraphServiceUsersCollectionPage users = await client.Users.Request().Filter($"userPrincipalName eq '{email}'").GetAsync();
            IGraphServiceUsersCollectionPage users2 = await client.Users.Request().Filter($"mail eq '{email}'").GetAsync();
            IGraphServiceUsersCollectionPage users3 = await client.Users.Request().Filter($"signInNames/any(x:x/value eq '{email}')").GetAsync();
            return users.Count > 0;
        }


        public async Task<User> GetUserByEmailAddress2(string email)
        {
            GraphServiceClient client = GetGraphServiceClient();
            IGraphServiceUsersCollectionPage users = await client.Users.Request()
                .Filter($"otherMails/any(id:id eq '{email}')")
                // .Filter($"userPrincipalName eq '{email}'") does not work
                .Select(x => new { x.Id, x.OtherMails, x.Identities })
                .GetAsync();

            return users[0];
        }




        public async Task<List<User>> GetUsers()
        {
            List<User> users = await JsonSerializer.DeserializeAsync<List<User>>(await GetFromGraph("/users"));
            return users;
        }


        private GraphServiceClient GetGraphServiceClient()
        {
            var ccab = ConfidentialClientApplicationBuilder
                .Create(credentials.ClientID)
                .WithClientSecret(credentials.ClientSecret)
                .WithTenantId(credentials.TenantID)
                .Build();

            GraphServiceClient client = new GraphServiceClient(new ClientCredentialProvider(ccab));
            return client;

        }


        private async Task<string> GetToken()
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

                using (var client = new HttpClient())
                using (var response = await client.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                        return await response.Content.ReadAsStreamAsync();

                    return Stream.Null;
                }
            }
        }
    }
}
