
using FujitNotes.Application.Articles.Repositories;

namespace FujitNotes.Application.Articles.UseCases;

public sealed class ArchiveArticleUseCase
{
    private readonly IArticleRepository _repository;

    public ArchiveArticleUseCase(
        IArticleRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // アーカイブ対象の記事を取得
        var article = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        // 存在しない記事はFalseを返す
        if (article is null)
        {
            return false;
        }

        // 状態変更のルールはDomain層に任せる
        article.Archive();

        // 変更された状態をRepositoryで保存する
        await _repository.UpdateAsync(
            article,
            cancellationToken);

        return true;
    }
}
