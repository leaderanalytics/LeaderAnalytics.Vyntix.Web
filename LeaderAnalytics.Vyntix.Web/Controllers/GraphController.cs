using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Web.Model;
using LeaderAnalytics.Vyntix.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LeaderAnalytics.Vyntix.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class GraphController : ControllerBase
    {
        private GraphService graphService;

        public GraphController(GraphService graphService)
        {
            this.graphService = graphService ?? throw new ArgumentNullException("graphService");
        }

        [HttpGet]
        public async Task<bool> VerifyUser(string userID) => await graphService.VerifyUser(userID);
        
        [HttpGet]
        public async Task<UserRecord> GetUserRecord(string id) => await graphService.GetUserRecordByID(id);
    }
}
