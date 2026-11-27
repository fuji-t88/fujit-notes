
import Link from "next/link";
import { notFound } from "next/navigation";
import { Suspense } from "react";
import sanitizeHtml from "sanitize-html";

import { getPublicArticleById } from "../../../features/articles/api/publicArticlesApi";
import { toArticle } from "../../../features/articles/api/articleMapper";
import {
  getRichTextHtml,
  isRichText,
} from "../../../features/articles/lib/articleContent";

interface ArticlePageProps {
  params: Promise<{
    slug: string;
  }>;
}

// 表示を許可するHTMLの要素と属性を限定する。
// 画像はアップロード機能の実装後に追加する。
const ALLOWED_TAGS = [
  "p",
  "br",
  "h1",
  "h2",
  "h3",
  "strong",
  "em",
  "s",
  "ul",
  "ol",
  "li",
  "blockquote",
  "pre",
  "code",
  "hr",
  "a",
];

function sanitizeArticleHtml(html: string): string {
  return sanitizeHtml(html, {
    allowedTags: ALLOWED_TAGS,

    allowedAttributes: {
      a: ["href", "title"],
    },

    allowedSchemes: [
      "http",
      "https",
      "mailto",
    ],

    allowedSchemesAppliedToAttributes: [
      "href",
    ],

    // scriptなどのタグ内の文字も破棄する。
    nonTextTags: [
      "script",
      "style",
      "textarea",
      "option",
      "noscript",
    ],
  });
}

export default function ArticleDetailPage({
  params,
}: ArticlePageProps) {
  return (
    <Suspense fallback={<ArticleLoading />}>
      <ArticleContent params={params} />
    </Suspense>
  );
}

function ArticleLoading() {
  return (
    <div className="mx-auto max-w-3xl px-6 py-16">
      <p className="text-gray-500">
        記事を読み込んでいます...
      </p>
    </div>
  );
}

async function ArticleContent({
  params,
}: ArticlePageProps) {
  const { slug } = await params;

  const dto = await getPublicArticleById(slug);

  if (!dto) {
    notFound();
  }

  const article = toArticle(dto);

  const richText = isRichText(dto.content);

  // 識別子付きの記事だけをHTMLとして扱う。
  // 表示前に必ずサニタイズする。
  const safeHtml = richText
    ? sanitizeArticleHtml(
        getRichTextHtml(dto.content),
      )
    : "";

  return (
    <article className="mx-auto max-w-3xl px-6 py-16">
      <Link
        href="/articles"
        className="text-sm text-orange-600 hover:underline"
      >
        ← 記事一覧へ戻る
      </Link>

      <header className="mt-8 border-b border-orange-100 pb-8">
        {article.category && (
          <p className="font-semibold text-orange-600">
            {article.category}
          </p>
        )}

        <h1
          className={`text-4xl font-extrabold leading-tight text-[#292929] ${
            article.category ? "mt-4" : ""
          }`}
        >
          {article.title}
        </h1>

        <p className="mt-5 leading-relaxed text-gray-600">
          {article.excerpt}
        </p>

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
        {richText ? (
          <div
            className={
              "break-words text-gray-700 leading-8 " +
              "[&_h1]:my-6 [&_h1]:text-3xl [&_h1]:font-bold " +
              "[&_h2]:my-5 [&_h2]:text-2xl [&_h2]:font-bold " +
              "[&_h3]:my-4 [&_h3]:text-xl [&_h3]:font-bold " +
              "[&_p]:my-4 " +
              "[&_ul]:my-4 [&_ul]:list-disc [&_ul]:pl-6 " +
              "[&_ol]:my-4 [&_ol]:list-decimal [&_ol]:pl-6 " +
              "[&_li]:my-1 " +
              "[&_blockquote]:my-5 [&_blockquote]:border-l-4 " +
              "[&_blockquote]:border-orange-400 " +
              "[&_blockquote]:pl-4 [&_blockquote]:italic " +
              "[&_pre]:my-5 [&_pre]:overflow-x-auto " +
              "[&_pre]:rounded-lg [&_pre]:bg-gray-900 " +
              "[&_pre]:p-4 [&_pre]:text-gray-100 " +
              "[&_code]:font-mono " +
              "[&_a]:text-orange-600 [&_a]:underline"
            }
            dangerouslySetInnerHTML={{
              __html: safeHtml,
            }}
          />
        ) : (
          <div className="whitespace-pre-wrap break-words leading-8 text-gray-700">
            {dto.content}
          </div>
        )}
      </section>

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
