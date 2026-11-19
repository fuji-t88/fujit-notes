
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Domain.Articles;
using FujitNotes.Infrastructure.Articles.Repositories;
using FujitNotes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace FujitNotes.Infrastructure.Tests.Articles;

public class ArticleEditingPersistenceTests
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
            new NpgsqlConnectionStringBuilder(
                connectionString);

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

    [Fact]
    public async Task UpdateArticleUseCase_編集内容をDBに永続化できる()
    {
        var article = new Article(
            "編集前のタイトル",
            "編集前の本文");

        var originalId = article.Id;

        // DBに保存された作成日時を保持する
        DateTimeOffset persistedCreatedAt = default;

        try
        {
            // 1. PostgreSQLに記事を新規保存
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                await repository.AddAsync(article);
            }

            // 2. DBから編集前の記事を取得
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                var before = await repository.GetByIdAsync(
                    article.Id);

                Assert.NotNull(before);

                Assert.Equal(
                    "編集前のタイトル",
                    before.Title.Value);

                Assert.Equal(
                    "編集前の本文",
                    before.Content);

                // DB保存後の日時を比較基準にする
                persistedCreatedAt = before.CreatedAt;
            }

            // 3. 別のDbContextで記事を編集
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                var useCase = new UpdateArticleUseCase(
                    repository);

                var result = await useCase.ExecuteAsync(
                    article.Id,
                    "編集後のタイトル",
                    "編集後の本文");

                Assert.True(result);
            }

            // 4. 新しいDbContextでDBから再取得
            await using (var db = CreateDbContext())
            {
                var repository = new EfArticleRepository(db);

                var updated = await repository.GetByIdAsync(
                    article.Id);

                Assert.NotNull(updated);

                // タイトルと本文の永続化
                Assert.Equal(
                    "編集後のタイトル",
                    updated.Title.Value);

                Assert.Equal(
                    "編集後の本文",
                    updated.Content);

                // 記事IDは維持されている
                Assert.Equal(
                    originalId,
                    updated.Id);

                // 編集前後でDB上の作成日時が同じ
                Assert.Equal(
                    persistedCreatedAt,
                    updated.CreatedAt);

                // 公開状態は変更されていない
                Assert.Equal(
                    ArticleStatus.Draft,
                    updated.Status);

                Assert.Null(updated.PublishedAt);
            }
        }
        finally
        {
            // 今回作成した記事だけ削除
            await using var db = CreateDbContext();

            var saved = await db.Articles.FindAsync(
                article.Id);

            if (saved is not null)
            {
                db.Articles.Remove(saved);
                await db.SaveChangesAsync();
            }
        }
    }
}
