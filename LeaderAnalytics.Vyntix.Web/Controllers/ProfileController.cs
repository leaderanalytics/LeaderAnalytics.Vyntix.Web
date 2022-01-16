namespace LeaderAnalytics.Vyntix.Web.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class ProfileController : Controller
{
    [Authorize]

    public IActionResult List() //dummy test method
    {
        return Ok();
    }
}
