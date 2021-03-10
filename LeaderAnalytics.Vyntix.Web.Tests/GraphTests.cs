using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using System.IO;
using LeaderAnalytics.Vyntix.Web.Model;
using LeaderAnalytics.Vyntix.Web.Services;
using System.Threading.Tasks;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Identity.Client;
using Microsoft.Graph.Auth;
using System.Linq;
using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LeaderAnalytics.Vyntix.Web.Domain;
using Autofac;

namespace LeaderAnalytics.Vyntix.Web.Tests
{
    [TestClass]
    public class GraphTests : BaseTest
    {
        // Can not save unmodified user record:
        // https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/868
        // https://github.com/stripe/stripe-dotnet/issues/2270
        private IGraphService graphService;
        private User user1;
        
        public GraphTests()
        {
            Setup();
            graphService = Container.Resolve<IGraphService>();
            user1 = (graphService.GetAllUsers().Result.First());
        }

        [TestMethod]
        public async Task VerifyUsersTest()
        {
            bool isValid = await graphService.VerifyUser(user1.Id);
            Assert.IsTrue(isValid);
        }


        [TestMethod]
        public async Task GetUserByIDTest()
        {
            UserRecord record = (await graphService.GetUserRecordByID(user1.Id));
            Assert.IsNotNull(record);
        }


        [TestMethod]
        public async Task GetUserByEmailAddressTest()
        {
            string email = "samspam92841@gmail.com";
            UserRecord record = await graphService.GetUserByEmailAddress(email);
            Assert.IsNotNull(record);
        }


        [TestMethod]
        public async Task GetAllUsersTest()
        {
            List<User> users = await graphService.FindUser(null);
            Assert.IsNotNull(users);
        }


        [TestMethod]
        public async Task UpdateUserTest_federated_user()
        {
            // https://stackoverflow.com/questions/61051028/how-to-update-identities-collection-for-existing-b2c-user-using-microsoft-graph

            string userID = "a880b5ac-d3cc-4e7c-89a1-123b1ad3bdc5";
            UserRecord userRecord = await graphService.GetUserRecordByID(userID);
            User user = userRecord.User;
            var users = (await graphService.GetAllUsers()).ToList();
            
            if (user == null)
                return;


            // Construct an authentication request
            HttpClient httpClient = new HttpClient();
            string tenantID = "f9087985-90e5-479d-9edf-9ef5ce5b4123";
            string clientID = "6814437e-2d8a-4953-b73e-063330d31dfc";     // client ID of Manangement App for Microsoft Graph
            string clientSecret = "facx5m_32vRLGFPSP-6KQP7T~oJY.44Rj_";
            string authContent = $"grant_type=client_credentials&client_id={clientID}&client_secret={clientSecret}&scope=https%3A%2F%2Fgraph.microsoft.com%2F.default";
            string api_version = "api-version=1.6";

            HttpRequestMessage authRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://login.microsoftonline.com/{tenantID}/oauth2/token"),
                Content = new StringContent(authContent, Encoding.UTF8, "application/x-www-form-urlencoded")
            };

            // Send the auth request and extract the token
            HttpResponseMessage authResponse = await httpClient.SendAsync(authRequest);
            string json = await authResponse.Content.ReadAsStringAsync();
            var jobject = (JObject)JsonConvert.DeserializeObject(json);
            string token = ((JValue)jobject["access_token"]).ToString();
            Assert.IsNotNull(token);


            // Get a user
            string resource_path = "users/" + user.Id;
            string apiUrl = $"https://graph.windows.net/LeaderAnalytics.onmicrosoft.com/{resource_path}?{api_version}";
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(apiUrl)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.SendAsync(request);
            json = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("OK", response.StatusCode.ToString());

            // Construct a request to update a user
            
