
import type { Metadata } from "next";
import { ArticleList } from "@/features/articles/ui/ArticleList";
import { sampleArticles } from "@/features/articles/data/sampleArticles";

export const metadata: Metadata = {
  title: "Articles | Fujit Notes",
  description: "Fujit Notesの記事一覧ページ",
};

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
          現在は開発用のサンプル記事を表示しています。
        </p>
      </div>

      <ArticleList articles={sampleArticles} />
    </section>
  );
}
