
import type { Article } from "../domain/Article";
import type { PublicArticleDto } from "./publicArticlesApi";

import { getArticleExcerpt } from "../lib/articleContent";

export function toArticle(
  dto: PublicArticleDto,
): Article {
  // プレーンテキストとTiptap HTMLの両方に対応
  const excerpt = getArticleExcerpt(dto.content);

  return {
    id: dto.id,
    title: dto.title,

    // 現在はUUIDを記事URLに使用
    slug: dto.id,

    excerpt,

    category: dto.category,
    tags: dto.tags,
  };
}
