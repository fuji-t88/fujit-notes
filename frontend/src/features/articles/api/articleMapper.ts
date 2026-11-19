
import type { Article } from "../domain/Article";
import type { PublicArticleDto } from "./publicArticlesApi";

export function toArticle(dto: PublicArticleDto): Article {
  // APIには概要フィールドがないため、
  // 本文の先頭140文字を概要として使用する
  const plainText = dto.content
    .replace(/\s+/g, " ")
    .trim();

  const excerpt =
    plainText.length > 140
      ? `${plainText.slice(0, 140)}…`
      : plainText;

  return {
    id: dto.id,
    title: dto.title,

    // 現在は記事のUUIDをURLに使用する
    slug: dto.id,

    excerpt,

    // ASP.NET Core APIからカテゴリ・タグを引き継ぐ
    category: dto.category,
    tags: dto.tags,
  };
}
