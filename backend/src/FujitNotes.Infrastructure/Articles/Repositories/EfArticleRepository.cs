
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Domain.Articles;
using FujitNotes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FujitNotes.Infrastructure.Articles.Repositories;

public sealed class EfArticleRepository : IArticleRepository
{
    private readonly FujitNotesDbContext _dbContext;

    public EfArticleRepository(
        FujitNotesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // 記事を新規保存
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

    // IDから記事を取得
    public async Task<Article?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Articles
            .FirstOrDefaultAsync(
                article => article.Id == id,
                cancellationToken);
    }

    // 記事を更新
    public async Task UpdateAsync(
        Article article,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Articles.Update(article);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
