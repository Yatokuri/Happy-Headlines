using Microsoft.AspNetCore.Mvc;
using SubscriberService.Contracts;
using SubscriberService.FeatureFlags;
using SubscriberService.Services;

namespace SubscriberService.Controllers;

[ApiController]
[Route("subscribers")]
public class SubscribersController(
    ISubscriberService subscriberService,
    IFeatureToggleService featureToggleService) : ControllerBase
{
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request, CancellationToken cancellationToken)
    {
        if (!featureToggleService.IsSubscriberServiceEnabled())
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "SubscriberService is disabled.");

        var result = await subscriberService.SubscribeAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("unsubscribe/{email}")]
    public async Task<IActionResult> Unsubscribe(string email, CancellationToken cancellationToken)
    {
        if (!featureToggleService.IsSubscriberServiceEnabled())
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "SubscriberService is disabled.");

        var deleted = await subscriberService.UnsubscribeAsync(email, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        if (!featureToggleService.IsSubscriberServiceEnabled())
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "SubscriberService is disabled.");

        var result = await subscriberService.GetAllAsync(cancellationToken);
        return Ok(result);
    }
}