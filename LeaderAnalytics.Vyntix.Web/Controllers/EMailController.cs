﻿namespace LeaderAnalytics.Vyntix.Web.Controllers;

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
