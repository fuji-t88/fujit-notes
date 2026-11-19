
import Link from "next/link";
import { notFound } from "next/navigation";
import { Suspense } from "react";

import { getPublicArticleById } from "../../../features/articles/api/publicArticlesApi";
import { toArticle } from "../../../features/articles/api/articleMapper";

interface ArticlePageProps {
  params: Promise<{
    slug: string;
  }>;
}

// 記事詳細ページ
export default function ArticleDetailPage({
  params,
}: ArticlePageProps) {
  return (
    <Suspense fallback={<ArticleLoading />}>
      <ArticleContent params={params} />
    </Suspense>
  );
}

// 読み込み中の表示
function ArticleLoading() {
  return (
    <div className="mx-auto max-w-3xl px-6 py-16">
      <p className="text-gray-500">
        記事を読み込んでいます...
      </p>
    </div>
  );
}

// 記事詳細表示
async function ArticleContent({
  params,
}: ArticlePageProps) {
  const { slug } = await params;

  // 現在は記事のUUIDをslugとして使用する
  const dto = await getPublicArticleById(slug);

  if (!dto) {
    notFound();
  }

  const article = toArticle(dto);

  return (
    <article className="mx-auto max-w-3xl px-6 py-16">
      {/* 記事一覧へ戻る */}
      <Link
        href="/articles"
        className="text-sm text-orange-600 hover:underline"
      >
        ← 記事一覧へ戻る
      </Link>

      {/* 記事タイトル・カテゴリ・タグ */}
      <header className="mt-8 border-b border-orange-100 pb-8">
        {/* カテゴリがある場合のみ表示 */}
        {article.category && (
          <p className="font-semibold text-orange-600">
            {article.category}
          </p>
        )}

        {/* 記事タイトル */}
        <h1
          className={`text-4xl font-extrabold leading-tight text-[#292929] ${
            article.category ? "mt-4" : ""
          }`}
        >
          {article.title}
        </h1>

        {/* 記事概要 */}
        <p className="mt-5 leading-relaxed text-gray-600">
          {article.excerpt}
        </p>

        {/* タグがある場合のみ表示 */}
        {article.tags.length > 0 && (
          <div className="mt-6 flex flex-wrap gap-2">
            {article.tags.map((tag) => (
              <span
                key={tag}
                className="rounded-full bg-orange-50 px-3 py-1 text-sm text-orange-700"
              >
                #{tag}
              </span>
            ))}
          </div>
        )}
      </header>

      {/* 記事本文 */}
      <section className="mt-10">
        <div className="whitespace-pre-wrap break-words leading-8 text-gray-700">
          {dto.content}
        </div>
      </section>

      {/* 記事一覧へ戻る */}
      <div className="mt-14 border-t border-orange-100 pt-8">
        <Link
          href="/articles"
          className="font-medium text-orange-600 hover:underline"
        >
          ← 記事一覧へ戻る
        </Link>
      </div>
    </article>
  );
}
