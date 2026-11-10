
import Link from "next/link";

interface ArticlePageProps {
  params: Promise<{
    slug: string;
  }>;
}

// ビルド時に生成する記事URLを定義する
export function generateStaticParams() {
  return [
    { slug: "nextjs-typescript-blog" },
    { slug: "why-build-fujit-notes" },
    { slug: "aspnet-core-docker" },
    { slug: "marathon-challenge" },
  ];
}

// 記事詳細ページ
export default async function ArticleDetailPage({
  params,
}: ArticlePageProps) {
  const { slug } = await params;

  return (
    <article className="mx-auto max-w-3xl px-6 py-16">
      <p className="mb-4 font-semibold text-orange-600">
        Fujit Notes / Article
      </p>

      <h1 className="text-3xl font-bold text-[#292929]">
        記事詳細ページ
      </h1>

      <p className="mt-6 text-gray-600">
        Slug: {slug}
      </p>

      <Link
        href="/articles"
        className="mt-8 inline-block text-orange-600 hover:underline"
      >
        ← 記事一覧へ戻る
      </Link>
    </article>
  );
}
