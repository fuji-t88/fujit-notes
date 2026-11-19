
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Domain.Articles;

namespace FujitNotes.Application.Articles.UseCases;

public sealed class GetPublishedArticlesUseCase
{
    private readonly IArticleRepository _repository;

    public GetPublishedArticlesUseCase(
        IArticleRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Article>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var articles = await _repository.GetAllAsync(
            cancellationToken);

        // 公開済み記事だけを返す
        return articles
            .Where(article =>
                article.Status == ArticleStatus.Published)
            .OrderByDescending(article => article.PublishedAt)
            .ThenByDescending(article => article.Id)
            .ToList();
    }
}
