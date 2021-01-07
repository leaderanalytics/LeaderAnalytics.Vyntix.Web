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

namespace LeaderAnalytics.Vyntix.Web.Tests
{
    [TestClass]
    public class Tests : BaseTest
    {
        // Can not save unmodified user record:
        // https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/868
        // https://github.com/stripe/stripe-dotnet/issues/2270
        private GraphService graphService;

        [TestInitialize]
        public void Setup()
        {
            graphService = new GraphService(GetGraphCredentials());
            
        }

        [TestMethod]
        public async Task VerifyUsersTest()
        {
            string userID = "7aee9e27-4767-4479-a9f3-a1816e3a2b24";
            userID = "9188040d-6c67-4c5b-b112-36a304b66dad";
            bool isValid = await graphService.VerifyUser(userID);
            Assert.IsTrue(isValid);
        }


        [TestMethod]
        public async Task GetUserByIDTest()
        {
            string id = "761f6057-d861-4433-8462-fdec3eb0de95";
            id = "fe184e4e-0c7d-494f-9b07-fc3bd51eacba";
            UserRecord record = (await graphService.GetUserRecordByID(id));
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
        public async Task UpdateUserTest()
        {
            string userID = "3f81ce01-ad56-4b79-899a-994b48a97c98";
            User user = (await graphService.FindUser(userID)).First();
            UserRecord record = new UserRecord(user);
            record.BillingID = null;
            record.IsBanned = false;
            record.SuspendedUntil = null;
            record.IsCorporateAdmin = true;
            string x = record.EMailAddress;
            await graphService.UpdateUser(record);
            record = await graphService.GetUserRecordByID(userID);
        }


        [TestMethod]
        public async Task UpdateUserTest2()
        {
            string userID = "a00afd9b-af51-4206-bd9f-2621343bc70d";
            User user = (await graphService.FindUser(userID)).First();
            await graphService.UpdateUser(user);
        }



        [TestMethod]
        public async Task DeleteUserTest()
        {


        }


      

        

        [TestMethod]
        public async Task CreateUserTest()
        {
            // MUST use this EXACT syntax.  See https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/850
            string myDomain = "LeaderAnalytics.onmicrosoft.com";

            var user = new User { DisplayName = "John Smith" };
            user.Identities = new List<ObjectIdentity>()
            {
                    new ObjectIdentity
                    {
                        SignInType = "emailAddress",
                        Issuer = myDomain,
                        IssuerAssignedId = "jsmith@yahoo.com"
                    }
            };

            user.PasswordProfile = new PasswordProfile
            {
                Password = "ABC12345!!",
                ForceChangePasswordNextSignIn = false
            };
            user.PasswordPolicies = "DisablePasswordExpiration";
            user.AccountEnabled = true;
            User addedUser = await graphService.CreateUser(user); // Passes
        }

        [TestMethod]
        public async Task CreateUserTest2()
        {
            string myDomain = "LeaderAnalytics.onmicrosoft.com";

            var user = new User { DisplayName = "John Smith" };
            user.Identities = new List<ObjectIdentity>();

            user.Identities = user.Identities.Append(
                    new ObjectIdentity
                    {
                        SignInType = "emailAddress",
                        Issuer = myDomain,
                        IssuerAssignedId = "jsmith@yahoo.com"
                    });

            user.PasswordProfile = new PasswordProfile
            {
                Password = "ABC12345!!",
                ForceChangePasswordNextSignIn = false
            };
            user.PasswordPolicies = "DisablePasswordExpiration";
            user.AccountEnabled = true;
            User addedUser = await graphService.CreateUser(user); // Passes
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
            // https://old.reddit.com/r/dotnet/comments/jud07a/why_a_two_hour_job_in_azure_took_two_days_and_i/

            // This method results in "Internal server error".
            // See thse two links: 
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

      
    }


    
}