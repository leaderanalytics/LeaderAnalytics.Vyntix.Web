using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using System.IO;
using LeaderAnalytics.Vyntix.Web.Model;
using LeaderAnalytics.Vyntix.Web.Services;
using System.Threading.Tasks;
using Microsoft.Graph;

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
        public async Task GetUserByEmailTest()
        {

            string userID = "sam.wheat@outlook.com";
            User user = await graphService.GetUserByEmailAddress2(userID);
            Assert.IsNotNull(user);
        }
    }
}