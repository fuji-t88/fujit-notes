
import { Suspense } from "react";
import type { Metadata } from "next";

import {
  getPublicArticles,
} from "@/features/articles/api/publicArticlesApi";

import {
  toArticle,
} from "@/features/articles/api/articleMapper";

import {
  ArticleList,
} from "@/features/articles/ui/ArticleList";

export const metadata: Metadata = {
  title: "Articles | Fujit Notes",
  description: "Fujit Notesの記事一覧ページ",
};

// ページ本体
export default function ArticlesPage() {
  return (
    <section className="mx-auto max-w-6xl px-6 py-16">
      <div className="mb-10">
        <p className="mb-3 font-semibold text-orange-600">
          FUJIT NOTES
        </p>

        <h1 className="text-4xl font-bold text-[#292929]">
          Articles
        </h1>

        <p className="mt-4 text-gray-600">
          技術・学習・キャリア・健康・挑戦の記録。
        </p>

        <p className="mt-2 text-sm text-gray-500">
          公開済みの記事を表示しています。
        </p>
      </div>

      {/* 非同期API通信をSuspenseで囲む */}
      <Suspense fallback={<ArticleListLoading />}>
        <PublicArticleList />
      </Suspense>
    </section>
  );
}

// API通信中の表示
function ArticleListLoading() {
  return (
    <div
      role="status"
      className="rounded-xl border border-orange-100 bg-orange-50 p-6"
    >
      <p className="text-sm text-gray-600">
        記事を読み込んでいます...
      </p>
    </div>
  );
}

// API通信を行うServer Component
async function PublicArticleList() {
  const publicArticles = await getPublicArticles();

  const articles = publicArticles.map(toArticle);

  return <ArticleList articles={articles} />;
}
