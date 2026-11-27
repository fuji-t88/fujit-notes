
"use client";

import {
  Suspense,
  useState,
  type FormEvent,
} from "react";

import Link from "next/link";
import { useRouter } from "next/navigation";

import ArticleEditor from "../../../../features/articles/components/ArticleEditor";
import { markRichText } from "../../../../features/articles/lib/articleContent";

const ARTICLE_CATEGORIES = [
  {
    value: "Development",
    label: "Development — 技術開発",
  },
  {
    value: "Cloud & DevOps",
    label: "Cloud & DevOps — インフラ・クラウド",
  },
  {
    value: "Career & Learning",
    label: "Career & Learning — キャリア・学習",
  },
  {
    value: "Build in Public",
    label: "Build in Public — 個人開発・制作記録",
  },
  {
    value: "Life & Fitness",
    label: "Life & Fitness — 生活・フィットネス",
  },
] as const;

export default function NewArticlePage() {
  const router = useRouter();

  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [category, setCategory] = useState("");
  const [tags, setTags] = useState("");

  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  function hasContent(html: string): boolean {
    return html
      .replace(/<[^>]*>/g, "")
      .replace(/&nbsp;|&#160;/gi, " ")
      .trim().length > 0;
  }

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (saving) {
      return;
    }

    setError("");

    if (!title.trim()) {
      setError("タイトルを入力してください。");
      return;
    }

    if (!hasContent(content)) {
      setError("本文を入力してください。");
      return;
    }

    const tagList = Array.from(
      new Set(
        tags
          .split(",")
          .map((tag) => tag.trim())
          .filter(Boolean),
      ),
    );

    if (tagList.length > 10) {
      setError("タグは最大10件までです。");
      return;
    }

    setSaving(true);

    try {
      const response = await fetch("/api/admin/articles", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "same-origin",
        body: JSON.stringify({
          title: title.trim(),

          // 新形式だと判別できるようマーカーを付ける。
          content: markRichText(content),

          metadata: {
            category: category || null,
            tags: tagList,
          },
        }),
      });

      if (response.status === 401) {
        router.replace("/admin/login");
        return;
      }

      if (response.status === 403) {
        setError("記事を作成する権限がありません。");
        return;
      }

      if (!response.ok) {
        setError(
          `記事を保存できませんでした。（HTTP ${response.status}）`,
        );
        return;
      }

      const result: unknown = await response.json();

      if (
        typeof result !== "object" ||
        result === null ||
        !("id" in result) ||
        typeof result.id !== "string"
      ) {
        setError(
          "保存結果を確認できませんでした。記事一覧を確認してください。",
        );
        return;
      }

      router.push("/admin/articles");
      router.refresh();
    } catch {
      setError(
        "通信結果を確認できませんでした。再送信する前に記事一覧を確認してください。",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <main className="mx-auto max-w-4xl px-6 py-12">
      <Link
        href="/admin/articles"
        className="text-sm font-medium text-orange-600 hover:underline"
      >
        ← 記事一覧へ戻る
      </Link>

      <div className="mb-8 mt-6">
        <p className="text-sm font-semibold text-orange-600">
          FUJIT NOTES / ADMIN
        </p>

        <h1 className="mt-2 text-3xl font-bold text-gray-900">
          新規記事作成
        </h1>

        <p className="mt-3 text-gray-600">
          Tiptapエディタで記事を作成し、
          下書きとして保存します。
        </p>
      </div>

      <form
        onSubmit={handleSubmit}
        className="space-y-6"
      >
        <div>
          <label
            htmlFor="title"
            className="mb-2 block font-semibold text-gray-800"
          >
            タイトル
          </label>

          <input
            id="title"
            type="text"
            required
            maxLength={200}
            value={title}
            onChange={(event) =>
              setTitle(event.target.value)
            }
            placeholder="記事タイトルを入力してください"
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-3 text-gray-900 focus:border-orange-500 focus:outline-none"
          />
        </div>

        <div>
          <label
            htmlFor="category"
            className="mb-2 block font-semibold text-gray-800"
          >
            カテゴリ
          </label>

          <select
            id="category"
            value={category}
            onChange={(event) =>
              setCategory(event.target.value)
            }
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-3 text-gray-900 focus:border-orange-500 focus:outline-none"
          >
            <option value="">
              カテゴリを選択（未設定も可）
            </option>

            {ARTICLE_CATEGORIES.map((item) => (
              <option
                key={item.value}
                value={item.value}
              >
                {item.label}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label
            htmlFor="tags"
            className="mb-2 block font-semibold text-gray-800"
          >
            タグ
          </label>

          <input
            id="tags"
            type="text"
            value={tags}
            onChange={(event) =>
              setTags(event.target.value)
            }
            placeholder="Next.js, TypeScript, C#"
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-3 text-gray-900 focus:border-orange-500 focus:outline-none"
          />

          <p className="mt-2 text-sm text-gray-500">
            カンマ区切りで最大10件まで指定できます。
          </p>
        </div>

        <div>
          <p
            id="article-content-label"
            className="mb-2 font-semibold text-gray-800"
          >
            本文
          </p>

          <Suspense
            fallback={
              <div
                role="status"
                className="min-h-[360px] rounded-xl border border-gray-300 bg-white p-5 text-gray-500"
              >
                エディタを読み込んでいます...
              </div>
            }
          >
            <div aria-labelledby="article-content-label">
              <ArticleEditor
                initialContent=""
                onChange={setContent}
              />
            </div>
          </Suspense>

          <p className="mt-2 text-sm text-gray-500">
            見出し、太字、リスト、コードブロックなどを
            使用できます。
          </p>
        </div>

        {error && (
          <p
            role="alert"
            className="rounded-lg bg-red-50 p-4 text-sm text-red-700"
          >
            {error}
          </p>
        )}

        <div className="flex flex-wrap justify-end gap-3">
          <Link
            href="/admin/articles"
            className="rounded-lg border border-gray-300 px-6 py-3 font-semibold text-gray-700 hover:bg-gray-50"
          >
            キャンセル
          </Link>

          <button
            type="submit"
            disabled={saving}
            className="rounded-lg bg-orange-500 px-6 py-3 font-semibold text-white hover:bg-orange-600 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {saving ? "保存中..." : "下書きを保存"}
          </button>
        </div>
      </form>
    </main>
  );
}
