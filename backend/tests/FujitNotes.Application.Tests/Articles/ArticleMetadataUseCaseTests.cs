
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Domain.Articles;
using Xunit;

namespace FujitNotes.Application.Tests.Articles;

public sealed class ArticleMetadataUseCaseTests
{
    // 1. カテゴリ・タグ付きの記事作成
    [Fact]
    public async Task Create_WithMetadata_SavesCategoryAndTags()
    {
        var repository = new FakeArticleRepository();
        var useCase = new CreateArticleUseCase(repository);

        var id = await useCase.ExecuteAsync(
            "DDDを学ぶ",
            "値オブジェクトについて",
            "Development",
            new[] { "C#", "DDD" });

        var article = await repository.GetByIdAsync(id);

        Assert.NotNull(article);
        Assert.Equal("Development", article.Category?.Value);
        Assert.Equal(2, article.Tags.Count);
        Assert.Contains(
            article.Tags,
            tag => tag.Value == "C#");
        Assert.Contains(
            article.Tags,
            tag => tag.Value == "DDD");
        Assert.Equal(ArticleStatus.Draft, article.Status);
        Assert.Equal(1, repository.AddCallCount);
    }

    // 2. 従来の記事作成も維持する
    [Fact]
    public async Task Create_WithoutMetadata_KeepsMetadataEmpty()
    {
        var repository = new FakeArticleRepository();
        var useCase = new CreateArticleUseCase(repository);

        var id = await useCase.ExecuteAsync(
            "通常の記事",
            "通常の本文");

        var article = await repository.GetByIdAsync(id);

        Assert.NotNull(article);
        Assert.Null(article.Category);
        Assert.Empty(article.Tags);
        Assert.Equal(1, repository.AddCallCount);
    }

    // 3. 不正なカテゴリの記事は保存されない
    [Fact]
    public async Task Create_InvalidCategory_DoesNotSave()
    {
        var repository = new FakeArticleRepository();
        var useCase = new CreateArticleUseCase(repository);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => useCase.ExecuteAsync(
                "テスト記事",
                "本文",
                "InvalidCategory",
                new[] { "C#" }));

        Assert.Equal(0, repository.AddCallCount);
        Assert.Empty(repository.Articles);
    }

    // 4. 下書きのカテゴリ・タグを変更できる
    [Fact]
    public async Task Update_WithMetadata_ChangesCategoryAndTags()
    {
        var repository = new FakeArticleRepository();

        var article = new Article("変更前", "元の本文");
        article.ChangeCategory("Development");
        article.ChangeTags(new[] { "C#" });

        await repository.AddAsync(article);

        var useCase = new UpdateArticleUseCase(repository);

        var result = await useCase.ExecuteAsync(
            article.Id,
            "変更後",
            "新しい本文",
            "Build in Public",
            new[] { "Next.js", "DDD" });

        var updated = await repository.GetByIdAsync(article.Id);

        Assert.True(result);
        Assert.NotNull(updated);
        Assert.Equal("変更後", updated.Title.Value);
        Assert.Equal("新しい本文", updated.Content);
        Assert.Equal("Build in Public", updated.Category?.Value);
        Assert.Equal(2, updated.Tags.Count);
        Assert.Contains(
            updated.Tags,
            tag => tag.Value == "Next.js");
        Assert.Contains(
            updated.Tags,
            tag => tag.Value == "DDD");
        Assert.Equal(1, repository.UpdateCallCount);
    }

    // 5. 従来の更新方法ではメタデータを維持する
    [Fact]
    public async Task Update_WithoutMetadata_PreservesMetadata()
    {
        var repository = new FakeArticleRepository();

        var article = new Article("変更前", "本文");
        article.ChangeCategory("Development");
        article.ChangeTags(new[] { "C#", "DDD" });

        await repository.AddAsync(article);

        var useCase = new UpdateArticleUseCase(repository);

        var result = await useCase.ExecuteAsync(
            article.Id,
            "タイトル変更",
            "本文変更");

        var updated = await repository.GetByIdAsync(article.Id);

        Assert.True(result);
        Assert.NotNull(updated);
        Assert.Equal("タイトル変更", updated.Title.Value);
        Assert.Equal("Development", updated.Category?.Value);
        Assert.Equal(2, updated.Tags.Count);
        Assert.Contains(
            updated.Tags,
            tag => tag.Value == "C#");
        Assert.Contains(
            updated.Tags,
            tag => tag.Value == "DDD");
        Assert.Equal(1, repository.UpdateCallCount);
    }

    // 6. カテゴリとタグを解除できる
    [Fact]
    public async Task Update_EmptyMetadata_ClearsCategoryAndTags()
    {
        var repository = new FakeArticleRepository();

        var article = new Article("記事タイトル", "本文");
        article.ChangeCategory("Development");
        article.ChangeTags(new[] { "C#", "DDD" });

        await repository.AddAsync(article);

        var useCase = new UpdateArticleUseCase(repository);

        var result = await useCase.ExecuteAsync(
            article.Id,
            "記事タイトル",
            "本文",
            category: null,
            tags: Array.Empty<string>());

        var updated = await repository.GetByIdAsync(article.Id);

        Assert.True(result);
        Assert.NotNull(updated);
        Assert.Null(updated.Category);
        Assert.Empty(updated.Tags);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    // 7. 存在しない記事は更新しない
    [Fact]
    public async Task Update_UnknownId_ReturnsFalse()
    {
        var repository = new FakeArticleRepository();
        var useCase = new UpdateArticleUseCase(repository);

        var result = await useCase.ExecuteAsync(
            Guid.NewGuid(),
            "タイトル",
            "本文",
            "Development",
            new[] { "C#" });

        Assert.False(result);
        Assert.Equal(0, repository.UpdateCallCount);
    }

    // 8. 公開済み記事は編集できない
    [Fact]
    public async Task Update_PublishedArticle_CannotEditMetadata()
    {
        var repository = new FakeArticleRepository();

        var article = new Article("公開記事", "公開本文");
        article.ChangeCategory("Development");
        article.ChangeTags(new[] { "C#" });
        article.Publish();

        await repository.AddAsync(article);

        var useCase = new UpdateArticleUseCase(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                article.Id,
                "変更後タイトル",
                "変更後本文",
                "Build in Public",
                new[] { "DDD" }));

        Assert.Equal(ArticleStatus.Published, article.Status);
        Assert.Equal("公開記事", article.Title.Value);
        Assert.Equal("Development", article.Category?.Value);
        Assert.Equal("C#", Assert.Single(article.Tags).Value);
        Assert.Equal(0, repository.UpdateCallCount);
    }

    // テスト専用のインメモリRepository
    private sealed class FakeArticleRepository : IArticleRepository
    {
        public Dictionary<Guid, Article> Articles { get; } = new();

        public int AddCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }

        public Task AddAsync(
            Article article,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Articles.Add(article.Id, article);
            AddCallCount++;

            return Task.CompletedTask;
        }

        public Task<Article?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Articles.TryGetValue(id, out var article);

            return Task.FromResult(article);
        }

        public Task<IReadOnlyList<Article>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<Article> articles = Articles.Values
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

            Articles[article.Id] = article;
            UpdateCallCount++;

            return Task.CompletedTask;
        }
    }
}
