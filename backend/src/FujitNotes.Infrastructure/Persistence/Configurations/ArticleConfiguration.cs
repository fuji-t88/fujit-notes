
using FujitNotes.Domain.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FujitNotes.Infrastructure.Persistence.Configurations;

public sealed class ArticleConfiguration
    : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        // テーブル名
        builder.ToTable("articles");

        // 主キー
        builder.HasKey(article => article.Id);

        builder.Property(article => article.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        // ArticleTitleはDBでは文字列として保存
        builder.Property(article => article.Title)
            .HasConversion(
                title => title.Value,
                value => new ArticleTitle(value))
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        // 記事本文
        builder.Property(article => article.Content)
            .HasColumnName("content")
            .HasColumnType("text")
            .IsRequired();

        // 記事ステータス
        builder.Property(article => article.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        // 作成日時
        builder.Property(article => article.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // 公開日時
        builder.Property(article => article.PublishedAt)
            .HasColumnName("published_at")
            .HasColumnType("timestamp with time zone");
    }
}
