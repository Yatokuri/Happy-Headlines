using Microsoft.AspNetCore.Mvc;
using NewsletterService.Contracts;
using NewsletterService.Services;

namespace NewsletterService.Controllers;

[ApiController]
[Route("newsletter")]
public class NewsletterController(INewsletterService newsletterService) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> Send(
        [FromBody] SendNewsletterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await newsletterService.SendAsync(request, cancellationToken);
        return Ok(result);
    }
}