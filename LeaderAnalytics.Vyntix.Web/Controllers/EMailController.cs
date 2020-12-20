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
        

        public EMailController(AzureADConfig config, IActionContextAccessor accessor)
        {
            this.accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));

            if (apiClient == null)
            {
                ClientCredentialsHelper helper = new ClientCredentialsHelper(config);
                apiClient = helper.AuthorizedClient();
            }
        }


        [HttpPost]
        public async Task<IActionResult> SendContactRequest(ContactRequest msg)
        {
            IActionResult result = null;
            msg.IP_Address = accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();        
            
            try
            {
                var apiResult = await apiClient.PostAsync("api/Message/SendContactRequest", new StringContent(JsonSerializer.Serialize(msg), Encoding.UTF8, "application/json"));

                if (apiResult.StatusCode == System.Net.HttpStatusCode.Created)
                    result = CreatedAtAction("SendContactRequest", "email") as IActionResult;
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
            try
            {
                
                var apiResult = await apiClient.GetStreamAsync(QueryHelpers.AddQueryString("api/Captcha/CaptchaImage", "ipaddress", ipaddress));
                return new FileStreamResult(apiResult, "image/jpeg");
            }
            catch (Exception ex)
            {
                Log.Error("CaptchaImage: {e}", ex.ToString());
                return BadRequest("Failed to generate Captcha Image.");
            }
        }
    }
}
