
namespace FujitNotes.Domain.Articles;

public class Article
{
    private List<ArticleTag> _tags = new();

    public Guid Id { get; private set; }

    public ArticleTitle Title { get; private set; } = null!;

    public string Content { get; private set; } = string.Empty;

    public ArticleStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public ArticleCategory? Category { get; private set; }

    public IReadOnlyList<ArticleTag> Tags => _tags.AsReadOnly();

    // EF Coreによるデータ復元用
    private Article()
    {
    }

    // 記事を新規作成する
    public Article(string title, string content)
    {
        Id = Guid.NewGuid();
        Title = new ArticleTitle(title);

        Content = content
            ?? throw new ArgumentException(
                "本文にnullは指定できません。",
                nameof(content));

        Status = ArticleStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
        PublishedAt = null;
        Category = null;
    }

    // タイトルを変更する
    public void ChangeTitle(string newTitle)
    {
        EnsureEditable();

        Title = new ArticleTitle(newTitle);
    }

    // 下書き・非公開記事のタイトルと本文を編集する
    // 既存ユースケースとの互換性のためメソッド名を維持
    public void EditDraft(
        string newTitle,
        string newContent)
    {
        EnsureEditable();

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

    // カテゴリを変更する
    public void ChangeCategory(string? newCategory)
    {
        EnsureEditable();

        var validatedCategory = newCategory is null
            ? null
            : new ArticleCategory(newCategory);

        Category = validatedCategory;
    }

    // タグをまとめて変更する
    public void ChangeTags(IEnumerable<string> newTags)
    {
        EnsureEditable();

        ArgumentNullException.ThrowIfNull(newTags);

        var validatedTags = newTags
            .Select(value => new ArticleTag(value))
            .ToList();

        if (validatedTags.Count > 10)
        {
            throw new ArgumentException(
                "タグは最大10件までです。",
                nameof(newTags));
        }

        var distinctCount = validatedTags
            .Select(tag => tag.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        if (distinctCount != validatedTags.Count)
        {
            throw new ArgumentException(
                "重複するタグは指定できません。",
                nameof(newTags));
        }

        _tags.Clear();
        _tags.AddRange(validatedTags);
    }

    // 下書き・非公開記事を公開する
    public void Publish()
    {
        if (Status == ArticleStatus.Published)
        {
            throw new InvalidOperationException(
                "すでに公開済みの記事です。");
        }

        if (Status != ArticleStatus.Draft &&
            Status != ArticleStatus.Archived)
        {
            throw new InvalidOperationException(
                "この記事は公開できません。");
        }

        if (string.IsNullOrWhiteSpace(Content))
        {
            throw new InvalidOperationException(
                "本文が空の記事は公開できません。");
        }

        Status = ArticleStatus.Published;

        // 再公開時も公開日時を更新する
        PublishedAt = DateTimeOffset.UtcNow;
    }

    // 公開済み記事を非公開にする
    // 既存API・DBとの互換性のためメソッド名を維持
    public void Archive()
    {
        if (Status != ArticleStatus.Published)
        {
            throw new InvalidOperationException(
                "公開済みの記事のみ非公開にできます。");
        }

        Status = ArticleStatus.Archived;

        // 非公開にした時点では最終公開日時を保持する
    }

    // 下書き・非公開記事のみ編集可能
    private void EnsureEditable()
    {
        if (Status != ArticleStatus.Draft &&
            Status != ArticleStatus.Archived)
        {
            throw new InvalidOperationException(
                "下書きまたは非公開の記事のみ編集できます。");
        }
    }
}
