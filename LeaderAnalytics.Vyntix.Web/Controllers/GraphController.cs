using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LeaderAnalytics.Vyntix.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraphController : ControllerBase
    {
        private GraphService graphService;

        public GraphController(GraphService graphService)
        {
            this.graphService = graphService ?? throw new ArgumentNullException("graphService");
        }


        public async Task<bool> VerifyUser(string userID)
        {
            return await graphService.VerifyUser(userID);
        }
    }
}
