
"use client";

import {
  Suspense,
  useEffect,
  useState,
  type FormEvent,
} from "react";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";

import ArticleEditor from "../../../../../features/articles/components/ArticleEditor";

import {
  getRichTextHtml,
  isRichText,
  markRichText,
} from "../../../../../features/articles/lib/articleContent";

type Article = {
  id: string;
  title: string;
  content: string;
  status: string;
  category: string | null;
  tags: string[];
};

const CATEGORIES = [
  "Development",
  "Cloud & DevOps",
  "Career & Learning",
  "Build in Public",
  "Life & Fitness",
];

function escapeHtml(value: string): string {
  return value
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");
}

function toEditableHtml(content: string): string {
  if (isRichText(content)) {
    return getRichTextHtml(content);
  }

  return content
    .split(/\r?\n/)
    .map((line) =>
      line.trim()
        ? `<p>${escapeHtml(line)}</p>`
        : "<p></p>",
    )
    .join("");
}

function hasContent(html: string): boolean {
  return (
    html
      .replace(/<[^>]*>/g, "")
      .replace(/&nbsp;|&#160;/gi, " ")
      .trim().length > 0
  );
}

export default function EditArticlePage() {
  return (
    <Suspense fallback={<EditArticleLoading />}>
      <EditArticleContent />
    </Suspense>
  );
}

function EditArticleLoading() {
  return (
    <main className="mx-auto max-w-4xl px-6 py-12">
      <p role="status" className="text-gray-600">
        記事編集画面を読み込んでいます...
      </p>
    </main>
  );
}

function EditArticleContent() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const id = params.id;

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  const [title, setTitle] = useState("");
  const [category, setCategory] = useState("");
  const [tags, setTags] = useState("");
  const [content, setContent] = useState("");

  const [initialHtml, setInitialHtml] = useState("");
  const [articleLoaded, setArticleLoaded] = useState(false);
  const [canEdit, setCanEdit] = useState(false);
  const [wasPrivate, setWasPrivate] = useState(false);

  useEffect(() => {
    const controller = new AbortController();

    async function loadArticle() {
      try {
        const response = await fetch(
          `/api/admin/articles/${encodeURIComponent(id)}`,
          {
            cache: "no-store",
            credentials: "same-origin",
            signal: controller.signal,
          },
        );

        if (response.status === 401) {
          router.replace("/admin/login");
          return;
        }

        if (response.status === 403) {
          setError("記事を参照する権限がありません。");
          return;
        }

        if (!response.ok) {
          throw new Error("記事の取得に失敗しました。");
        }

        const article = (await response.json()) as Article;

        if (controller.signal.aborted) {
          return;
        }

        const html = toEditableHtml(article.content);
        const status = article.status.toLowerCase();

        setTitle(article.title);
        setCategory(article.category ?? "");
        setTags(article.tags.join(", "));
        setContent(html);
        setInitialHtml(html);

        setCanEdit(
          status === "draft" || status === "archived",
        );
        setWasPrivate(status === "archived");
        setArticleLoaded(true);
      } catch {
        if (!controller.signal.aborted) {
          setError("記事を取得できませんでした。");
        }
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      }
    }

    void loadArticle();

    return () => controller.abort();
  }, [id, router]);

  async function handleSave(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (saving || !canEdit) {
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
      const response = await fetch(
        `/api/admin/articles/${encodeURIComponent(id)}`,
        {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
          },
          credentials: "same-origin",
          body: JSON.stringify({
            title: title.trim(),
            content: markRichText(content),
            metadata: {
              category: category || null,
              tags: tagList,
            },
          }),
        },
      );

      if (response.status === 401) {
        router.replace("/admin/login");
        return;
      }

      if (response.status === 403) {
        setError("記事を更新する権限がありません。");
        return;
      }

      if (!response.ok) {
        setError(
          `記事を更新できませんでした。（HTTP ${response.status}）`,
        );
        return;
      }

      router.push("/admin/articles");
      router.refresh();
    } catch {
      setError(
        "通信結果を確認できませんでした。再送信前に記事一覧を確認してください。",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <main className="mx-auto max-w-4xl px-6 py-12">
      <Link
        href="/admin/articles"
        className="text-sm text-orange-600 hover:underline"
      >
        ← 記事一覧へ戻る
      </Link>

      <h1 className="mt-6 text-3xl font-bold text-gray-900">
        記事編集
      </h1>

      <p className="mt-3 text-gray-600">
        下書き・非公開記事を編集できます。
      </p>

      {loading ? (
        <p role="status" className="mt-8 text-gray-600">
          記事を読み込んでいます...
        </p>
      ) : error && !articleLoaded ? (
        <p role="alert" className="mt-8 text-red-600">
          {error}
        </p>
      ) : !canEdit ? (
        <p className="mt-8 rounded-lg bg-amber-50 p-4 text-amber-800">
          公開済みの記事は直接編集できません。
          非公開にしてから編集してください。
        </p>
      ) : (
        <form onSubmit={handleSave} className="mt-8 space-y-6">
          {wasPrivate && (
            <p className="rounded-lg border border-gray-200 bg-gray-50 p-4 text-gray-700">
              この記事は現在、非公開です。
              編集・保存しても自動的に公開されません。
              記事一覧から「再公開」を選んでください。
            </p>
          )}

          <div>
            <label
              htmlFor="edit-title"
              className="mb-2 block font-semibold text-gray-800"
            >
              タイトル
            </label>

            <input
              id="edit-title"
              required
              maxLength={200}
              value={title}
              onChange={(event) =>
                setTitle(event.target.value)
              }
              className="w-full rounded-lg border border-gray-300 p-3 text-gray-900 focus:border-orange-500 focus:outline-none"
            />
          </div>

          <div>
            <label
              htmlFor="edit-category"
              className="mb-2 block font-semibold text-gray-800"
            >
              カテゴリ
            </label>

            <select
              id="edit-category"
              value={category}
              onChange={(event) =>
                setCategory(event.target.value)
              }
              className="w-full rounded-lg border border-gray-300 bg-white p-3 text-gray-900"
            >
              <option value="">未設定</option>

              {CATEGORIES.map((value) => (
                <option key={value} value={value}>
                  {value}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label
              htmlFor="edit-tags"
              className="mb-2 block font-semibold text-gray-800"
            >
              タグ
            </label>

            <input
              id="edit-tags"
              value={tags}
              onChange={(event) =>
                setTags(event.target.value)
              }
              placeholder="Next.js, Tiptap"
              className="w-full rounded-lg border border-gray-300 p-3 text-gray-900"
            />

            <p className="mt-2 text-sm text-gray-500">
              カンマ区切り、最大10件です。
            </p>
          </div>

          <div>
            <p
              id="edit-body-label"
              className="mb-2 font-semibold text-gray-800"
            >
              本文
            </p>

            <Suspense
              fallback={
                <div
                  role="status"
                  className="min-h-[360px] rounded-xl border border-gray-300 p-5"
                >
                  エディタを準備しています...
                </div>
              }
            >
              <div aria-labelledby="edit-body-label">
                <ArticleEditor
                  initialContent={initialHtml}
                  onChange={setContent}
                />
              </div>
            </Suspense>
          </div>

          {error && (
            <p
              role="alert"
              className="rounded-lg bg-red-50 p-4 text-red-700"
            >
              {error}
            </p>
          )}

          <div className="flex flex-wrap justify-end gap-3">
            <Link
              href="/admin/articles"
              className="rounded-lg border border-gray-300 px-5 py-3 font-semibold text-gray-700"
            >
              キャンセル
            </Link>

            <button
              type="submit"
              disabled={saving}
              className="rounded-lg bg-orange-500 px-6 py-3 font-semibold text-white hover:bg-orange-600 disabled:opacity-50"
            >
              {saving ? "保存中..." : "変更を保存"}
            </button>
          </div>
        </form>
      )}
    </main>
  );
}
