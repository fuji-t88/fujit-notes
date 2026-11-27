
"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";

type AdminUser = {
  authenticated: boolean;
  username: string;
};

export default function AdminPage() {
  const router = useRouter();

  const [user, setUser] = useState<AdminUser | null>(null);
  const [loading, setLoading] = useState(true);
  const [loggingOut, setLoggingOut] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;

    async function checkAuthentication() {
      try {
        const response = await fetch("/api/admin/auth/me", {
          cache: "no-store",
          credentials: "same-origin",
        });

        if (response.status === 401 || response.status === 403) {
          router.replace("/admin/login");
          return;
        }

        if (!response.ok) {
          throw new Error("認証状態を確認できませんでした。");
        }

        const data = (await response.json()) as AdminUser;

        if (!data.authenticated) {
          router.replace("/admin/login");
          return;
        }

        if (active) {
          setUser(data);
        }
      } catch {
        if (active) {
          setError("認証状態の確認に失敗しました。");
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    void checkAuthentication();

    return () => {
      active = false;
    };
  }, [router]);

  async function handleLogout() {
    setLoggingOut(true);
    setError("");

    try {
      const response = await fetch("/api/admin/auth/logout", {
        method: "POST",
        credentials: "same-origin",
      });

      if (!response.ok) {
        throw new Error("ログアウトできませんでした。");
      }

      router.replace("/admin/login");
      router.refresh();
    } catch {
      setError("ログアウトに失敗しました。");
    } finally {
      setLoggingOut(false);
    }
  }

  if (loading) {
    return (
      <main className="mx-auto max-w-5xl px-6 py-16">
        <p>認証状態を確認しています...</p>
      </main>
    );
  }

  if (!user) {
    return (
      <main className="mx-auto max-w-5xl px-6 py-16">
        <p role="alert">
          {error || "ログイン画面へ移動します..."}
        </p>
      </main>
    );
  }

  return (
    <main className="mx-auto max-w-5xl px-6 py-12">
      <div className="mb-10 flex flex-wrap items-center justify-between gap-4">
        <div>
          <p className="text-sm font-semibold text-orange-600">
            FUJIT NOTES / ADMIN
          </p>

          <h1 className="mt-2 text-3xl font-bold text-gray-900">
            管理者ダッシュボード
          </h1>

          <p className="mt-3 text-gray-600">
            ようこそ、{user.username} さん
          </p>
        </div>

        <button
          type="button"
          onClick={handleLogout}
          disabled={loggingOut}
          className="rounded-lg border border-gray-300 px-5 py-3 font-semibold text-gray-700 hover:bg-gray-100 disabled:opacity-50"
        >
          {loggingOut ? "ログアウト中..." : "ログアウト"}
        </button>
      </div>

      {error && (
        <p role="alert" className="mb-6 text-red-600">
          {error}
        </p>
      )}

      <div className="grid gap-5 md:grid-cols-2">
        <section className="rounded-xl border border-gray-200 bg-white p-6">
          <h2 className="text-xl font-bold text-gray-900">
            記事管理
          </h2>
          <p className="mt-3 text-gray-600">
            記事一覧・編集・公開機能を追加予定です。
          </p>
        </section>

        <section className="rounded-xl border border-gray-200 bg-white p-6">
          <h2 className="text-xl font-bold text-gray-900">
            新規記事
          </h2>
          <p className="mt-3 text-gray-600">
            記事エディタを追加予定です。
          </p>
        </section>
      </div>
    </main>
  );
}
