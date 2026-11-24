
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

    // 従来の記事作成処理
    // タイトルと本文のみで作成できる
    public Task<Guid> ExecuteAsync(
        string title,
        string content,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            title,
            content,
            category: null,
            tags: Array.Empty<string>(),
            cancellationToken: cancellationToken);
    }

    // カテゴリ・タグ付きの記事作成処理
    public async Task<Guid> ExecuteAsync(
        string title,
        string content,
        string? category,
        IEnumerable<string> tags,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tags);

        // Domainモデルで記事を作成
        var article = new Article(title, content);

        // カテゴリを設定
        article.ChangeCategory(category);

        // タグを設定
        article.ChangeTags(tags);

        // Repositoryを通じて保存
        await _repository.AddAsync(
            article,
            cancellationToken);

        // 作成した記事のIDを返す
        return article.Id;
    }
}
