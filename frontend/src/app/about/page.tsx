
import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: "About | Fujit Notes",
  description:
    "Fujit Notesについて。技術・学習・挑戦を記録するエンジニアの成長ブログ。",
};

const topics = [
  {
    title: "Engineering",
    description:
      "C#、TypeScript、Web開発など、技術を学び、実践したことを記録します。",
  },
  {
    title: "Learning & Career",
    description:
      "英語学習、キャリア、個人開発など、日々の成長につながる挑戦を紹介します。",
  },
  {
    title: "Life & Fitness",
    description:
      "健康づくりやトレーニングなど、生活をより良くするための取り組みを記録します。",
  },
];

export default function AboutPage() {
  return (
    <section className="mx-auto max-w-6xl px-6 py-16">
      <div className="max-w-3xl">
        <p className="mb-3 font-semibold text-orange-600">
          FUJIT NOTES
        </p>

        <h1 className="text-4xl font-bold tracking-tight text-[#292929]">
          About
        </h1>

        <p className="mt-6 text-2xl font-bold">
          Build. Learn.{" "}
          <span className="text-orange-500">Grow.</span>
        </p>

        <p className="mt-6 leading-8 text-gray-600">
          Fujit Notesは、技術を学び、挑戦を記録し、
          人生をアップデートするためのエンジニアの成長ブログです。
        </p>

        <p className="mt-4 leading-8 text-gray-600">
          プログラミングやシステム設計を中心に、
          英語学習、キャリア、健康づくりなど、
          日々の学びと試行錯誤を発信していきます。
        </p>
      </div>

      <div className="mt-16">
        <h2 className="text-2xl font-bold text-[#292929]">
          What I Write
        </h2>

        <p className="mt-3 text-gray-600">
          Fujit Notesで扱う主なテーマです。
        </p>

        <div className="mt-8 grid gap-6 md:grid-cols-3">
          {topics.map((topic) => (
            <article
              key={topic.title}
              className="rounded-2xl border border-orange-100 bg-white p-6"
            >
              <h3 className="text-lg font-bold text-orange-600">
                {topic.title}
              </h3>

              <p className="mt-4 leading-7 text-gray-600">
                {topic.description}
              </p>
            </article>
          ))}
        </div>
      </div>

      <div className="mt-16 rounded-2xl border border-orange-100 bg-orange-50 p-8">
        <h2 className="text-2xl font-bold text-[#292929]">
          Keep Learning, Keep Growing.
        </h2>

        <p className="mt-4 leading-7 text-gray-600">
          完璧な答えだけでなく、そこに至るまでの考え方や
          設計判断も大切にして記録していきます。
        </p>

        <Link
          href="/articles"
          className="mt-8 inline-flex rounded-lg bg-orange-500 px-6 py-3 font-semibold text-white transition-colors hover:bg-orange-600"
        >
          記事一覧を見る
        </Link>
      </div>
    </section>
  );
}
