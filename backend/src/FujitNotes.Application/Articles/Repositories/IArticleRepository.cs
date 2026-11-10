
using FujitNotes.Domain.Articles;

namespace FujitNotes.Application.Articles.Repositories;

public interface IArticleRepository
{
    Task AddAsync(
        Article article,
        CancellationToken cancellationToken = default);

    Task<Article?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Article article,
        CancellationToken cancellationToken = default);
}
