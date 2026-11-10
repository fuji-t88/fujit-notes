
using FujitNotes.Domain.Articles;

namespace FujitNotes.Domain.Tests.Articles;

public class ArticleTests
{
    [Fact]
    public void Constructor_ValidInput_CreatesDraftArticle()
    {
        var article = new Article(
            "初めての技術記事",
            "ASP.NET Coreを学習しています。");

        Assert.NotEqual(Guid.Empty, article.Id);
        Assert.Equal("初めての技術記事", article.Title.Value);
        Assert.Equal(
            ArticleStatus.Draft,
            article.Status);
        Assert.Null(article.PublishedAt);
        Assert.True(article.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_InvalidTitle_ThrowsArgumentException(
        string title)
    {
        Assert.Throws<ArgumentException>(() =>
            new Article(title, "本文"));
    }

    [Fact]
    public void ChangeTitle_ValidTitle_UpdatesTitle()
    {
        var article = new Article("変更前", "本文");

        article.ChangeTitle("変更後");

        Assert.Equal("変更後", article.Title.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ChangeTitle_InvalidTitle_ThrowsArgumentException(
        string newTitle)
    {
        var article = new Article("元のタイトル", "本文");

        Assert.Throws<ArgumentException>(() =>
            article.ChangeTitle(newTitle));
    }

    [Fact]
    public void Publish_ValidArticle_ChangesStatusAndPublishedAt()
    {
        var article = new Article(
            "公開する記事",
            "公開できる本文");

        article.Publish();

        Assert.Equal(
            ArticleStatus.Published,
            article.Status);
        Assert.NotNull(article.PublishedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Publish_EmptyContent_ThrowsInvalidOperationException(
        string content)
    {
        var article = new Article("タイトル", content);

        Assert.Throws<InvalidOperationException>(() =>
            article.Publish());

        Assert.Equal(ArticleStatus.Draft, article.Status);
    }

    [Fact]
    public void Publish_AlreadyPublished_ThrowsInvalidOperationException()
    {
        var article = new Article("タイトル", "本文");
        article.Publish();

        Assert.Throws<InvalidOperationException>(() =>
            article.Publish());
    }

    [Fact]
    public void Archive_DraftArticle_ChangesStatusToArchived()
    {
        var article = new Article("タイトル", "本文");

        article.Archive();

        Assert.Equal(
            ArticleStatus.Archived,
            article.Status);
    }

    [Fact]
    public void ChangeTitle_ArchivedArticle_ThrowsInvalidOperationException()
    {
        var article = new Article("タイトル", "本文");
        article.Archive();

        Assert.Throws<InvalidOperationException>(() =>
            article.ChangeTitle("変更後"));
    }

    [Fact]
    public void Publish_ArchivedArticle_ThrowsInvalidOperationException()
    {
        var article = new Article("タイトル", "本文");
        article.Archive();

        Assert.Throws<InvalidOperationException>(() =>
            article.Publish());
    }
}
