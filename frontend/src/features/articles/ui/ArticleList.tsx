
import type { Article } from "../domain/Article";
import { ArticleCard } from "./ArticleCard";

interface ArticleListProps {
  articles: Article[];
}

export function ArticleList({
  articles,
}: ArticleListProps) {
  if (articles.length === 0) {
    return (
      <p className="py-12 text-center text-gray-500">
        まだ記事がありません。
      </p>
    );
  }

  return (
    <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
      {articles.map((article) => (
        <ArticleCard
          key={article.id}
          article={article}
        />
      ))}
    </div>
  );
}
