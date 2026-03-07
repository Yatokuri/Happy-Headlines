using CommentService.Contracts;
using CommentService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommentService.Controllers;

[ApiController]
[Route("comments")]
public class CommentsController(ICommentService commentService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await commentService.CreateCommentAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetCommentByArticleId), new { articleId = result.ArticleId }, result);
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

    [HttpGet("article/{articleId}")]
    public async Task<IActionResult> GetCommentByArticleId(string articleId, CancellationToken cancellationToken)
    {
        var result = await commentService.GetCommentByArticleIdAsync(articleId, cancellationToken);
        return Ok(result);
    }
}