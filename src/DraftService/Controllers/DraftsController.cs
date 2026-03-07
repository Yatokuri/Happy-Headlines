using DraftService.Contracts;
using DraftService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DraftService.Controllers;

[ApiController]
[Route("drafts")]
public class DraftsController(IDraftService draftService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateDraft(
        [FromBody] CreateDraftRequest request,
        CancellationToken cancellationToken)
    {
        var result = await draftService.CreateDraftAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetDraftById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDraftById(Guid id, CancellationToken cancellationToken)
    {
        var result = await draftService.GetDraftByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("publisher/{publisherId}")]
    public async Task<IActionResult> GetDraftByPublisherId(string publisherId, CancellationToken cancellationToken)
    {
        var result = await draftService.GetDraftByPublisherIdAsync(publisherId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateDraft(
        Guid id,
        [FromBody] UpdateDraftRequest request,
        CancellationToken cancellationToken)
    {
        var result = await draftService.UpdateDraftAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDraft(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await draftService.DeleteDraftAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}