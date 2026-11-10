
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Domain.Articles;
using Microsoft.AspNetCore.Mvc;

namespace FujitNotes.Api.Controllers;

[ApiController]
[Route("api/articles")]
public sealed class ArticlesController : ControllerBase
{
    private readonly CreateArticleUseCase _createArticle;
    private readonly GetArticleUseCase _getArticle;
    private readonly PublishArticleUseCase _publishArticle;

    public ArticlesController(
        CreateArticleUseCase createArticle,
        GetArticleUseCase getArticle,
        PublishArticleUseCase publishArticle)
    {
        _createArticle = createArticle;
        _getArticle = getArticle;
        _publishArticle = publishArticle;
    }

    // POST /api/articles
    [HttpPost]
    public async Task<ActionResult<CreateArticleResponse>> Create(
        [FromBody] CreateArticleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await _createArticle.ExecuteAsync(
                request.Title,
                request.Content,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id },
                new CreateArticleResponse(id));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/articles/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArticleResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var article = await _getArticle.ExecuteAsync(
            id,
            cancellationToken);

        if (article is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(article));
    }

    // POST /api/articles/{id}/publish
    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var found = await _publishArticle.ExecuteAsync(
                id,
                cancellationToken);

            if (!found)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // DomainモデルをAPIレスポンスに変換
    private static ArticleResponse ToResponse(Article article)
    {
        return new ArticleResponse(
            article.Id,
            article.Title.Value,
            article.Content,
            article.Status.ToString(),
            article.CreatedAt,
            article.PublishedAt);
    }
}

// APIの入力モデル
public sealed record CreateArticleRequest(
    string Title,
    string Content);

// 記事作成時のレスポンス
public sealed record CreateArticleResponse(
    Guid Id);

// 記事取得時のレスポンス
public sealed record ArticleResponse(
    Guid Id,
    string Title,
    string Content,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt);
