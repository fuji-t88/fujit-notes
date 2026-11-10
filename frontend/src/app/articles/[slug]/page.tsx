
import { Suspense } from "react";
import Link from "next/link";
import { notFound } from "next/navigation";
import type { Metadata } from "next";

import {
  sampleArticles,
  sampleArticleContents,
} from "@/features/articles/data/sampleArticles";

// Next.js 16ではparamsをPromiseとして受け取る
interface ArticlePageProps {
  params: Promise<{
    slug: string;
  }>;
}

// ビルド時に事前生成する記事URL
export function generateStaticParams() {
  return sampleArticles.map((article) => ({
    slug: article.slug,
  }));
}

// 記事ごとのSEO情報
export async function generateMetadata({
  params,
}: ArticlePageProps): Promise<Metadata> {
  const { slug } = await params;

  const article = sampleArticles.find(
    (item) => item.slug === slug
  );

  if (!article) {
    return {
      title: "記事が見つかりません | Fujit Notes",
    };
  }

  return {
    title: `${article.title} | Fujit Notes`,
    description: article.excerpt,
  };
}

// ページ本体：Suspenseで非同期処理を囲む
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

// 実際の記事表示処理
async function ArticleContent({
  params,
}: ArticlePageProps) {
  const { slug } = await params;

  // slugから記事を取得
  const article = sampleArticles.find(
    (item) => item.slug === slug
  );

  if (!article) {
    notFound();
  }

  // 記事本文を取得
  const content = sampleArticleContents[slug];

  if (!content) {
    notFound();
  }

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

        {/* 記事タグ */}
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
      </header>

      {/* 記事本文 */}
      <section className="mt-10 space-y-6">
        <h2 className="text-2xl font-bold text-[#292929]">
          {content.heading}
        </h2>

        {content.paragraphs.map((paragraph, index) => (
          <p
            key={index}
            className="leading-8 text-gray-700"
          >
            {paragraph}
          </p>
        ))}

        {/* 技術記事用コードブロック */}
        {content.code && (
          <pre className="overflow-x-auto rounded-xl bg-[#292929] p-5 text-sm text-white">
            <code>{content.code}</code>
          </pre>
        )}
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
