
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

    // Step 23：カテゴリとタグ
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
        if (Status == ArticleStatus.Archived)
        {
            throw new InvalidOperationException(
                "アーカイブ済みの記事は編集できません。");
        }

        Title = new ArticleTitle(newTitle);
    }

    // 下書き記事のタイトルと本文を編集する
    public void EditDraft(string newTitle, string newContent)
    {
        EnsureDraft();

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

    // Step 23：カテゴリを変更する
    public void ChangeCategory(string? newCategory)
    {
        EnsureDraft();

        // nullを指定した場合は未設定に戻す
        var validatedCategory = newCategory is null
            ? null
            : new ArticleCategory(newCategory);

        Category = validatedCategory;
    }

    // Step 23：タグをまとめて変更する
    public void ChangeTags(IEnumerable<string> newTags)
    {
        EnsureDraft();

        if (newTags is null)
        {
            throw new ArgumentNullException(nameof(newTags));
        }

        // 変更前にすべてのタグを検証する
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

        // 検証成功後に変更する
        _tags.Clear();
        _tags.AddRange(validatedTags);
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

    // 下書きかどうかを確認する共通処理
    private void EnsureDraft()
    {
        if (Status != ArticleStatus.Draft)
        {
            throw new InvalidOperationException(
                "下書きの記事のみ編集できます。");
        }
    }
}
