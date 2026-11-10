
using FujitNotes.Application.Articles.Repositories;

namespace FujitNotes.Application.Articles.UseCases;

public sealed class PublishArticleUseCase
{
    private readonly IArticleRepository _repository;

    public PublishArticleUseCase(
        IArticleRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // 公開対象の記事を取得する
        var article = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        // 記事が存在しない場合
        if (article is null)
        {
            return false;
        }

        // 公開可能かどうかはDomainが判断する
        article.Publish();

        // 変更した記事を保存する
        await _repository.UpdateAsync(
            article,
            cancellationToken);

        return true;
    }
}
