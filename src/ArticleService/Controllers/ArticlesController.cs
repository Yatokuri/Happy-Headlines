using ArticleService.Contracts;
using ArticleService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArticleService.Controllers;

[ApiController]
[Route("articles")]
public class ArticlesController(IArticleService articleService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateArticle([FromBody] CreateArticleRequest request, CancellationToken cancellationToken)
    {
        var result = await articleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetArticleById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetArticleById(string id, CancellationToken cancellationToken)
    {
        var result = await articleService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArticle(string id, [FromBody] UpdateArticleRequest request, CancellationToken cancellationToken)
    {
        var result = await articleService.UpdateAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArticle(string id, CancellationToken cancellationToken)
    {
        var deleted = await articleService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}