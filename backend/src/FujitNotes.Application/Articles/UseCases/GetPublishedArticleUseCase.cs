
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Domain.Articles;

namespace FujitNotes.Application.Articles.UseCases;

public sealed class GetPublishedArticleUseCase
{
    private readonly IArticleRepository _repository;

    public GetPublishedArticleUseCase(
        IArticleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Article?> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var article = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        // 存在しない記事も、非公開記事も取得させない
        if (article is null ||
            article.Status != ArticleStatus.Published)
        {
            return null;
        }

        return article;
    }
}
