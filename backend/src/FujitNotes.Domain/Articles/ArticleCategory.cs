
namespace FujitNotes.Domain.Articles;

public sealed record ArticleCategory
{
    private static readonly HashSet<string> AllowedCategories =
        new(StringComparer.Ordinal)
        {
            "Development",
            "Cloud & DevOps",
            "Career & Learning",
            "Build in Public",
            "Life & Fitness"
        };

    public string Value { get; }

    public ArticleCategory(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "カテゴリは空にできません。",
                nameof(value));
        }

        if (!AllowedCategories.Contains(value))
        {
            throw new ArgumentException(
                "指定されたカテゴリは使用できません。",
                nameof(value));
        }

        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}
