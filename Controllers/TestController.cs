using HealthAidAPI.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

public class TestController : ControllerBase
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public TestController(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    [HttpGet("welcome")]
    public IActionResult GetWelcomeMessage()
    {
        var message = _localizer["WelcomeMessage"];
        return Ok(new { message = message.Value });
    }
}