            resource_path = "users/" + user.Id;
            string requestContent =  "{" + $"\"userIdentities\"" + ":[" + JsonConvert.SerializeObject(new 
            {
                issuer = "LeaderAnalytics.com",
                issuerUserId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()))
            
            }) + "]}"; 
            apiUrl = $"https://graph.windows.net/LeaderAnalytics.onmicrosoft.com/{resource_path}?{api_version}";
            request = new HttpRequestMessage
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(apiUrl),
                Content = new StringContent(requestContent, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Send the update request
            response = await httpClient.SendAsync(request);
            json = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("NoContent", response.StatusCode.ToString());
        }


        private async Task UpdateUser(string userID)
        {
            User user = (await graphService.FindUser(userID)).First();
            await graphService.UpdateUser(user); // No change, no error.

            if (user.AdditionalData == null)
                user.AdditionalData = new Dictionary<string, object>();

            // Set a user attribute and an extended attribute and update
            // both at the same time
            user.DisplayName = "Bob";
            user.AdditionalData[UserAttributes.BillingID] = "ABC";
            await graphService.UpdateUser(user);
            // Get the user again
            user = (await graphService.FindUser(userID)).First();
            // Verify the change
            Assert.IsNotNull(user.AdditionalData);
            Assert.AreEqual("Bob", user.DisplayName);
            Assert.AreEqual("ABC", user.AdditionalData[UserAttributes.BillingID]);

            // Set a user attribute and an extended attribute and update
            // both at the same time
            user.AdditionalData[UserAttributes.BillingID] = null;
            user.DisplayName = "Sam";
            await graphService.UpdateUser(user);
            // Get the user again
            user = (await graphService.FindUser(userID)).First();
            // Verify the change
            Assert.IsNull(user.AdditionalData); // Nothing to retrieve
            Assert.AreEqual("Sam", user.DisplayName);
        }

        [TestMethod]
        public async Task DeleteUserTest()
        {


        }

        [TestMethod]
        public async Task CreateUserTest()
        {
            string myDomain = "LeaderAnalytics.onmicrosoft.com";
            string emailAddress = "jsmith@yahoo.com";
            UserRecord userRecord = await graphService.GetUserByEmailAddress(emailAddress);

            if (userRecord != null)
                await graphService.DeleteUser(userRecord.User.Id);

            var user = new User { DisplayName = "John Smith" };
            user.Identities = new List<ObjectIdentity>()
            {
                    new ObjectIdentity
                    {
                        SignInType = "emailAddress",
                        Issuer = myDomain,
                        IssuerAssignedId = emailAddress
                    }
            };

            user.PasswordProfile = new PasswordProfile
            {
                Password = "ABC12345!!",
                ForceChangePasswordNextSignIn = false
            };
            user.PasswordPolicies = "DisablePasswordExpiration";
            user.AccountEnabled = true;
            User addedUser = await graphService.CreateUser(user);
            Assert.IsNotNull(addedUser);
            await graphService.DeleteUser(addedUser.Id);
        }

        [TestMethod]
        public async Task GetDelegatedUsersTest()
        {
            string userID1 = "";
            object userOwner1 = "";
            string userID2 = "";
            object userOwner2 = "";

            List<User> users = await graphService.GetAllUsers();
            
            if (users.Count < 2)
                return;

            userID1 = users[0].Id;
            users[0].AdditionalData?.TryGetValue(UserAttributes.BillingID, out userOwner1);

            userID2 = users[1].Id;
            users[1].AdditionalData?.TryGetValue(UserAttributes.BillingID, out userOwner2);
        }


        [TestMethod]
        public async Task CreateUserSchemaExtension() 
        {


            return;


            // https://old.reddit.com/r/dotnet/comments/jud07a/why_a_two_hour_job_in_azure_took_two_days_and_i/

            // This method results in "Internal server error".
            // See these two links: 
            // links to SO issue below https://github.com/microsoftgraph/microsoft-graph-docs/issues/6647
            // internal server error https://stackoverflow.com/questions/59395423/graph-api-adding-schema-extension-using-net-core-3-1


            //This page https://docs.microsoft.com/en-us/azure/active-directory-b2c/user-profile-attributes
            // links to this one:  https://docs.microsoft.com/en-us/graph/extensibility-overview#schema-extensions

            // https://docs.microsoft.com/en-us/graph/extensibility-schema-groups
            // get data:  https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/238


            IConfidentialClientApplication clientApplication = ConfidentialClientApplicationBuilder
                .Create(GraphClientCredentials.ClientID)
                .WithTenantId(GraphClientCredentials.TenantID)
                .WithClientSecret(GraphClientCredentials.ClientSecret)
                .Build();
            ClientCredentialProvider provider = new ClientCredentialProvider(clientApplication);
            GraphServiceClient graphServiceClient = new GraphServiceClient(provider);


            var domains = await graphServiceClient.Domains.Request().GetAsync();
            var vyntixDomain = domains.First(x => x.Id == "vyntix.com");


            if (!vyntixDomain.IsVerified.Value)
            {
                var verifyDNSResponse = await graphServiceClient.Domains["vyntix.com"]
                    .VerificationDnsRecords
                    .Request()
                    .GetAsync();

                var verifyResponse = await graphServiceClient.Domains["vyntix.com"]
                .Verify()
                .Request()
                .PostAsync();

                if (!verifyResponse.IsVerified.Value)
                    return; // Find out why it is not verified.
            }

            var schemaExtension = new SchemaExtension
            {
                Id = "vyntix_subscriberInfo",
                Description = "Billing and configuration properties for users",
                TargetTypes = new List<string> { "User" },
                Properties = new List<ExtensionSchemaProperty>
                {
                    new ExtensionSchemaProperty
                    {
                        Name = "BillingID",
                        Type = "String"
                    },

                    new ExtensionSchemaProperty
                    {
                        Name = "IsCorporateAdmin",
                        Type = "Boolean"
                    },

                    new ExtensionSchemaProperty
                    {
                        Name = "IsOptIn",
                        Type = "Boolean"
                    },

                    new ExtensionSchemaProperty
                    {
                        Name = "DisciplinaryAction", // S = Suspended, B = Banned
                        Type = "String"
                    },
                }
            };

            try
            {
                // https://docs.microsoft.com/en-us/graph/api/application-post-extensionproperty?view=graph-rest-1.0&tabs=csharp

                // this is wrong.  see above.
                var response = await graphServiceClient.SchemaExtensions
                    .Request()
                    .AddAsync(schemaExtension);
            }
            catch (Exception ex)
            {
                string y = ex.ToString();
            }
        }


        [TestMethod]
        public async Task SetAdminFlagTest()
        {
            // This test sets the IsCorporateAdmin flag for a federated user (IsLocalAccount = false)
            // and a non-federated user (IsLocalAccount = true).
            List<User> users = await graphService.GetAllUsers();
            string federatedUserID = users.FirstOrDefault(x => x.CreationType == "LocalAccount")?.Id;
            string nonFederatedUserID = users.FirstOrDefault(x => x.CreationType != "LocalAccount")?.Id;
            Assert.IsNotNull(federatedUserID);
            Assert.IsNotNull(nonFederatedUserID);
            UserRecord[] userRecords = new UserRecord[2] { await graphService.GetUserRecordByID(federatedUserID), await graphService.GetUserRecordByID(nonFederatedUserID) };
            
            foreach (UserRecord userRecord in userRecords)
            {
                bool originalSetting = userRecord.IsCorporateAdmin;
                bool newSetting = !originalSetting;
                await graphService.SetAdminFlag(userRecord, newSetting);
                await Task.Delay(100); // Test will intermittently fail if we don't give the server a chance to catch up
                UserRecord updatedUser = await graphService.GetUserRecordByID(userRecord.User.Id);
                Assert.AreEqual(newSetting, updatedUser.IsCorporateAdmin, $"UserID: {updatedUser.User.Id}");
                await graphService.SetAdminFlag(userRecord, originalSetting);
            }
        }


        [TestMethod]
        public async Task UpdateUserExtendedPropertyUsingAzureADGraphAPI()
        {
            // This test uses the deprecated Azure AD Graph API
            // https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-graph-api
            // https://docs.microsoft.com/en-us/previous-versions/azure/ad/graph/howto/azure-ad-graph-api-directory-schema-extensions
            // https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/868
            // https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/413

            // Construct an authentication request
            HttpClient httpClient = new HttpClient();
            string tenantID = "f9087985-90e5-479d-9edf-9ef5ce5b4123";
            string clientID = "6814437e-2d8a-4953-b73e-063330d31dfc";     // client ID of Manangement App for Microsoft Graph
            string clientSecret = "facx5m_32vRLGFPSP-6KQP7T~oJY.44Rj_";
            string authContent = $"grant_type=client_credentials&client_id={clientID}&client_secret={clientSecret}&scope=https%3A%2F%2Fgraph.microsoft.com%2F.default";
            HttpRequestMessage authRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://login.microsoftonline.com/{tenantID}/oauth2/token"),
                Content = new StringContent(authContent, Encoding.UTF8, "application/x-www-form-urlencoded")
            };

            // Send the auth request and extract the token
            HttpResponseMessage authResponse = await httpClient.SendAsync(authRequest);
            string json = await authResponse.Content.ReadAsStringAsync();
            var jobject = (JObject)JsonConvert.DeserializeObject(json);
            string token = ((JValue)jobject["access_token"]).ToString();
            Assert.IsNotNull(token);

            // Construct a request to get a user
            string userID = "a880b5ac-d3cc-4e7c-89a1-123b1ad3bdc5"; // A federated user
            string resource_path = "users/" + userID;
            string apiUrl = $"https://graph.windows.net/LeaderAnalytics.onmicrosoft.com/{resource_path}?api-version=1.6";
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(apiUrl)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.SendAsync(request);
            json = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("OK", response.StatusCode.ToString());


            // Construct a request to update a user
            string api_version = "api-version=1.6";
            string requestContent = "{" + $"\"{UserAttributes.IsCorporateAdmin}\"" + ":" + "true" + "}";
            apiUrl = $"https://graph.windows.net/LeaderAnalytics.onmicrosoft.com/{resource_path}?api-version=1.6";
            request = new HttpRequestMessage
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(apiUrl),
                Content = new StringContent(requestContent, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Send the update request
            response = await httpClient.SendAsync(request);
            json = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("NoContent", response.StatusCode.ToString()); 

            // Get the user again make sure it was updated.
            apiUrl = $"https://graph.windows.net/LeaderAnalytics.onmicrosoft.com/{resource_path}?api-version=1.6";
            request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(apiUrl)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            response = await httpClient.SendAsync(request);
            json = await response.Content.ReadAsStringAsync(); 
        }
    }
}