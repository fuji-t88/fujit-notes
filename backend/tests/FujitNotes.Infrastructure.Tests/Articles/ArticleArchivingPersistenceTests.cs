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

        var builder = new NpgsqlConnectionStringBuilder(connectionString);

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
    public async Task ArchiveArticleUseCase_下書き記事の非公開化を拒否しDBの状態を維持する()
    {
        var article = new Article(
            "下書きアーカイブ拒否テスト",
            "下書きの記事本文");

        try
        {
            // 1. 下書きを保存
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                await repository.AddAsync(article);
            }

            DateTimeOffset persistedCreatedAt;

            // 2. DBから下書きを読み直す
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                var before = await repository.GetByIdAsync(article.Id);

                Assert.NotNull(before);
                Assert.Equal(ArticleStatus.Draft, before.Status);
                Assert.Null(before.PublishedAt);
                persistedCreatedAt = before.CreatedAt;
            }

            // 3. 下書きの非公開化は拒否される
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                var useCase = new ArchiveArticleUseCase(repository);

                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => useCase.ExecuteAsync(article.Id));
            }

            // 4. 別のDbContextから確認しても下書きのまま
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                var unchanged = await repository.GetByIdAsync(article.Id);

                Assert.NotNull(unchanged);
                Assert.Equal(ArticleStatus.Draft, unchanged.Status);
                Assert.Equal("下書きアーカイブ拒否テスト", unchanged.Title.Value);
                Assert.Equal("下書きの記事本文", unchanged.Content);
                Assert.Equal(article.Id, unchanged.Id);
                Assert.Equal(persistedCreatedAt, unchanged.CreatedAt);
                Assert.Null(unchanged.PublishedAt);
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
            // 1. 公開済み記事を保存
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                await repository.AddAsync(article);
            }

            DateTimeOffset persistedCreatedAt;
            DateTimeOffset? persistedPublishedAt;

            // 2. 保存済みの日付を取得
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                var before = await repository.GetByIdAsync(article.Id);

                Assert.NotNull(before);
                Assert.Equal(ArticleStatus.Published, before.Status);
                Assert.NotNull(before.PublishedAt);

                persistedCreatedAt = before.CreatedAt;
                persistedPublishedAt = before.PublishedAt;
            }

            // 3. 公開済み記事を非公開にする
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                var useCase = new ArchiveArticleUseCase(repository);

                var result = await useCase.ExecuteAsync(article.Id);
                Assert.True(result);
            }

            // 4. 別のDbContextから非公開状態を確認
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                var archived = await repository.GetByIdAsync(article.Id);

                Assert.NotNull(archived);
                Assert.Equal(ArticleStatus.Archived, archived.Status);
                Assert.Equal("公開済みアーカイブテスト", archived.Title.Value);
                Assert.Equal("公開済みの記事本文", archived.Content);
                Assert.Equal(article.Id, archived.Id);
                Assert.Equal(persistedCreatedAt, archived.CreatedAt);
                Assert.Equal(persistedPublishedAt, archived.PublishedAt);
            }
        }
        finally
        {
            await CleanupAsync(article.Id);
        }
    }

    [Fact]
    public async Task ArchiveArticleUseCase_非公開記事を編集して再公開した状態がDBに保存される()
    {
        var article = new Article(
            "再公開前のタイトル",
            "再公開前の本文");

        article.Publish();

        try
        {
            // 1. 公開記事を保存
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                await repository.AddAsync(article);
            }

            DateTimeOffset persistedCreatedAt;
            DateTimeOffset? firstPublishedAt;

            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                var published = await repository.GetByIdAsync(article.Id);

                Assert.NotNull(published);
                Assert.Equal(ArticleStatus.Published, published.Status);
                Assert.NotNull(published.PublishedAt);
                persistedCreatedAt = published.CreatedAt;
                firstPublishedAt = published.PublishedAt;
            }

            // 2. UseCase経由で非公開にする
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                var archiveUseCase = new ArchiveArticleUseCase(repository);

                Assert.True(await archiveUseCase.ExecuteAsync(article.Id));
            }

            // 3. 非公開状態をDBから読み出して編集・保存
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                var archived = await repository.GetByIdAsync(article.Id);

                Assert.NotNull(archived);
                Assert.Equal(ArticleStatus.Archived, archived.Status);

                archived.EditDraft("再公開後のタイトル", "再公開後の本文");
                await repository.UpdateAsync(archived);
            }

            // 4. 保存した内容と非公開状態を確認し、再公開・保存
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                var edited = await repository.GetByIdAsync(article.Id);

                Assert.NotNull(edited);
                Assert.Equal(ArticleStatus.Archived, edited.Status);
                Assert.Equal("再公開後のタイトル", edited.Title.Value);
                Assert.Equal("再公開後の本文", edited.Content);
                Assert.Equal(firstPublishedAt, edited.PublishedAt);

                edited.Publish();
                await repository.UpdateAsync(edited);
            }

            // 5. 別のDbContextから再公開後の永続化結果を検証
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);
                var republished = await repository.GetByIdAsync(article.Id);

                Assert.NotNull(republished);
                Assert.Equal(ArticleStatus.Published, republished.Status);
                Assert.Equal("再公開後のタイトル", republished.Title.Value);
                Assert.Equal("再公開後の本文", republished.Content);
                Assert.Equal(article.Id, republished.Id);
                Assert.Equal(persistedCreatedAt, republished.CreatedAt);
                Assert.NotNull(republished.PublishedAt);
                Assert.True(republished.PublishedAt >= firstPublishedAt);
            }
        }
        finally
        {
            await CleanupAsync(article.Id);
        }
    }
}
