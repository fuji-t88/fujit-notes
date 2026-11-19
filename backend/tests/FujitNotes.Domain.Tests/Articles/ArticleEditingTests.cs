
using FujitNotes.Domain.Articles;
using Xunit;

namespace FujitNotes.Domain.Tests.Articles;

public class ArticleEditingTests
{
    [Fact]
    public void EditDraft_下書きのタイトルと本文を変更できる()
    {
        var article = new Article(
            "変更前タイトル",
            "変更前本文");

        article.EditDraft(
            "変更後タイトル",
            "変更後本文");

        Assert.Equal("変更後タイトル", article.Title.Value);
        Assert.Equal("変更後本文", article.Content);
        Assert.Equal(ArticleStatus.Draft, article.Status);
        Assert.Null(article.PublishedAt);
    }

    [Fact]
    public void EditDraft_下書きの本文を空にできる()
    {
        var article = new Article(
            "タイトル",
            "変更前本文");

        article.EditDraft("タイトル", "");

        Assert.Equal("", article.Content);
    }

    [Fact]
    public void EditDraft_公開済み記事は編集できない()
    {
        var article = new Article(
            "公開記事",
            "公開本文");

        article.Publish();

        Assert.Throws<InvalidOperationException>(() =>
            article.EditDraft("変更後", "変更後本文"));

        Assert.Equal("公開記事", article.Title.Value);
        Assert.Equal("公開本文", article.Content);
    }

    [Fact]
    public void EditDraft_アーカイブ済み記事は編集できない()
    {
        var article = new Article(
            "保存記事",
            "本文");

        article.Archive();

        Assert.Throws<InvalidOperationException>(() =>
            article.EditDraft("変更後", "変更後本文"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void EditDraft_不正なタイトルは拒否する(
        string invalidTitle)
    {
        var article = new Article(
            "元のタイトル",
            "元の本文");

        Assert.Throws<ArgumentException>(() =>
            article.EditDraft(invalidTitle, "変更後本文"));

        Assert.Equal("元のタイトル", article.Title.Value);
        Assert.Equal("元の本文", article.Content);
    }

    [Fact]
    public void EditDraft_200文字を超えるタイトルは拒否する()
    {
        var article = new Article("元のタイトル", "元の本文");
        var longTitle = new string('A', 201);

        Assert.Throws<ArgumentException>(() =>
            article.EditDraft(longTitle, "変更後本文"));

        Assert.Equal("元のタイトル", article.Title.Value);
        Assert.Equal("元の本文", article.Content);
    }

    [Fact]
    public void EditDraft_Null本文は拒否し変更を残さない()
    {
        var article = new Article("元のタイトル", "元の本文");

        Assert.Throws<ArgumentException>(() =>
            article.EditDraft("変更後", null!));

        Assert.Equal("元のタイトル", article.Title.Value);
        Assert.Equal("元の本文", article.Content);
    }

    [Fact]
    public void EditDraft_IDと作成日時は維持される()
    {
        var article = new Article("変更前", "本文");

        var originalId = article.Id;
        var originalCreatedAt = article.CreatedAt;

        article.EditDraft("変更後", "新しい本文");

        Assert.Equal(originalId, article.Id);
        Assert.Equal(originalCreatedAt, article.CreatedAt);
    }
}
