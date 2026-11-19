
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Domain.Articles;
using Microsoft.AspNetCore.Mvc;

namespace FujitNotes.Api.Controllers;

[ApiController]
[Route("api/public/articles")]
public sealed class PublicArticlesController : ControllerBase
{
    private readonly GetPublishedArticlesUseCase
        _getPublishedArticles;

    private readonly GetPublishedArticleUseCase
        _getPublishedArticle;

    public PublicArticlesController(
        GetPublishedArticlesUseCase getPublishedArticles,
        GetPublishedArticleUseCase getPublishedArticle)
    {
        _getPublishedArticles = getPublishedArticles;
        _getPublishedArticle = getPublishedArticle;
    }

    // GET /api/public/articles
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<PublicArticleResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var articles = await _getPublishedArticles.ExecuteAsync(
            cancellationToken);

        var responses = articles
            .Select(ToResponse)
            .ToList();

        return Ok(responses);
    }

    // GET /api/public/articles/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PublicArticleResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var article = await _getPublishedArticle.ExecuteAsync(
            id,
            cancellationToken);

        if (article is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(article));
    }

    private static PublicArticleResponse ToResponse(
        Article article)
    {
        return new PublicArticleResponse(
            article.Id,
            article.Title.Value,
            article.Content,
            article.PublishedAt,
            article.Category?.Value,
            article.Tags
                .Select(tag => tag.Value)
                .ToArray());
    }
}

// 読者向けには管理用情報を必要以上に公開しない
public sealed record PublicArticleResponse(
    Guid Id,
    string Title,
    string Content,
    DateTimeOffset? PublishedAt,
    string? Category,
    IReadOnlyList<string> Tags);
