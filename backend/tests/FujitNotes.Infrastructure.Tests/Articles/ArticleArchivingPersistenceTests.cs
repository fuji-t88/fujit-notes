
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Domain.Articles;
using FujitNotes.Infrastructure.Articles.Repositories;
using FujitNotes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace FujitNotes.Infrastructure.Tests.Articles;

public class ArticleArchivingPersistenceTests
{
    private static FujitNotesDbContext CreateDbContext()
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "FUJITNOTES_TEST_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "テストDB接続情報が設定されていません。");
        }

        var builder =
            new NpgsqlConnectionStringBuilder(connectionString);

        if (builder.Database != "fujitnotes_test" ||
            builder.Host != "localhost" ||
            builder.Port != 5433)
        {
            throw new InvalidOperationException(
                "テスト専用DB以外には接続できません。");
        }

        var options =
            new DbContextOptionsBuilder<FujitNotesDbContext>()
                .UseNpgsql(connectionString)
                .Options;

        return new FujitNotesDbContext(options);
    }

    private static async Task CleanupAsync(Guid articleId)
    {
        await using var db = CreateDbContext();

        var article = await db.Articles.FindAsync(articleId);

        if (article is not null)
        {
            db.Articles.Remove(article);
            await db.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task ArchiveArticleUseCase_下書き記事をDB上でアーカイブできる()
    {
        var article = new Article(
            "下書きアーカイブテスト",
            "下書きの記事本文");

        try
        {
            // 1. 記事をPostgreSQLへ保存
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                await repository.AddAsync(article);
            }

            DateTimeOffset persistedCreatedAt;

            // 2. DB保存済みの作成日時を取得
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                var before = await repository.GetByIdAsync(
                    article.Id);

                Assert.NotNull(before);
                Assert.Equal(ArticleStatus.Draft, before.Status);

                persistedCreatedAt = before.CreatedAt;
            }

            // 3. UseCaseでアーカイブ
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                var useCase = new ArchiveArticleUseCase(repository);

                var result = await useCase.ExecuteAsync(article.Id);

                Assert.True(result);
            }

            // 4. 新しいDbContextで再取得
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                var archived = await repository.GetByIdAsync(
                    article.Id);

                Assert.NotNull(archived);

                Assert.Equal(
                    ArticleStatus.Archived,
                    archived.Status);

                Assert.Equal(
                    "下書きアーカイブテスト",
                    archived.Title.Value);

                Assert.Equal(
                    "下書きの記事本文",
                    archived.Content);

                Assert.Equal(article.Id, archived.Id);

                Assert.Equal(
                    persistedCreatedAt,
                    archived.CreatedAt);

                Assert.Null(archived.PublishedAt);
            }
        }
        finally
        {
            await CleanupAsync(article.Id);
        }
    }

    [Fact]
    public async Task ArchiveArticleUseCase_公開済み記事をアーカイブして公開日時を維持できる()
    {
        var article = new Article(
            "公開済みアーカイブテスト",
            "公開済みの記事本文");

        article.Publish();

        try
        {
            // 1. 公開済み記事をPostgreSQLへ保存
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                await repository.AddAsync(article);
            }

            DateTimeOffset persistedCreatedAt;
            DateTimeOffset? persistedPublishedAt;

            // 2. DBに保存された日時を取得
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                var before = await repository.GetByIdAsync(
                    article.Id);

                Assert.NotNull(before);
                Assert.Equal(
                    ArticleStatus.Published,
                    before.Status);

                Assert.NotNull(before.PublishedAt);

                persistedCreatedAt = before.CreatedAt;
                persistedPublishedAt = before.PublishedAt;
            }

            // 3. 公開済み記事をアーカイブ
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                var useCase = new ArchiveArticleUseCase(repository);

                var result = await useCase.ExecuteAsync(article.Id);

                Assert.True(result);
            }

            // 4. DBから再取得して確認
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                var archived = await repository.GetByIdAsync(
                    article.Id);

                Assert.NotNull(archived);

                Assert.Equal(
                    ArticleStatus.Archived,
                    archived.Status);

                Assert.Equal(
                    "公開済みアーカイブテスト",
                    archived.Title.Value);

                Assert.Equal(
                    "公開済みの記事本文",
                    archived.Content);

                Assert.Equal(article.Id, archived.Id);

                Assert.Equal(
                    persistedCreatedAt,
                    archived.CreatedAt);

                Assert.Equal(
                    persistedPublishedAt,
                    archived.PublishedAt);
            }
        }
        finally
        {
            await CleanupAsync(article.Id);
        }
    }
}
