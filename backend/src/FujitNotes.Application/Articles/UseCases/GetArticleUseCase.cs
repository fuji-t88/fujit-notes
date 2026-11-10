
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Domain.Articles;

namespace FujitNotes.Application.Articles.UseCases;

public sealed class GetArticleUseCase
{
    private readonly IArticleRepository _repository;

    public GetArticleUseCase(
        IArticleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Article?> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(
            id,
            cancellationToken);
    }
}
