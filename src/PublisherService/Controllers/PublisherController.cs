using Microsoft.AspNetCore.Mvc;
using PublisherService.Contracts;
using PublisherService.Services;

namespace PublisherService.Controllers;

[ApiController]
[Route("publisher")]
public class PublisherController(IPublisherService publisherService) : ControllerBase
{
    [HttpPost("publish")]
    public async Task<IActionResult> Publish([FromBody] PublishArticleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await publisherService.PublishAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { message = ex.Message });
        }
    }
}