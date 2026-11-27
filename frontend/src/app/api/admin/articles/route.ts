
import { NextRequest, NextResponse } from "next/server";

const COOKIE_NAME = "FujitNotes.Admin";

function getApiBaseUrl(): string | null {
  const url = process.env.FUJIT_NOTES_API_BASE_URL;
  return url ? url.replace(/\/+$/, "") : null;
}

function getAdminCookie(request: NextRequest): string | null {
  const cookie = request.cookies.get(COOKIE_NAME);

  if (!cookie) {
    return null;
  }

  return `${COOKIE_NAME}=${cookie.value}`;
}

function jsonError(message: string, status: number) {
  return NextResponse.json(
    { message },
    {
      status,
      headers: { "Cache-Control": "no-store" },
    },
  );
}

// GET /api/admin/articles
// 管理者向け記事一覧
export async function GET(request: NextRequest) {
  const baseUrl = getApiBaseUrl();

  if (!baseUrl) {
    return jsonError("API接続先が設定されていません。", 503);
  }

  const cookie = getAdminCookie(request);

  if (!cookie) {
    return jsonError("ログインが必要です。", 401);
  }

  try {
    const upstream = await fetch(`${baseUrl}/api/articles`, {
      method: "GET",
      headers: {
        Cookie: cookie,
      },
      cache: "no-store",
    });

    if (!upstream.ok) {
      if (upstream.status === 401) {
        return jsonError("ログインが必要です。", 401);
      }

      if (upstream.status === 403) {
        return jsonError("アクセス権限がありません。", 403);
      }

      return jsonError(
        "記事一覧の取得に失敗しました。",
        upstream.status,
      );
    }

    const articles: unknown = await upstream.json();

    if (!Array.isArray(articles)) {
      return jsonError("記事データの形式が不正です。", 502);
    }

    return NextResponse.json(articles, {
      headers: { "Cache-Control": "no-store" },
    });
  } catch (error) {
    console.error("Admin articles GET error:", error);

    return jsonError(
      "記事一覧を取得できませんでした。",
      502,
    );
  }
}

type CreateArticleInput = {
  title: string;
  content: string;
  metadata: {
    category: string | null;
    tags: string[];
  };
};

function isCreateArticleInput(
  value: unknown,
): value is CreateArticleInput {
  if (typeof value !== "object" || value === null) {
    return false;
  }

  if (
    !("title" in value) ||
    !("content" in value) ||
    !("metadata" in value)
  ) {
    return false;
  }

  if (
    typeof value.title !== "string" ||
    typeof value.content !== "string" ||
    !value.title.trim() ||
    !value.content.trim()
  ) {
    return false;
  }

  const metadata = value.metadata;

  if (typeof metadata !== "object" || metadata === null) {
    return false;
  }

  if (
    !("category" in metadata) ||
    !("tags" in metadata)
  ) {
    return false;
  }

  return (
    (metadata.category === null ||
      typeof metadata.category === "string") &&
    Array.isArray(metadata.tags) &&
    metadata.tags.every(
      (tag: unknown) => typeof tag === "string",
    )
  );
}

// POST /api/admin/articles
// 管理者向け記事作成
export async function POST(request: NextRequest) {
  // 別オリジンからの変更要求を拒否
  const origin = request.headers.get("origin");

  if (!origin || origin !== request.nextUrl.origin) {
    return jsonError(
      "許可されていないリクエストです。",
      403,
    );
  }

  const contentType = request.headers.get("content-type");

  if (
    !contentType ||
    !/^application\/json(?:\s*;|$)/i.test(contentType)
  ) {
    return jsonError(
      "JSON形式で送信してください。",
      415,
    );
  }

  const baseUrl = getApiBaseUrl();

  if (!baseUrl) {
    return jsonError("API接続先が設定されていません。", 503);
  }

  const cookie = getAdminCookie(request);

  if (!cookie) {
    return jsonError("ログインが必要です。", 401);
  }

  let input: unknown;

  try {
    input = await request.json();
  } catch {
    return jsonError("リクエスト形式が不正です。", 400);
  }

  if (!isCreateArticleInput(input)) {
    return jsonError("記事の入力内容が不正です。", 400);
  }

  const payload: CreateArticleInput = {
    title: input.title.trim(),
    content: input.content.trim(),
    metadata: {
      category: input.metadata.category?.trim() || null,
      tags: Array.from(
        new Set(
          input.metadata.tags
            .map((tag) => tag.trim())
            .filter(Boolean),
        ),
      ),
    },
  };

  try {
    const upstream = await fetch(`${baseUrl}/api/articles`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Cookie: cookie,
      },
      body: JSON.stringify(payload),
      cache: "no-store",
    });

    if (!upstream.ok) {
      if (upstream.status === 401) {
        return jsonError("ログインが必要です。", 401);
      }

      if (upstream.status === 403) {
        return jsonError("アクセス権限がありません。", 403);
      }

      return jsonError(
        "記事を保存できませんでした。",
        upstream.status,
      );
    }

    const result: unknown = await upstream.json();

    if (
      typeof result !== "object" ||
      result === null ||
      !("id" in result) ||
      typeof result.id !== "string"
    ) {
      return jsonError(
        "保存結果の形式が不正です。記事一覧を確認してください。",
        502,
      );
    }

    return NextResponse.json(result, {
      status: upstream.status,
      headers: { "Cache-Control": "no-store" },
    });
  } catch (error) {
    console.error("Admin articles POST error:", error);

    return jsonError(
      "記事保存処理を確認できませんでした。記事一覧を確認してください。",
      502,
    );
  }
}
