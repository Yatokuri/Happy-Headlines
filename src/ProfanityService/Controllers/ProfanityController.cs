using Microsoft.AspNetCore.Mvc;
using ProfanityService.Contracts;
using ProfanityService.Services;

namespace ProfanityService.Controllers;

[ApiController]
[Route("profanity")]
public class ProfanityController(IProfanityService profanityService) : ControllerBase
{
    [HttpPost("check")]
    public async Task<IActionResult> Check([FromBody] CheckProfanityRequest request, CancellationToken cancellationToken)
    {
        var result = await profanityService.CheckTextAsync(request.Text, cancellationToken);
        return Ok(result);
    }

    [HttpPost("words")]
    public async Task<IActionResult> AddWord([FromBody] CreateProfanityWordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await profanityService.AddWordAsync(request.Word, cancellationToken);
            return CreatedAtAction(nameof(GetWords), new { }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("words")]
    public async Task<IActionResult> GetWords(CancellationToken cancellationToken)
    {
        var result = await profanityService.GetWordsAsync(cancellationToken);
        return Ok(result);
    }
}