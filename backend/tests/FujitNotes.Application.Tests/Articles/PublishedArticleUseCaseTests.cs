
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Domain.Articles;
using Xunit;

namespace FujitNotes.Application.Tests.Articles;

public class PublishedArticleUseCaseTests
{
    [Fact]
    public async Task GetPublishedArticles_公開済み記事だけを返す()
    {
        var repository = new FakeArticleRepository();

        var draft = new Article("下書き", "下書き本文");

        var published = new Article("公開記事", "公開本文");
        published.Publish();

        var archived = new Article("保管記事", "保管本文");
        archived.Archive();

        await repository.AddAsync(draft);
        await repository.AddAsync(published);
        await repository.AddAsync(archived);

        var useCase = new GetPublishedArticlesUseCase(
            repository);

        var results = await useCase.ExecuteAsync();

        var result = Assert.Single(results);

        Assert.Equal(published.Id, result.Id);
        Assert.Equal(
            ArticleStatus.Published,
            result.Status);
    }

    [Fact]
    public async Task GetPublishedArticles_公開記事がなければ空一覧を返す()
    {
        var repository = new FakeArticleRepository();

        var draft = new Article("下書き", "本文");

        await repository.AddAsync(draft);

        var useCase = new GetPublishedArticlesUseCase(
            repository);

        var results = await useCase.ExecuteAsync();

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetPublishedArticles_公開日時の新しい順に返す()
    {
        var repository = new FakeArticleRepository();

        var older = new Article("古い公開記事", "本文");
        older.Publish();

        // 公開日時の順序を確実に分ける
        await Task.Delay(30);

        var newer = new Article("新しい公開記事", "本文");
        newer.Publish();

        await repository.AddAsync(older);
        await repository.AddAsync(newer);

        var useCase = new GetPublishedArticlesUseCase(
            repository);

        var results = await useCase.ExecuteAsync();

        Assert.Equal(2, results.Count);

        Assert.Equal(newer.Id, results[0].Id);
        Assert.Equal(older.Id, results[1].Id);
    }

    [Fact]
    public async Task GetPublishedArticles_公開後にアーカイブした記事を除外する()
    {
        var repository = new FakeArticleRepository();

        var article = new Article(
            "元公開記事",
            "本文");

        article.Publish();
        article.Archive();

        await repository.AddAsync(article);

        var useCase = new GetPublishedArticlesUseCase(
            repository);

        var results = await useCase.ExecuteAsync();

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetPublishedArticle_公開記事を取得できる()
    {
        var repository = new FakeArticleRepository();

        var article = new Article(
            "公開記事",
            "公開本文");

        article.Publish();

        await repository.AddAsync(article);

        var useCase = new GetPublishedArticleUseCase(
            repository);

        var result = await useCase.ExecuteAsync(article.Id);

        Assert.NotNull(result);
        Assert.Equal(article.Id, result.Id);
        Assert.Equal("公開記事", result.Title.Value);
        Assert.Equal(
            ArticleStatus.Published,
            result.Status);
    }

    [Fact]
    public async Task GetPublishedArticle_下書き記事はNullを返す()
    {
        var repository = new FakeArticleRepository();

        var article = new Article(
            "下書き",
            "本文");

        await repository.AddAsync(article);

        var useCase = new GetPublishedArticleUseCase(
            repository);

        var result = await useCase.ExecuteAsync(article.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPublishedArticle_アーカイブ済み記事はNullを返す()
    {
        var repository = new FakeArticleRepository();

        var article = new Article(
            "アーカイブ記事",
            "本文");

        article.Publish();
        article.Archive();

        await repository.AddAsync(article);

        var useCase = new GetPublishedArticleUseCase(
            repository);

        var result = await useCase.ExecuteAsync(article.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPublishedArticle_存在しない記事はNullを返す()
    {
        var repository = new FakeArticleRepository();

        var useCase = new GetPublishedArticleUseCase(
            repository);

        var result = await useCase.ExecuteAsync(
            Guid.NewGuid());

        Assert.Null(result);
    }

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

            IReadOnlyList<Article> results = _articles.Values
                .OrderByDescending(article => article.CreatedAt)
                .ThenByDescending(article => article.Id)
                .ToList();

            return Task.FromResult(results);
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
