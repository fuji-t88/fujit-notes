
using System.Collections.Concurrent;
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Domain.Articles;

namespace FujitNotes.Infrastructure.Articles.Repositories;

// 開発・学習用のメモリRepository
public sealed class InMemoryArticleRepository : IArticleRepository
{
    private readonly ConcurrentDictionary<Guid, Article> _articles = new();

    public Task AddAsync(
        Article article,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_articles.TryAdd(article.Id, article))
        {
            throw new InvalidOperationException(
                "同じIDの記事がすでに存在します。");
        }

        return Task.CompletedTask;
    }

    public Task<Article?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _articles.TryGetValue(id, out var article);

        return Task.FromResult(article);
    }

    public Task UpdateAsync(
        Article article,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_articles.ContainsKey(article.Id))
        {
            throw new KeyNotFoundException(
                "更新対象の記事が見つかりません。");
        }

        _articles[article.Id] = article;

        return Task.CompletedTask;
    }
}
