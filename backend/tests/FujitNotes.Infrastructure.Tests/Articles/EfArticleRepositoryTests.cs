
using FujitNotes.Domain.Articles;
using FujitNotes.Infrastructure.Articles.Repositories;
using FujitNotes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace FujitNotes.Infrastructure.Tests.Articles;

public class EfArticleRepositoryTests
{
    private static FujitNotesDbContext CreateDbContext()
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "FUJITNOTES_TEST_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "テストDB接続情報がありません。");
        }

        var builder =
            new NpgsqlConnectionStringBuilder(connectionString);

        // テスト専用DB以外への接続を拒否
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

    private static async Task CleanupAsync(Guid id)
    {
        await using var db = CreateDbContext();

        var article = await db.Articles.FindAsync(id);

        if (article is not null)
        {
            db.Articles.Remove(article);
            await db.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task AddAsync_記事をDBへ保存できる()
    {
        var article = new Article(
            "DB保存テスト",
            "PostgreSQLに保存する本文");

        try
        {
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                await repository.AddAsync(article);
            }

            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                var saved =
                    await repository.GetByIdAsync(article.Id);

                Assert.NotNull(saved);
                Assert.Equal(article.Id, saved.Id);
                Assert.Equal("DB保存テスト", saved.Title.Value);
                Assert.Equal(ArticleStatus.Draft, saved.Status);
            }
        }
        finally
        {
            await CleanupAsync(article.Id);
        }
    }

    [Fact]
    public async Task UpdateAsync_記事の公開状態を保存できる()
    {
        var article = new Article(
            "記事公開テスト",
            "公開可能な本文");

        try
        {
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                await repository.AddAsync(article);
            }

            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                var saved =
                    await repository.GetByIdAsync(article.Id);

                Assert.NotNull(saved);

                saved.Publish();

                await repository.UpdateAsync(saved);
            }

            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                var published =
                    await repository.GetByIdAsync(article.Id);

                Assert.NotNull(published);
                Assert.Equal(
                    ArticleStatus.Published,
                    published.Status);
                Assert.NotNull(published.PublishedAt);
            }
        }
        finally
        {
            await CleanupAsync(article.Id);
        }
    }

    [Fact]
    public async Task GetByIdAsync_存在しない記事はNullを返す()
    {
        await using var db = CreateDbContext();

        var repository = new EfArticleRepository(db);

        var result =
            await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}
