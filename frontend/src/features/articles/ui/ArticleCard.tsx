
import Link from "next/link";
import type { Article } from "../domain/Article";

interface ArticleCardProps {
  article: Article;
}

export function ArticleCard({
  article,
}: ArticleCardProps) {
  return (
    <article className="rounded-xl border border-orange-100 bg-white p-6 shadow-sm transition-shadow hover:shadow-md">
      <div className="flex h-full flex-col gap-4">
        {/* カテゴリ */}
        <span className="text-sm font-semibold text-orange-600">
          {article.category}
        </span>

        {/* 記事タイトル */}
        <h2 className="text-xl font-bold text-[#292929]">
          <Link
            href={`/articles/${article.slug}`}
            className="transition-colors hover:text-orange-600"
          >
            {article.title}
          </Link>
        </h2>

        {/* 記事の概要 */}
        <p className="text-sm leading-relaxed text-gray-600">
          {article.excerpt}
        </p>

        {/* タグ */}
        <div className="flex flex-wrap gap-2">
          {article.tags.map((tag) => (
            <span
              key={tag}
              className="rounded-full bg-orange-50 px-3 py-1 text-xs text-orange-700"
            >
              {tag}
            </span>
          ))}
        </div>

        {/* 記事詳細へのリンク */}
        <Link
          href={`/articles/${article.slug}`}
          className="mt-auto pt-3 text-sm font-medium text-orange-600 hover:underline"
        >
          記事を読む →
        </Link>
      </div>
    </article>
  );
}
