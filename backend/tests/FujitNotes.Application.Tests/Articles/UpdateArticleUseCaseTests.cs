
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Domain.Articles;
using Xunit;

namespace FujitNotes.Application.Tests.Articles;

public class UpdateArticleUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_下書き記事を更新できる()
    {
        var repository = new FakeArticleRepository();

        var article = new Article(
            "変更前タイトル",
            "変更前本文");

        await repository.AddAsync(article);

        var useCase = new UpdateArticleUseCase(repository);

        var result = await useCase.ExecuteAsync(
            article.Id,
            "変更後タイトル",
            "変更後本文");

        var updated = await repository.GetByIdAsync(article.Id);

        Assert.True(result);
        Assert.NotNull(updated);
        Assert.Equal("変更後タイトル", updated.Title.Value);
        Assert.Equal("変更後本文", updated.Content);
        Assert.Equal(ArticleStatus.Draft, updated.Status);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_存在しない記事はFalseを返す()
    {
        var repository = new FakeArticleRepository();

        var useCase = new UpdateArticleUseCase(repository);

        var result = await useCase.ExecuteAsync(
            Guid.NewGuid(),
            "タイトル",
            "本文");

        Assert.False(result);
        Assert.Equal(0, repository.UpdateCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_公開済み記事は編集できない()
    {
        var repository = new FakeArticleRepository();

        var article = new Article("公開記事", "公開本文");
        article.Publish();

        await repository.AddAsync(article);

        var useCase = new UpdateArticleUseCase(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                article.Id,
                "変更後タイトル",
                "変更後本文"));

        Assert.Equal("公開記事", article.Title.Value);
        Assert.Equal("公開本文", article.Content);
        Assert.Equal(0, repository.UpdateCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_不正なタイトルは保存しない()
    {
        var repository = new FakeArticleRepository();

        var article = new Article(
            "元のタイトル",
            "元の本文");

        await repository.AddAsync(article);

        var useCase = new UpdateArticleUseCase(repository);

        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(
                article.Id,
                "",
                "変更後本文"));

        Assert.Equal("元のタイトル", article.Title.Value);
        Assert.Equal("元の本文", article.Content);
        Assert.Equal(0, repository.UpdateCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_アーカイブ済み記事は編集できない()
    {
        var repository = new FakeArticleRepository();

        var article = new Article(
            "アーカイブ記事",
            "本文");

        article.Archive();

        await repository.AddAsync(article);

        var useCase = new UpdateArticleUseCase(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                article.Id,
                "変更後タイトル",
                "変更後本文"));

        Assert.Equal(ArticleStatus.Archived, article.Status);
        Assert.Equal(0, repository.UpdateCallCount);
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

            UpdateCallCount++;
            _articles[article.Id] = article;

            return Task.CompletedTask;
        }
    }
}
