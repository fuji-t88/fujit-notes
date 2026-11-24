
using FujitNotes.Application.Articles.Repositories;

namespace FujitNotes.Application.Articles.UseCases;

public sealed class UpdateArticleUseCase
{
    private readonly IArticleRepository _repository;

    public UpdateArticleUseCase(
        IArticleRepository repository)
    {
        _repository = repository;
    }

    // 従来の更新処理
    // カテゴリ・タグは変更せず維持する
    public Task<bool> ExecuteAsync(
        Guid id,
        string title,
        string content,
        CancellationToken cancellationToken = default)
    {
        return ExecuteCoreAsync(
            id,
            title,
            content,
            updateMetadata: false,
            category: null,
            tags: null,
            cancellationToken: cancellationToken);
    }

    // カテゴリ・タグも含めて更新する処理
    public Task<bool> ExecuteAsync(
        Guid id,
        string title,
        string content,
        string? category,
        IEnumerable<string> tags,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tags);

        return ExecuteCoreAsync(
            id,
            title,
            content,
            updateMetadata: true,
            category: category,
            tags: tags,
            cancellationToken: cancellationToken);
    }

    // 共通更新処理
    private async Task<bool> ExecuteCoreAsync(
        Guid id,
        string title,
        string content,
        bool updateMetadata,
        string? category,
        IEnumerable<string>? tags,
        CancellationToken cancellationToken)
    {
        // 更新対象の記事を取得
        var article = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        // 対象の記事が見つからない場合
        if (article is null)
        {
            return false;
        }

        // 下書きの記事タイトルと本文を更新
        article.EditDraft(title, content);

        // カテゴリ・タグ付き更新の場合のみ変更
        if (updateMetadata)
        {
            article.ChangeCategory(category);
            article.ChangeTags(tags!);
        }

        // Repositoryを通じて変更を保存
        await _repository.UpdateAsync(
            article,
            cancellationToken);

        return true;
    }
}
