using System.Net;

namespace LeaderAnalytics.Vyntix.Web.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class EMailController : Controller
{
    private HttpClient apiClient;
    private IActionContextAccessor accessor;


    public EMailController(HttpClient apiClient, IActionContextAccessor accessor)
    {
        this.accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }


    [HttpPost]
    public async Task<IActionResult> SendContactRequest(LeaderAnalytics.Vyntix.Web.Model.ContactRequest msg)
    {
        IActionResult result = null;
        msg.IP_Address = accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();

        try
        {
            var apiResult = await apiClient.PostAsync("api/Message/SendContactRequest", new StringContent(System.Text.Json.JsonSerializer.Serialize(msg), Encoding.UTF8, "application/json"));

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

    [HttpPost]
    public async Task<IActionResult> SendInternalMessage(LeaderAnalytics.Vyntix.Web.Model.ContactRequest msg)
    {
        IActionResult result = null;
        string ipaddress  = accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();
        msg.Msg += Environment.NewLine + "IP: " + ipaddress;
        EmailMessage email = new EmailMessage { From = "DoNotReply@LeaderAnalytics.com", To = ["leaderanalytics@outlook.com", "sam.wheat@outlook.com"], IsHTML = false, Msg = msg.Msg, Subject = "Internal Message from LeaderAnalytics.Vyntix.com" };
        var apiResult = await apiClient.PostAsync("api/Message/SendEmailMessage", new StringContent(System.Text.Json.JsonSerializer.Serialize(email), Encoding.UTF8, "application/json"));
        if (apiResult.StatusCode == System.Net.HttpStatusCode.Created)
            result = CreatedAtAction("SendInternalMessage", "email") as IActionResult;
        else
        {
            string errorMsg = await apiResult.Content.ReadAsStringAsync();
            result = BadRequest(errorMsg);
            Log.Error("Failed to send Internal message.  The response from the API server is: {@e}", apiResult);
        }
        return result;
    }


        [HttpGet]
    public async Task<IActionResult> CaptchaImage()
    {
        string ipaddress = accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();
        try
        {

            //var apiResult = await apiClient.GetStreamAsync(QueryHelpers.AddQueryString("api/Captcha/CaptchaImage", "ipaddress", ipaddress));
            //return new FileStreamResult(apiResult, "image/jpeg");

            Uri url = new Uri($"{apiClient.BaseAddress.ToString().TrimEnd('/')}/{QueryHelpers.AddQueryString("/api/Captcha/CaptchaImage", "ipaddress", ipaddress).TrimStart('/')}");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            var apiResponse = await apiClient.SendAsync(request);
            
            if (apiResponse.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Error calling Captcha API.  Url is {request.RequestUri}, Response is {apiResponse.StatusCode}.  Content is {apiResponse.Content}");
            
            return new FileStreamResult(apiResponse.Content.ReadAsStream(), "image/jpeg");
        }
        catch (Exception ex)
        {
            Log.Error("CaptchaImage: {e}", ex.ToString());
            return BadRequest("Failed to generate Captcha Image.");
        }
    }
}
