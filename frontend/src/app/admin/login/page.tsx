
"use client";

import { useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";

export default function AdminLoginPage() {
  const router = useRouter();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    setError("");
    setLoading(true);

    try {
      const response = await fetch("/api/admin/auth/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "same-origin",
        body: JSON.stringify({
          username,
          password,
        }),
      });

      if (!response.ok) {
        if (response.status === 429) {
          setError(
            "ログイン試行回数が上限に達しました。",
          );
        } else {
          setError(
            "ログインに失敗しました。入力内容を確認してください。",
          );
        }

        return;
      }

      setPassword("");

      router.replace("/admin");
      router.refresh();
    } catch {
      setError("サーバーとの通信に失敗しました。");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-[#f8f8f8] px-4 py-12">
      <div className="w-full max-w-md rounded-2xl border border-gray-200 bg-white p-8 shadow-sm">
        <div className="mb-8 text-center">
          <h1 className="text-3xl font-bold tracking-tight">
            <span className="text-[#333333]">
              Fujit
            </span>{" "}
            <span className="text-orange-500">
              Notes
            </span>
          </h1>

          <p className="mt-3 text-sm text-gray-500">
            管理者ログイン
          </p>
        </div>

        <form
          onSubmit={handleSubmit}
          className="space-y-5"
        >
          <div>
            <label
              htmlFor="username"
              className="mb-2 block text-sm font-medium text-gray-700"
            >
              ユーザー名
            </label>

            <input
              id="username"
              type="text"
              autoComplete="username"
              required
              maxLength={100}
              value={username}
              onChange={(event) =>
                setUsername(event.target.value)
              }
              className="w-full rounded-lg border border-gray-300 px-4 py-3 text-gray-900 focus:border-orange-500 focus:outline-none"
            />
          </div>

          <div>
            <label
              htmlFor="password"
              className="mb-2 block text-sm font-medium text-gray-700"
            >
              パスワード
            </label>

            <input
              id="password"
              type="password"
              autoComplete="current-password"
              required
              value={password}
              onChange={(event) =>
                setPassword(event.target.value)
              }
              className="w-full rounded-lg border border-gray-300 px-4 py-3 text-gray-900 focus:border-orange-500 focus:outline-none"
            />
          </div>

          {error && (
            <p
              role="alert"
              className="text-sm text-red-600"
            >
              {error}
            </p>
          )}

          <button
            type="submit"
            disabled={loading}
            className="w-full rounded-lg bg-orange-500 px-4 py-3 font-semibold text-white transition hover:bg-orange-600 disabled:opacity-50"
          >
            {loading
              ? "ログイン中..."
              : "ログイン"}
          </button>
        </form>
      </div>
    </main>
  );
}
