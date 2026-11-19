
import { Suspense } from "react";
import Link from "next/link";
import { notFound } from "next/navigation";
import type { Metadata } from "next";

import {
  getPublicArticleById,
} from "@/features/articles/api/publicArticlesApi";

import {
  toArticle,
} from "@/features/articles/api/articleMapper";

interface ArticlePageProps {
  params: Promise<{
    slug: string;
  }>;
}

// 記事ごとのSEO情報を作成する
export async function generateMetadata({
  params,
}: ArticlePageProps): Promise<Metadata> {
  const { slug } = await params;

  const dto = await getPublicArticleById(slug);

  if (!dto) {
    return {
      title: "記事が見つかりません | Fujit Notes",
    };
  }

  const article = toArticle(dto);

  return {
    title: `${article.title} | Fujit Notes`,
    description: article.excerpt,
  };
}

// ページ本体
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

  // slugには暫定的に記事Guidが入る
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

      {/* 記事タイトル・カテゴリ */}
      <header className="mt-8 border-b border-orange-100 pb-8">
        <p className="font-semibold text-orange-600">
          {article.category}
        </p>

        <h1 className="mt-4 text-4xl font-extrabold leading-tight text-[#292929]">
          {article.title}
        </h1>

        <p className="mt-5 leading-relaxed text-gray-600">
          {article.excerpt}
        </p>

        {/* タグ */}
        {article.tags.length > 0 && (
          <div className="mt-6 flex flex-wrap gap-2">
            {article.tags.map((tag) => (
              <span
                key={tag}
                className="rounded-full bg-orange-50 px-3 py-1 text-sm text-orange-700"
              >
                {tag}
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
