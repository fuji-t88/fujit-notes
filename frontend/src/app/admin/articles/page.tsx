
"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";

type Article = {
  id: string;
  title: string;
  content: string;
  status: string;
  createdAt: string;
  publishedAt: string | null;
  category: string | null;
  tags: string[];
};

type ArticleAction = "publish" | "archive";

function getStatusLabel(status: string): string {
  switch (status.toLowerCase()) {
    case "draft":
      return "下書き";
    case "published":
      return "公開済み";
    case "archived":
      return "非公開";
    default:
      return status;
  }
}

function getStatusClass(status: string): string {
  switch (status.toLowerCase()) {
    case "draft":
      return "bg-amber-50 text-amber-800";
    case "published":
      return "bg-green-50 text-green-800";
    case "archived":
      return "bg-gray-100 text-gray-700";
    default:
      return "bg-gray-100 text-gray-700";
  }
}

function formatDate(value: string): string {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return "-";
  }

  return date.toLocaleDateString("ja-JP", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });
}

export default function AdminArticlesPage() {
  const router = useRouter();

  const [articles, setArticles] = useState<Article[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [refreshKey, setRefreshKey] = useState(0);
  const [processingId, setProcessingId] = useState<string | null>(
    null,
  );

  useEffect(() => {
    const controller = new AbortController();

    async function loadArticles() {
      setLoading(true);
      setError("");

      try {
        const response = await fetch("/api/admin/articles", {
          cache: "no-store",
          credentials: "same-origin",
          signal: controller.signal,
        });

        if (response.status === 401 || response.status === 403) {
          router.replace("/admin/login");
          return;
        }

        if (!response.ok) {
          throw new Error("記事一覧の取得に失敗しました。");
        }

        const data: unknown = await response.json();

        if (!Array.isArray(data)) {
          throw new Error("記事データの形式が不正です。");
        }

        if (!controller.signal.aborted) {
          setArticles(data as Article[]);
        }
      } catch {
        if (!controller.signal.aborted) {
          setError("記事一覧を取得できませんでした。");
        }
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      }
    }

    void loadArticles();

    return () => controller.abort();
  }, [router, refreshKey]);

  async function handleAction(
    article: Article,
    action: ArticleAction,
  ) {
    if (processingId !== null) {
      return;
    }

    const status = article.status.toLowerCase();

    const canPublish =
      status === "draft" || status === "archived";

    if (action === "publish" && !canPublish) {
      return;
    }

    if (action === "archive" && status !== "published") {
      return;
    }

    const isRepublish = status === "archived";

    const confirmed = window.confirm(
      action === "publish"
        ? isRepublish
          ? `「${article.title}」を再公開しますか？\n\n一般の読者から再び閲覧できるようになります。`
          : `「${article.title}」を公開しますか？\n\n一般の読者から閲覧できるようになります。`
        : `「${article.title}」を非公開にしますか？\n\n公開一覧と一般向けの記事詳細から閲覧できなくなります。`,
    );

    if (!confirmed) {
      return;
    }

    setProcessingId(article.id);
    setError("");
    setNotice("");

    try {
      const response = await fetch(
        `/api/admin/articles/${encodeURIComponent(article.id)}/${action}`,
        {
          method: "POST",
          credentials: "same-origin",
          cache: "no-store",
        },
      );

      if (response.status === 401) {
        router.replace("/admin/login");
        return;
      }

      if (!response.ok) {
        let message =
          `操作に失敗しました。（HTTP ${response.status}）`;

        try {
          const result: unknown = await response.json();

          if (
            typeof result === "object" &&
            result !== null &&
            "message" in result &&
            typeof result.message === "string"
          ) {
            message = result.message;
          }
        } catch {
          // JSON以外のエラーレスポンス
        }

        setError(message);
        return;
      }

      setNotice(
        action === "archive"
          ? `「${article.title}」を非公開にしました。`
          : isRepublish
            ? `「${article.title}」を再公開しました。`
            : `「${article.title}」を公開しました。`,
      );

      setRefreshKey((value) => value + 1);
      router.refresh();
    } catch {
      setError(
        "通信結果を確認できませんでした。再操作する前に記事の状態を確認してください。",
      );
    } finally {
      setProcessingId(null);
    }
  }

  return (
    <main className="mx-auto max-w-6xl px-6 py-12">
      <div className="mb-8">
        <Link
          href="/admin"
          className="text-sm text-orange-600 hover:underline"
        >
          ← ダッシュボードへ戻る
        </Link>

        <div className="mt-5 flex flex-wrap items-center justify-between gap-4">
          <h1 className="text-3xl font-bold text-gray-900">
            記事管理
          </h1>

          <Link
            href="/admin/articles/new"
            className="rounded-lg bg-orange-500 px-5 py-3 font-semibold text-white hover:bg-orange-600"
          >
            ＋ 新規記事作成
          </Link>
        </div>

        <p className="mt-2 text-gray-600">
          下書き・公開済み・非公開記事を管理します。
        </p>
      </div>

      {error && (
        <p
          role="alert"
          className="mb-5 rounded-lg bg-red-50 p-4 text-red-700"
        >
          {error}
        </p>
      )}

      {notice && (
        <p
          role="status"
          className="mb-5 rounded-lg border border-gray-200 p-4 text-gray-800"
        >
          {notice}
        </p>
      )}

      {loading ? (
        <p role="status" className="text-gray-600">
          記事を読み込んでいます...
        </p>
      ) : articles.length === 0 ? (
        <div className="rounded-xl border border-gray-200 bg-white p-10 text-center">
          <h2 className="text-xl font-semibold text-gray-900">
            記事がまだありません
          </h2>

          <p className="mt-3 text-gray-600">
            新規記事作成から最初の記事を作成できます。
          </p>
        </div>
      ) : (
        <div className="overflow-x-auto rounded-xl border border-gray-200 bg-white">
          <table className="w-full min-w-[920px] text-left text-sm">
            <thead className="bg-gray-50 text-gray-700">
              <tr>
                <th className="px-5 py-4">タイトル</th>
                <th className="px-5 py-4">状態</th>
                <th className="px-5 py-4">カテゴリ</th>
                <th className="px-5 py-4">作成日</th>
                <th className="px-5 py-4">操作</th>
              </tr>
            </thead>

            <tbody>
              {articles.map((article) => {
                const status = article.status.toLowerCase();
                const busy = processingId !== null;
                const currentBusy =
                  processingId === article.id;

                return (
                  <tr
                    key={article.id}
                    className="border-t border-gray-100"
                  >
                    <td className="px-5 py-4 font-medium text-gray-900">
                      {article.title}
                    </td>

                    <td className="px-5 py-4">
                      <span
                        className={`inline-block rounded-full px-3 py-1 text-xs font-medium ${getStatusClass(article.status)}`}
                      >
                        {getStatusLabel(article.status)}
                      </span>
                    </td>

                    <td className="px-5 py-4 text-gray-600">
                      {article.category || "未設定"}
                    </td>

                    <td className="px-5 py-4 text-gray-600">
                      {formatDate(article.createdAt)}
                    </td>

                    <td className="px-5 py-4">
                      <div className="flex flex-wrap items-center gap-3">
                        {(status === "draft" ||
                          status === "archived") && (
                          <>
                            <Link
                              href={`/admin/articles/${article.id}/edit`}
                              className="font-semibold text-orange-600 hover:underline"
                            >
                              編集
                            </Link>

                            <button
                              type="button"
                              disabled={busy || loading}
                              onClick={() => {
                                void handleAction(
                                  article,
                                  "publish",
                                );
                              }}
                              className="font-semibold text-green-700 hover:underline disabled:cursor-not-allowed disabled:opacity-40"
                            >
                              {currentBusy
                                ? "処理中..."
                                : status === "archived"
                                  ? "再公開"
                                  : "公開"}
                            </button>
                          </>
                        )}

                        {status === "published" && (
                          <>
                            <Link
                              href={`/articles/${article.id}`}
                              target="_blank"
                              rel="noopener noreferrer"
                              className="font-semibold text-orange-600 hover:underline"
                            >
                              記事を見る
                            </Link>

                            <button
                              type="button"
                              disabled={busy || loading}
                              onClick={() => {
                                void handleAction(
                                  article,
                                  "archive",
                                );
                              }}
                              className="font-semibold text-gray-600 hover:underline disabled:cursor-not-allowed disabled:opacity-40"
                            >
                              {currentBusy
                                ? "処理中..."
                                : "非公開にする"}
                            </button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </main>
  );
}
