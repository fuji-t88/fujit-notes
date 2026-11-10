
namespace FujitNotes.Domain.Articles;

public class Article
{
    public Guid Id { get; private set; }

    public ArticleTitle Title { get; private set; }

    public string Content { get; private set; }

    public ArticleStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public Article(string title, string content)
    {
        Id = Guid.NewGuid();
        Title = new ArticleTitle(title);
        Content = content;
        Status = ArticleStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
        PublishedAt = null;
    }

    public void ChangeTitle(string newTitle)
    {
        if (Status == ArticleStatus.Archived)
        {
            throw new InvalidOperationException(
                "アーカイブ済みの記事は編集できません。");
        }

        Title = new ArticleTitle(newTitle);
    }

    public void Publish()
    {
        if (Status == ArticleStatus.Archived)
        {
            throw new InvalidOperationException(
                "アーカイブ済みの記事は公開できません。");
        }

        if (string.IsNullOrWhiteSpace(Content))
        {
            throw new InvalidOperationException(
                "本文が空の記事は公開できません。");
        }

        if (Status == ArticleStatus.Published)
        {
            throw new InvalidOperationException(
                "すでに公開済みの記事です。");
        }

        Status = ArticleStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        if (Status == ArticleStatus.Archived)
        {
            throw new InvalidOperationException(
                "すでにアーカイブ済みの記事です。");
        }

        Status = ArticleStatus.Archived;
    }
}
