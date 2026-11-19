
using FujitNotes.Domain.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FujitNotes.Infrastructure.Persistence.Configurations;

public sealed class ArticleConfiguration
    : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("articles");

        builder.HasKey(article => article.Id);

        builder.Property(article => article.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(article => article.Title)
            .HasConversion(
                title => title.Value,
                value => new ArticleTitle(value))
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(article => article.Content)
            .HasColumnName("content")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(article => article.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(article => article.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(article => article.PublishedAt)
            .HasColumnName("published_at")
            .HasColumnType("timestamp with time zone");

        // カテゴリは文字列として保存する
        builder.Property(article => article.Category)
            .HasConversion(
                category => category == null
                    ? null
                    : category.Value,
                value => value == null
                    ? null
                    : new ArticleCategory(value))
            .HasColumnName("category")
            .HasMaxLength(50)
            .IsRequired(false);

        // ArticleTagのコレクションを
        // PostgreSQLのtext[]に変換する
        var tagsConverter =
            new ValueConverter<List<ArticleTag>, string[]>(
                tags => tags.Select(tag => tag.Value).ToArray(),
                values => values
                    .Select(value => new ArticleTag(value))
                    .ToList());

        // コレクションの内容による変更検知
        var tagsComparer =
            new ValueComparer<List<ArticleTag>>(
                (left, right) =>
                    left!.Select(tag => tag.Value)
                        .SequenceEqual(
                            right!.Select(tag => tag.Value)),
                tags => tags.Aggregate(
                    0,
                    (hash, tag) =>
                        HashCode.Combine(hash, tag.Value)),
                tags => tags.ToList());

        // 公開プロパティは読み取り専用のため、
        // 内部フィールドを直接マッピングする
        builder.Ignore(article => article.Tags);

        builder.Property<List<ArticleTag>>("_tags")
            .HasConversion(tagsConverter)
            .HasColumnName("tags")
            .HasColumnType("text[]")
            .IsRequired()
            .Metadata.SetValueComparer(tagsComparer);
    }
}
