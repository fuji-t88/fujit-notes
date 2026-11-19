
using FujitNotes.Domain.Articles;
using FujitNotes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace FujitNotes.Infrastructure.Tests.Articles;

public class ArticleMetadataPersistenceTests
{
    // テスト専用DB以外には接続しない
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

        if (builder.Host != "localhost" ||
            builder.Port != 5433 ||
            builder.Database != "fujitnotes_test")
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

    // テストで作成した記事のみ削除する
    private static async Task CleanupAsync(Guid articleId)
    {
        await using var context = CreateDbContext();

        await context.Articles
            .Where(article => article.Id == articleId)
            .ExecuteDeleteAsync();
    }

    [Fact]
    public async Task CategoryAndTags_保存して復元できる()
    {
        var article = new Article(
            "カテゴリ・タグ保存テスト",
            "PostgreSQLへの保存を検証します。");

        article.ChangeCategory("Development");
        article.ChangeTags(
            new[] { "C#", "DDD", "PostgreSQL" });

        try
        {
            // 保存
            await using (var context = CreateDbContext())
            {
                context.Articles.Add(article);
                await context.SaveChangesAsync();
            }

            // 別のDbContextで取得
            await using (var context = CreateDbContext())
            {
                var loaded = await context.Articles
                    .AsNoTracking()
                    .SingleAsync(item => item.Id == article.Id);

                Assert.Equal(
                    "Development",
                    loaded.Category?.Value);

                Assert.Equal(
                    new[] { "C#", "DDD", "PostgreSQL" },
                    loaded.Tags.Select(tag => tag.Value));
            }
        }
        finally
        {
            await CleanupAsync(article.Id);
        }
    }

    [Fact]
    public async Task CategoryAndTags_未設定でも保存できる()
    {
        var article = new Article(
            "未設定メタデータテスト",
            "カテゴリとタグがありません。");

        try
        {
            await using (var context = CreateDbContext())
            {
                context.Articles.Add(article);
                await context.SaveChangesAsync();
            }

            await using (var context = CreateDbContext())
            {
                var loaded = await context.Articles
                    .AsNoTracking()
                    .SingleAsync(item => item.Id == article.Id);

                Assert.Null(loaded.Category);
                Assert.Empty(loaded.Tags);
            }
        }
        finally
        {
            await CleanupAsync(article.Id);
        }
    }

    [Fact]
    public async Task CategoryAndTags_変更内容をDBに反映できる()
    {
        var article = new Article(
            "メタデータ変更テスト",
            "変更追跡を検証します。");

        article.ChangeCategory("Development");
        article.ChangeTags(new[] { "C#", "DDD" });

        try
        {
            // 初期データを保存
            await using (var context = CreateDbContext())
            {
                context.Articles.Add(article);
                await context.SaveChangesAsync();
            }

            // 読み込んだ記事のカテゴリ・タグを変更
            await using (var context = CreateDbContext())
            {
                var loaded = await context.Articles
                    .SingleAsync(item => item.Id == article.Id);

                loaded.ChangeCategory("Cloud & DevOps");
                loaded.ChangeTags(
                    new[] { "Azure", "Docker" });

                await context.SaveChangesAsync();
            }

            // 別のDbContextで変更内容を確認
            await using (var context = CreateDbContext())
            {
                var reloaded = await context.Articles
                    .AsNoTracking()
                    .SingleAsync(item => item.Id == article.Id);

                Assert.Equal(
                    "Cloud & DevOps",
                    reloaded.Category?.Value);

                Assert.Equal(
                    new[] { "Azure", "Docker" },
                    reloaded.Tags.Select(tag => tag.Value));
            }
        }
        finally
        {
            await CleanupAsync(article.Id);
        }
    }
}
