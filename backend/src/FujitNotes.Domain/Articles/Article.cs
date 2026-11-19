
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

    // 既存のタイトル変更メソッド
    public void ChangeTitle(string newTitle)
    {
        if (Status == ArticleStatus.Archived)
        {
            throw new InvalidOperationException(
                "アーカイブ済みの記事は編集できません。");
        }

        Title = new ArticleTitle(newTitle);
    }

    // Step 18：下書き記事のタイトルと本文を編集
    public void EditDraft(string newTitle, string newContent)
    {
        if (Status != ArticleStatus.Draft)
        {
            throw new InvalidOperationException(
                "下書きの記事のみ編集できます。");
        }

        // 変更前にすべての入力を検証する
        var validatedTitle = new ArticleTitle(newTitle);

        if (newContent is null)
        {
            throw new ArgumentException(
                "本文にnullは指定できません。",
                nameof(newContent));
        }

        Title = validatedTitle;
        Content = newContent;
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
