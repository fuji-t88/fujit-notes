
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Domain.Articles;
using FujitNotes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FujitNotes.Infrastructure.Articles.Repositories;

public class EfArticleRepository : IArticleRepository
{
    private readonly FujitNotesDbContext _dbContext;

    public EfArticleRepository(
        FujitNotesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Article article,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Articles.AddAsync(
            article,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<Article?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Articles
            .FirstOrDefaultAsync(
                article => article.Id == id,
                cancellationToken);
    }

    // 新規追加：新しい記事から一覧取得
    public async Task<IReadOnlyList<Article>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Articles
            .AsNoTracking()
            .OrderByDescending(article => article.CreatedAt)
            .ThenByDescending(article => article.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Article article,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Articles.Update(article);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
