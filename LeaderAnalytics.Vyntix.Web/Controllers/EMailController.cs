using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Serilog;
using LeaderAnalytics.Core.Azure;
using LeaderAnalytics.Vyntix.Web.Model;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography.X509Certificates;

namespace LeaderAnalytics.Vyntix.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class EMailController : Controller
    {
        private static HttpClient apiClient;
        private IActionContextAccessor accessor;
        private AzureADConfig config;

        public EMailController(AzureADConfig config, IActionContextAccessor accessor)
        {
            this.config = config;
            this.accessor = accessor;

            if (apiClient == null)
            {
                ClientCredentialsHelper helper = new ClientCredentialsHelper(config);
                apiClient = helper.AuthorizedClient();
            }
        }


        [HttpPost]
        public async Task<IActionResult> SendEMail(EmailMsg msg)
        {
            IActionResult result = null;
            msg.IP_Address = accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();        
            
            try
            {
                var apiResult = await apiClient.PostAsync("api/Message/SendEmail", new StringContent(JsonSerializer.Serialize(msg), Encoding.UTF8, "application/json"));

                if (apiResult.StatusCode == System.Net.HttpStatusCode.Created)
                    result = CreatedAtAction("SendEMail", "email") as IActionResult;
                else
                {
                    string errorMsg = await apiResult.Content.ReadAsStringAsync();
                    result = BadRequest(errorMsg);
                    Log.Error("Failed to send contact email.  The response from the API server is: {@e}", apiResult);
                }
            }
            catch (Exception ex)
            {
                result = BadRequest("Failed to send contact email.");
                Log.Error("An exception occurred when trying to send contact email.  The error is: {e}", ex.ToString());
            }
            return result;
        }


        [HttpGet]
        public async Task<IActionResult> CaptchaImage()
        {
            string ipaddress = accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();
            var apiResult = await apiClient.GetStreamAsync(QueryHelpers.AddQueryString("api/Message/CaptchaImage","ipaddress", ipaddress));
            return new FileStreamResult(apiResult, "image/jpeg");
        }
    }
}
