
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Domain.Articles;

namespace FujitNotes.Application.Tests.Articles;

public class ArticleUseCaseTests
{
    [Fact]
    public async Task CreateArticle_ValidInput_SavesArticle()
    {
        // Arrange
        var repository = new FakeArticleRepository();
        var useCase = new CreateArticleUseCase(repository);

        // Act
        var id = await useCase.ExecuteAsync(
            "DDDを学ぶ",
            "Application層の学習");

        // Assert
        var article = await repository.GetByIdAsync(id);

        Assert.NotNull(article);
        Assert.Equal("DDDを学ぶ", article.Title.Value);
        Assert.Equal(ArticleStatus.Draft, article.Status);
    }

    [Fact]
    public async Task CreateArticle_EmptyTitle_ThrowsException()
    {
        // Arrange
        var repository = new FakeArticleRepository();
        var useCase = new CreateArticleUseCase(repository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync("", "本文"));

        Assert.Empty(repository.Articles);
    }

    [Fact]
    public async Task GetArticle_ExistingId_ReturnsArticle()
    {
        // Arrange
        var repository = new FakeArticleRepository();
        var article = new Article("テスト記事", "本文");
        await repository.AddAsync(article);

        var useCase = new GetArticleUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(article.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(article.Id, result.Id);
    }

    [Fact]
    public async Task GetArticle_UnknownId_ReturnsNull()
    {
        // Arrange
        var repository = new FakeArticleRepository();
        var useCase = new GetArticleUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task PublishArticle_ValidArticle_PublishesArticle()
    {
        // Arrange
        var repository = new FakeArticleRepository();
        var article = new Article("公開テスト", "本文あり");
        await repository.AddAsync(article);

        var useCase = new PublishArticleUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(article.Id);

        // Assert
        var saved = await repository.GetByIdAsync(article.Id);

        Assert.True(result);
        Assert.NotNull(saved);
        Assert.Equal(ArticleStatus.Published, saved.Status);
        Assert.NotNull(saved.PublishedAt);
    }

    [Fact]
    public async Task PublishArticle_UnknownId_ReturnsFalse()
    {
        // Arrange
        var repository = new FakeArticleRepository();
        var useCase = new PublishArticleUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task PublishArticle_EmptyContent_ThrowsException()
    {
        // Arrange
        var repository = new FakeArticleRepository();
        var article = new Article("タイトル", "");
        await repository.AddAsync(article);

        var useCase = new PublishArticleUseCase(repository);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(article.Id));

        Assert.Equal(ArticleStatus.Draft, article.Status);
    }

    // テスト専用のRepository
    private sealed class FakeArticleRepository : IArticleRepository
    {
        public Dictionary<Guid, Article> Articles { get; } = new();

        public Task AddAsync(
            Article article,
            CancellationToken cancellationToken = default)
        {
            Articles.Add(article.Id, article);
            return Task.CompletedTask;
        }

        public Task<Article?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            Articles.TryGetValue(id, out var article);
            return Task.FromResult(article);
        }

        public Task UpdateAsync(
            Article article,
            CancellationToken cancellationToken = default)
        {
            Articles[article.Id] = article;
            return Task.CompletedTask;
        }
    }
}
