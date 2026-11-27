using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Domain.Articles;
using Xunit;

namespace FujitNotes.Application.Tests.Articles;

public class ArchiveArticleUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_下書きは非公開化できない()
    {
        var repository = new FakeArticleRepository();
        var article = new Article("下書き記事", "記事本文");
        await repository.AddAsync(article);
        var useCase = new ArchiveArticleUseCase(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(article.Id));

        Assert.Equal(ArticleStatus.Draft, article.Status);
        Assert.Null(article.PublishedAt);
        Assert.Equal(0, repository.UpdateCallCount);
    }


    [Fact]
    public async Task ExecuteAsync_公開済み記事をアーカイブできる()
    {
        var repository = new FakeArticleRepository();

        var article = new Article(
            "公開済み記事",
            "記事本文");

        article.Publish();

        await repository.AddAsync(article);

        var publishedAt = article.PublishedAt;

        var useCase = new ArchiveArticleUseCase(repository);

        var result = await useCase.ExecuteAsync(article.Id);

        Assert.True(result);
        Assert.Equal(
            ArticleStatus.Archived,
            article.Status);
        Assert.Equal(publishedAt, article.PublishedAt);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_存在しない記事はFalseを返す()
    {
        var repository = new FakeArticleRepository();

        var useCase = new ArchiveArticleUseCase(repository);

        var result = await useCase.ExecuteAsync(
            Guid.NewGuid());

        Assert.False(result);
        Assert.Equal(0, repository.UpdateCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_再度の非公開化は拒否する()
    {
        var repository = new FakeArticleRepository();
        var article = new Article("非公開記事", "記事本文");
        article.Publish();
        article.Archive();
        await repository.AddAsync(article);
        var useCase = new ArchiveArticleUseCase(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(article.Id));

        Assert.Equal(ArticleStatus.Archived, article.Status);
        Assert.Equal(0, repository.UpdateCallCount);
    }


    [Fact]
    public async Task ExecuteAsync_非公開化しても記事内容と公開日時は変わらない()
    {
        var repository = new FakeArticleRepository();
        var article = new Article("元のタイトル", "元の本文");
        article.Publish();
        var originalId = article.Id;
        var originalCreatedAt = article.CreatedAt;
        var publishedAt = article.PublishedAt;
        await repository.AddAsync(article);
        var useCase = new ArchiveArticleUseCase(repository);

        var result = await useCase.ExecuteAsync(article.Id);

        Assert.True(result);
        Assert.Equal(ArticleStatus.Archived, article.Status);
        Assert.Equal(originalId, article.Id);
        Assert.Equal(originalCreatedAt, article.CreatedAt);
        Assert.Equal("元のタイトル", article.Title.Value);
        Assert.Equal("元の本文", article.Content);
        Assert.Equal(publishedAt, article.PublishedAt);
        Assert.Equal(1, repository.UpdateCallCount);
    }


    private sealed class FakeArticleRepository : IArticleRepository
    {
        private readonly Dictionary<Guid, Article> _articles = new();

        public int UpdateCallCount { get; private set; }

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

            IReadOnlyList<Article> articles = _articles.Values
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.Id)
                .ToList();

            return Task.FromResult(articles);
        }

        public Task UpdateAsync(
            Article article,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            UpdateCallCount++;

            _articles[article.Id] = article;

            return Task.CompletedTask;
        }
    }
}
