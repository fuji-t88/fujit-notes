
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Domain.Articles;

namespace FujitNotes.Application.Articles.UseCases;

public sealed class CreateArticleUseCase
{
    private readonly IArticleRepository _repository;

    public CreateArticleUseCase(
        IArticleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> ExecuteAsync(
        string title,
        string content,
        CancellationToken cancellationToken = default)
    {
        // Domainモデルに記事作成を任せる
        var article = new Article(title, content);

        // Repositoryを通じて記事を保存する
        await _repository.AddAsync(
            article,
            cancellationToken);

        // 作成した記事のIDを返す
        return article.Id;
    }
}
