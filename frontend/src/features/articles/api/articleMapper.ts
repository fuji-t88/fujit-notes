
import type { Article } from "../domain/Article";
import type { PublicArticleDto } from "./publicArticlesApi";

export function toArticle(
  dto: PublicArticleDto,
): Article {
  // 現在のAPIには概要フィールドがないため、
  // 本文の先頭140文字を暫定的に使用する
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

    // Step 21ではGuidをURLに使用する
    slug: dto.id,

    excerpt,

    // カテゴリ・タグはバックエンド対応後に取得する
    category: "Articles",
    tags: [],
  };
}
