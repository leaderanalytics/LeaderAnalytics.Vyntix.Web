﻿namespace LeaderAnalytics.Vyntix.Web.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SubscriptionController : Controller
{
    private LeaderAnalytics.Vyntix.Web.Services.SubscriptionService subscriptionService;
    private string Host => $"{this.Request.Scheme}://{this.Request.Host}";

    public SubscriptionController(LeaderAnalytics.Vyntix.Web.Services.SubscriptionService subscriptionService)
    {
        this.subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    }

    [HttpGet]
    public ActionResult<string> Identity()
    {
        return "Leader Analytics Vyntix Web API";
    }

    [HttpGet]
    public JsonResult GetActiveSubscriptionPlans()
    {
        return Json(subscriptionService.GetActiveSubscriptionPlans());
    }

    [HttpPost]
    public async Task<JsonResult> IsPrepaymentRequired(SubscriptionOrder order)
    {
        CreateSubscriptionResponse response = await subscriptionService.ApproveSubscriptionOrder(order);

        if (string.IsNullOrEmpty(response.ErrorMessage))
            return Json(subscriptionService.IsPrepaymentRequired(order));

        return Json(null);
    }

    [HttpPost]
    public async Task<ActionResult> CreateSubscription(SubscriptionOrder order)
    {
        try
        {
            CreateSubscriptionResponse response = await subscriptionService.CreateSubscription(order, Host);

            if (string.IsNullOrEmpty(response.ErrorMessage))
                return Ok(response);
            else
                return StatusCode(300, response);
        }
        catch (Exception ex)
        {
            string s = ex.ToString();
            Log.Error(s);
            return StatusCode(500, s);
        }
    }

    // This method is called by the payment processor after the user has submitted payment.
    [HttpGet]
    public async Task<ActionResult> ConfirmSubscription()
    {
        string sessionID = Request.Query["session_id"];
        string errorMsg = await subscriptionService.ConfirmSubscription(sessionID);
        return RedirectToStatus(string.IsNullOrEmpty(errorMsg));
    }

    private ActionResult RedirectToStatus(bool success)
    {
        string redirect = $"{Host}/SubActivation" + (success ? "Success" : "Failure");
        return new RedirectResult(redirect, false);
    }

    [HttpPost]
    public async Task<ActionResult> ManageSubscription([FromBody] string customerID)
    {
        AsyncResult<string> result = await subscriptionService.ManageSubscriptions(customerID, Host);

        if (!result.Success)
            return BadRequest(System.Text.Json.JsonSerializer.Serialize(result.ErrorMessage));
        else
            return new JsonResult(result.Result);
    }

    [HttpPost]
    public async Task<ActionResult<SubscriptionInfoResponse>> GetSubscriptionInfo([FromBody] string userEmail)
    {
        SubscriptionInfoResponse response = await subscriptionService.GetSubscriptionInfo(userEmail);
        return response;
    }

    [HttpPost]
    public async Task<ActionResult<CorpSubscriptionInfoResponse>> GetCorpSubscriptionInfo([FromBody] string userID)
    {
        CorpSubscriptionInfoResponse response = await subscriptionService.GetCorpSubscriptionInfo(userID);
        return response;
    }

    [HttpPost]
    public void LogInfo([FromBody] string msg)
    {
        var request = Request;
        Log.Information(msg);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="adminID">Admin user ID</param>
    /// <param name="subscriberID">Subscriber user ID</param>
    /// <param name="isApproved">IsApproved flag</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult> CorpSubAllocation(CorpSubscriptionAllocationRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        AsyncResult result = await subscriptionService.AllocateCorporateSubscription(request.AdminID, request.SubscriberID, request.IsApproved, Host);
        return new JsonResult(result);
    }
}
