using FujitNotes.Domain.Articles;

namespace FujitNotes.Domain.Tests.Articles;

public class ArticleMetadataTests
{
    [Fact]
    public void Constructor_CategoryAndTagsAreEmpty()
    {
        var article = new Article("タイトル", "本文");

        Assert.Null(article.Category);
        Assert.Empty(article.Tags);
    }

    [Fact]
    public void ChangeCategory_ValidCategory_UpdatesCategory()
    {
        var article = new Article("タイトル", "本文");

        article.ChangeCategory("Development");

        Assert.Equal("Development", article.Category?.Value);
    }

    [Fact]
    public void ChangeCategory_Null_ClearsCategory()
    {
        var article = new Article("タイトル", "本文");
        article.ChangeCategory("Development");

        article.ChangeCategory(null);

        Assert.Null(article.Category);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Unknown")]
    public void ChangeCategory_InvalidCategory_DoesNotChangeState(
        string category)
    {
        var article = new Article("タイトル", "本文");
        article.ChangeCategory("Development");

        Assert.Throws<ArgumentException>(() =>
            article.ChangeCategory(category));

        Assert.Equal("Development", article.Category?.Value);
    }

    [Fact]
    public void ChangeTags_ValidTags_UpdatesTags()
    {
        var article = new Article("タイトル", "本文");

        article.ChangeTags(new[] { "C#", "DDD", "PostgreSQL" });

        Assert.Equal(
            new[] { "C#", "DDD", "PostgreSQL" },
            article.Tags.Select(tag => tag.Value));
    }

    [Fact]
    public void ChangeTags_Whitespace_TrimsTags()
    {
        var article = new Article("タイトル", "本文");

        article.ChangeTags(new[] { " DDD " });

        Assert.Equal("DDD", Assert.Single(article.Tags).Value);
    }

    [Fact]
    public void ChangeTags_Duplicates_DoesNotChangeExistingTags()
    {
        var article = new Article("タイトル", "本文");
        article.ChangeTags(new[] { "C#" });

        Assert.Throws<ArgumentException>(() =>
            article.ChangeTags(new[] { "DDD", "ddd" }));

        Assert.Equal("C#", Assert.Single(article.Tags).Value);
    }

    [Fact]
    public void ChangeTags_TooManyTags_ThrowsArgumentException()
    {
        var article = new Article("タイトル", "本文");

        var tags = Enumerable.Range(1, 11)
            .Select(number => $"Tag{number}");

        Assert.Throws<ArgumentException>(() =>
            article.ChangeTags(tags));

        Assert.Empty(article.Tags);
    }

    [Fact]
    public void ChangeTags_TagTooLong_ThrowsArgumentException()
    {
        var article = new Article("タイトル", "本文");

        Assert.Throws<ArgumentException>(() =>
            article.ChangeTags(new[] { new string('a', 51) }));

        Assert.Empty(article.Tags);
    }

    [Fact]
    public void ChangeTags_EmptyCollection_ClearsTags()
    {
        var article = new Article("タイトル", "本文");
        article.ChangeTags(new[] { "C#", "DDD" });

        article.ChangeTags(Array.Empty<string>());

        Assert.Empty(article.Tags);
    }

    [Fact]
    public void ChangeTags_Null_ThrowsArgumentNullException()
    {
        var article = new Article("タイトル", "本文");

        Assert.Throws<ArgumentNullException>(() =>
            article.ChangeTags(null!));
    }

    [Fact]
    public void ChangeMetadata_PublishedArticle_Throws()
    {
        var article = new Article("タイトル", "本文");
        article.Publish();

        Assert.Throws<InvalidOperationException>(() =>
            article.ChangeCategory("Development"));

        Assert.Throws<InvalidOperationException>(() =>
            article.ChangeTags(new[] { "DDD" }));
    }

    [Fact]
    public void ChangeMetadata_PrivateArticle_AllowsChanges()
    {
        var article = new Article("タイトル", "本文");
        article.Publish();
        article.Archive();

        article.ChangeCategory("Development");
        article.ChangeTags(new[] { "DDD", "C#" });

        Assert.Equal("Development", article.Category?.Value);
        Assert.Equal(
            new[] { "DDD", "C#" },
            article.Tags.Select(tag => tag.Value));
        Assert.Equal(ArticleStatus.Archived, article.Status);
    }


    [Fact]
    public void ArticleTag_SameValue_HasValueEquality()
    {
        var first = new ArticleTag("DDD");
        var second = new ArticleTag("DDD");

        Assert.Equal(first, second);
    }
}
