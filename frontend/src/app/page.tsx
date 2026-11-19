
import { Suspense } from "react";
import Link from "next/link";

import {
  getPublicArticles,
} from "@/features/articles/api/publicArticlesApi";

import {
  toArticle,
} from "@/features/articles/api/articleMapper";

import {
  ArticleList,
} from "@/features/articles/ui/ArticleList";

const categories = [
  "Development",
  "Cloud & DevOps",
  "Career & Learning",
  "Build in Public",
  "Life & Fitness",
];

export default function Home() {
  return (
    <main>
      {/* メインビジュアル */}
      <section className="mx-auto max-w-6xl px-6 py-20">
        <p className="mb-4 font-semibold text-orange-600">
          ENGINEERING · GROWTH · LIFE
        </p>

        <h1 className="text-5xl font-extrabold tracking-tight">
          Build. Learn.{" "}
          <span className="text-orange-500">
            Grow.
          </span>
        </h1>

        <p className="mt-6 max-w-2xl text-lg leading-relaxed text-gray-600">
          技術を学び、挑戦を記録し、人生をアップデートする。
          エンジニアの成長ブログ。
        </p>

        {/* カテゴリ一覧 */}
        <div className="mt-10 flex flex-wrap gap-3">
          {categories.map((category) => (
            <span
              key={category}
              className="rounded-full border border-orange-200 bg-white px-4 py-2 text-sm"
            >
              {category}
            </span>
          ))}
        </div>
      </section>

      {/* 最新記事 */}
      <section className="border-t border-orange-100 bg-orange-50/30">
        <div className="mx-auto max-w-6xl px-6 py-16">
          <div className="mb-10 flex flex-wrap items-end justify-between gap-4">
            <div>
              <p className="mb-3 font-semibold text-orange-600">
                FUJIT NOTES
              </p>

              <h2 className="text-3xl font-bold text-[#292929]">
                Latest Articles
              </h2>

              <p className="mt-4 text-gray-600">
                最近公開した記事をご紹介します。
              </p>
            </div>

            <Link
              href="/articles"
              className="font-medium text-orange-600 hover:underline"
            >
              すべての記事を見る →
            </Link>
          </div>

          {/* API通信はSuspenseの内側で実行 */}
          <Suspense fallback={<LatestArticlesLoading />}>
            <LatestArticles />
          </Suspense>
        </div>
      </section>
    </main>
  );
}

// API通信中の表示
function LatestArticlesLoading() {
  return (
    <div
      role="status"
      className="rounded-xl border border-orange-100 bg-white p-6"
    >
      <p className="text-sm text-gray-600">
        最新記事を読み込んでいます...
      </p>
    </div>
  );
}

// 最新記事3件を取得して表示する
async function LatestArticles() {
  const publicArticles = await getPublicArticles();

  // APIは公開日時の新しい順で返す
  const latestArticles = publicArticles
    .slice(0, 3)
    .map(toArticle);

  if (latestArticles.length === 0) {
    return (
      <div className="rounded-xl border border-orange-100 bg-white p-6">
        <p className="text-gray-600">
          まだ公開されている記事はありません。
        </p>
      </div>
    );
  }

  return <ArticleList articles={latestArticles} />;
}
