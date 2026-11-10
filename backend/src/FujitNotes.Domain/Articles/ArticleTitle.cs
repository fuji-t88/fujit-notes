
namespace FujitNotes.Domain.Articles;

public sealed record ArticleTitle
{
    public string Value { get; }

    public ArticleTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "記事タイトルは必須です。",
                nameof(value));
        }

        if (value.Length > 200)
        {
            throw new ArgumentException(
                "記事タイトルは200文字以内です。",
                nameof(value));
        }

        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}
