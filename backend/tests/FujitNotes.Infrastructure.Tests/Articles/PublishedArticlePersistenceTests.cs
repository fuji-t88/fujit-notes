
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Domain.Articles;
using FujitNotes.Infrastructure.Articles.Repositories;
using FujitNotes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace FujitNotes.Infrastructure.Tests.Articles;

public class PublishedArticlePersistenceTests
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
                "テスト専用DB以外への接続は禁止されています。");
        }

        var options =
            new DbContextOptionsBuilder<FujitNotesDbContext>()
                .UseNpgsql(connectionString)
                .Options;

        return new FujitNotesDbContext(options);
    }

    private static async Task CleanupAsync(params Guid[] ids)
    {
        await using var db = CreateDbContext();

        foreach (var id in ids)
        {
            var article = await db.Articles.FindAsync(id);

            if (article is not null)
            {
                db.Articles.Remove(article);
            }
        }

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetPublishedArticles_公開済み記事だけをDBから取得できる()
    {
        var draft = new Article(
            "Step20 DB下書き",
            "下書き本文");

        var published = new Article(
            "Step20 DB公開記事",
            "公開本文");

        published.Publish();

        var archived = new Article(
            "Step20 DBアーカイブ",
            "アーカイブ本文");

        archived.Publish();
        archived.Archive();

        try
        {
            // 3種類の記事をPostgreSQLに保存
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                await repository.AddAsync(draft);
                await repository.AddAsync(published);
                await repository.AddAsync(archived);
            }

            // 新しいDbContextから公開記事一覧を取得
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                var useCase =
                    new GetPublishedArticlesUseCase(repository);

                var results = await useCase.ExecuteAsync();

                Assert.Contains(
                    results,
                    article => article.Id == published.Id);

                Assert.DoesNotContain(
                    results,
                    article => article.Id == draft.Id);

                Assert.DoesNotContain(
                    results,
                    article => article.Id == archived.Id);

                Assert.All(
                    results,
                    article => Assert.Equal(
                        ArticleStatus.Published,
                        article.Status));
            }
        }
        finally
        {
            await CleanupAsync(
                draft.Id,
                published.Id,
                archived.Id);
        }
    }

    [Fact]
    public async Task GetPublishedArticle_記事状態に応じてDBからの取得を制御できる()
    {
        var draft = new Article(
            "詳細テスト下書き",
            "下書き本文");

        var published = new Article(
            "詳細テスト公開記事",
            "公開本文");

        published.Publish();

        var archived = new Article(
            "詳細テストアーカイブ",
            "アーカイブ本文");

        archived.Publish();
        archived.Archive();

        try
        {
            // PostgreSQLへ記事を保存
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                await repository.AddAsync(draft);
                await repository.AddAsync(published);
                await repository.AddAsync(archived);
            }

            // 別のDbContextで公開記事詳細を取得
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                var useCase =
                    new GetPublishedArticleUseCase(repository);

                var publishedResult =
                    await useCase.ExecuteAsync(published.Id);

                var draftResult =
                    await useCase.ExecuteAsync(draft.Id);

                var archivedResult =
                    await useCase.ExecuteAsync(archived.Id);

                var unknownResult =
                    await useCase.ExecuteAsync(Guid.NewGuid());

                // 公開済み記事だけ取得できる
                Assert.NotNull(publishedResult);

                Assert.Equal(
                    published.Id,
                    publishedResult.Id);

                Assert.Equal(
                    "詳細テスト公開記事",
                    publishedResult.Title.Value);

                Assert.Equal(
                    ArticleStatus.Published,
                    publishedResult.Status);

                Assert.NotNull(publishedResult.PublishedAt);

                // 非公開記事と存在しない記事は取得不可
                Assert.Null(draftResult);
                Assert.Null(archivedResult);
                Assert.Null(unknownResult);
            }
        }
        finally
        {
            await CleanupAsync(
                draft.Id,
                published.Id,
                archived.Id);
        }
    }
}
