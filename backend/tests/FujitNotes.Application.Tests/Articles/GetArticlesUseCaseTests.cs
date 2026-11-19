
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Domain.Articles;
using Xunit;

namespace FujitNotes.Application.Tests.Articles;

public class GetArticlesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_記事がない場合は空の一覧を返す()
    {
        var repository = new FakeArticleRepository();
        var useCase = new GetArticlesUseCase(repository);

        var result = await useCase.ExecuteAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_保存された記事をすべて返す()
    {
        var repository = new FakeArticleRepository();

        var article1 = new Article(
            "記事その1",
            "1件目の本文");

        var article2 = new Article(
            "記事その2",
            "2件目の本文");

        await repository.AddAsync(article1);
        await repository.AddAsync(article2);

        var useCase = new GetArticlesUseCase(repository);

        var result = await useCase.ExecuteAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, a => a.Id == article1.Id);
        Assert.Contains(result, a => a.Id == article2.Id);
    }

    // このテスト専用のFake Repository
    private sealed class FakeArticleRepository : IArticleRepository
    {
        private readonly Dictionary<Guid, Article> _articles = new();

        public Task AddAsync(
            Article article,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _articles.Add(article.Id, article);
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

        public Task<IReadOnlyList<Article>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<Article> result = _articles.Values
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.Id)
                .ToList();

            return Task.FromResult(result);
        }

        public Task UpdateAsync(
            Article article,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _articles[article.Id] = article;
            return Task.CompletedTask;
        }
    }
}
