
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Domain.Articles;

namespace FujitNotes.Application.Articles.UseCases;

public class GetArticlesUseCase
{
    private readonly IArticleRepository _repository;

    public GetArticlesUseCase(
        IArticleRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Article>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(
            cancellationToken);
    }
}
