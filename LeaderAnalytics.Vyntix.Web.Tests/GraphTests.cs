using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using System.IO;
using LeaderAnalytics.Vyntix.Web.Models;
using LeaderAnalytics.Vyntix.Web.Services;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace LeaderAnalytics.Vyntix.Web.Tests
{
    public class Tests : BaseTest
    {
        private GraphService graphService;

        [SetUp]
        public void Setup()
        {
            graphService = new GraphService(GetGraphCredentials());
        }

        [Test]
        public async Task VerifyUsersTest()
        {
            string userID = "7aee9e27-4767-4479-a9f3-a1816e3a2b24";
            userID = "9e32d9eb-fe41-4ff3-a351-28d47c5f099e";
            bool isValid = await graphService.VerifyUser(userID);
            Assert.IsTrue(isValid);
        }


        [Test]
        public async Task GetUserByEmailTest()
        {

            string userID = "sam.wheat@outlook.com";
            User user = await graphService.GetUserByEmailAddress2(userID);
            Assert.IsNotNull(user);
        }
    }
}