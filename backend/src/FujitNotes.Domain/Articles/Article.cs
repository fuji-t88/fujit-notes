
namespace FujitNotes.Domain.Articles;

public class Article
{
    public Guid Id { get; private set; }

    public ArticleTitle Title { get; private set; } = null!;

    public string Content { get; private set; } = string.Empty;

    public ArticleStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    // EF Coreによるデータ復元用
    private Article()
    {
    }

    // 記事を新規作成する
    public Article(string title, string content)
    {
        Id = Guid.NewGuid();
        Title = new ArticleTitle(title);
        Content = content;
        Status = ArticleStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
        PublishedAt = null;
    }

    // 記事タイトルを変更する
    public void ChangeTitle(string newTitle)
    {
        if (Status == ArticleStatus.Archived)
        {
            throw new InvalidOperationException(
                "アーカイブ済みの記事は編集できません。");
        }

        Title = new ArticleTitle(newTitle);
    }

    // 記事を公開する
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

    // 記事をアーカイブする
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
