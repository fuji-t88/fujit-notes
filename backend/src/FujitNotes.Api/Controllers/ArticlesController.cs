
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
    private readonly GetArticlesUseCase _getArticles;
    private readonly PublishArticleUseCase _publishArticle;
    private readonly UpdateArticleUseCase _updateArticle;
    private readonly ArchiveArticleUseCase _archiveArticle;

    public ArticlesController(
        CreateArticleUseCase createArticle,
        GetArticleUseCase getArticle,
        GetArticlesUseCase getArticles,
        PublishArticleUseCase publishArticle,
        UpdateArticleUseCase updateArticle,
        ArchiveArticleUseCase archiveArticle)
    {
        _createArticle = createArticle;
        _getArticle = getArticle;
        _getArticles = getArticles;
        _publishArticle = publishArticle;
        _updateArticle = updateArticle;
        _archiveArticle = archiveArticle;
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

    // GET /api/articles
    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyList<ArticleResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var articles = await _getArticles.ExecuteAsync(
            cancellationToken);

        return Ok(
            articles.Select(ToResponse).ToList());
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

    // PUT /api/articles/{id}
    // 下書きの記事のみ編集可能
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateArticleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var found = await _updateArticle.ExecuteAsync(
                id,
                request.Title,
                request.Content,
                cancellationToken);

            if (!found)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
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

    // POST /api/articles/{id}/archive
    // 下書き・公開済みの記事をアーカイブする
    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var found = await _archiveArticle.ExecuteAsync(
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

    private static ArticleResponse ToResponse(
        Article article)
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

public sealed record CreateArticleRequest(
    string Title,
    string Content);

public sealed record UpdateArticleRequest(
    string Title,
    string Content);

public sealed record CreateArticleResponse(
    Guid Id);

public sealed record ArticleResponse(
    Guid Id,
    string Title,
    string Content,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt);
