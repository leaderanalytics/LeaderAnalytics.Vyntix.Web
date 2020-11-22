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
        public async Task GetAllUsersTest()
        {
            List<User> users = await graphService.FindUser(null);
            Assert.IsNotNull(users);
        }

        [TestMethod]
        public async Task UpdateUserTest()
        {
            string userID = "4ca22b1d-5299-4cae-ab35-f23ef0f59343";
            User user = (await graphService.FindUser(userID)).First();
            UserRecord record = new UserRecord(user);
            string guid = Guid.NewGuid().ToString();
            DateTime now = DateTime.UtcNow; 
            record.BillingID = guid;
            record.IsBanned = false;
            record.IsCorporateAdmin = true;
            record.IsOptIn = true;
            record.PaymentProviderCustomerID = guid;
            record.SuspendedUntil = now;
            await graphService.UpdateUser(record);
            record = await graphService.GetUserRecordByID(userID);
            Assert.AreEqual(guid, record.BillingID);
            Assert.AreEqual(now, record.SuspendedUntil);
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