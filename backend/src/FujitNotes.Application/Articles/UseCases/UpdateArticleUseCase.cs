
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

    public async Task<bool> ExecuteAsync(
        Guid id,
        string title,
        string content,
        CancellationToken cancellationToken = default)
    {
        // 更新対象の記事を取得
        var article = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        // 記事が存在しない場合
        if (article is null)
        {
            return false;
        }

        // 編集ルールはDomainに任せる
        article.EditDraft(title, content);

        // PostgreSQLなどの保存先へ変更を反映
        await _repository.UpdateAsync(
            article,
            cancellationToken);

        return true;
    }
}
