
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Domain.Articles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FujitNotes.Api.Controllers;

// 記事管理用API
// 管理者ロールを持つ認証済みユーザーのみ利用可能
[ApiController]
[Authorize(Roles = "Admin")]
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
    // カテゴリ・タグを指定した記事作成にも対応
    [HttpPost]
    public async Task<ActionResult<CreateArticleResponse>> Create(
        [FromBody] CreateArticleRequest request,
        CancellationToken cancellationToken)
    {
        // metadataを指定した場合はtagsを必須とする
        if (request.Metadata is { Tags: null })
        {
            return BadRequest(new
            {
                message = "metadataを指定する場合、tagsが必要です。"
            });
        }

        try
        {
            Guid id;

            if (request.Metadata is null)
            {
                // 従来の記事作成
                id = await _createArticle.ExecuteAsync(
                    request.Title,
                    request.Content,
                    cancellationToken);
            }
            else
            {
                // カテゴリ・タグ付きの記事作成
                id = await _createArticle.ExecuteAsync(
                    request.Title,
                    request.Content,
                    request.Metadata.Category,
                    request.Metadata.Tags!,
                    cancellationToken);
            }

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
    // 管理者向け記事一覧（下書き・公開済みを含む）
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
    // 管理者向け記事詳細
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
        // metadataを指定した場合はtagsを必須とする
        if (request.Metadata is { Tags: null })
        {
            return BadRequest(new
            {
                message = "metadataを指定する場合、tagsが必要です。"
            });
        }

        try
        {
            bool found;

            if (request.Metadata is null)
            {
                // 従来の更新
                // カテゴリ・タグは維持する
                found = await _updateArticle.ExecuteAsync(
                    id,
                    request.Title,
                    request.Content,
                    cancellationToken);
            }
            else
            {
                // カテゴリ・タグも更新する
                found = await _updateArticle.ExecuteAsync(
                    id,
                    request.Title,
                    request.Content,
                    request.Metadata.Category,
                    request.Metadata.Tags!,
                    cancellationToken);
            }

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
    // 記事を公開する
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
    // 記事をアーカイブする
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

    // DomainモデルからAPIレスポンスへ変換
    private static ArticleResponse ToResponse(
        Article article)
    {
        return new ArticleResponse(
            article.Id,
            article.Title.Value,
            article.Content,
            article.Status.ToString(),
            article.CreatedAt,
            article.PublishedAt,
            article.Category?.Value,
            article.Tags
                .Select(tag => tag.Value)
                .ToArray());
    }
}

// 記事作成リクエスト
public sealed record CreateArticleRequest(
    string Title,
    string Content,
    ArticleMetadataRequest? Metadata = null);

// 記事更新リクエスト
public sealed record UpdateArticleRequest(
    string Title,
    string Content,
    ArticleMetadataRequest? Metadata = null);

// カテゴリ・タグの入力
public sealed record ArticleMetadataRequest(
    string? Category,
    IReadOnlyList<string>? Tags);

// 記事作成レスポンス
public sealed record CreateArticleResponse(
    Guid Id);

// 記事取得レスポンス
public sealed record ArticleResponse(
    Guid Id,
    string Title,
    string Content,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt,
    string? Category,
    IReadOnlyList<string> Tags);
