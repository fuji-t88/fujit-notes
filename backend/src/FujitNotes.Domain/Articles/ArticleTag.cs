
namespace FujitNotes.Domain.Articles;

public sealed record ArticleTag
{
    public const int MaxLength = 50;

    public string Value { get; }

    public ArticleTag(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "タグは空にできません。",
                nameof(value));
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
        {
            throw new ArgumentException(
                $"タグは{MaxLength}文字以内です。",
                nameof(value));
        }

        Value = normalizedValue;
    }

    public override string ToString()
    {
        return Value;
    }
}
