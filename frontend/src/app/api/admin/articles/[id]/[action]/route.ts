
import {
  NextRequest,
  NextResponse,
} from "next/server";

const COOKIE_NAME = "FujitNotes.Admin";

type RouteContext = {
  params: Promise<{
    id: string;
    action: string;
  }>;
};

function errorResponse(
  message: string,
  status: number,
): NextResponse {
  return NextResponse.json(
    { message },
    {
      status,
      headers: {
        "Cache-Control": "no-store",
      },
    },
  );
}

function isValidId(id: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(
    id,
  );
}

function getApiBaseUrl(): string | null {
  const value = process.env.FUJIT_NOTES_API_BASE_URL;

  if (!value) {
    return null;
  }

  try {
    const url = new URL(value);

    if (url.protocol !== "http:" && url.protocol !== "https:") {
      return null;
    }

    return url.origin;
  } catch {
    return null;
  }
}

function getAdminCookie(
  request: NextRequest,
): string | null {
  const value = request.cookies.get(COOKIE_NAME)?.value;

  if (!value) {
    return null;
  }

  return `${COOKIE_NAME}=${encodeURIComponent(value)}`;
}

// POST /api/admin/articles/[id]/publish
// POST /api/admin/articles/[id]/archive
export async function POST(
  request: NextRequest,
  context: RouteContext,
) {
  const { id, action } = await context.params;

  if (!isValidId(id)) {
    return errorResponse("記事IDが不正です。", 400);
  }

  // 操作対象をホワイトリストで制限
  if (action !== "publish" && action !== "archive") {
    return errorResponse("未対応の操作です。", 404);
  }

  // 別サイトからのCookie付き操作を防ぐ
  const origin = request.headers.get("origin");

  if (!origin || origin !== request.nextUrl.origin) {
    return errorResponse("不正なリクエストです。", 403);
  }

  const cookie = getAdminCookie(request);

  if (!cookie) {
    return errorResponse("ログインが必要です。", 401);
  }

  const baseUrl = getApiBaseUrl();

  if (!baseUrl) {
    return errorResponse("記事APIの設定がありません。", 500);
  }

  try {
    const upstream = await fetch(
      `${baseUrl}/api/articles/${id}/${action}`,
      {
        method: "POST",
        headers: {
          Cookie: cookie,
        },
        cache: "no-store",
      },
    );

    if (!upstream.ok) {
      // バックエンド内部のエラー詳細は公開しない
      if (upstream.status === 401) {
        return errorResponse("ログインが必要です。", 401);
      }

      if (upstream.status === 403) {
        return errorResponse("操作する権限がありません。", 403);
      }

      if (upstream.status === 404) {
        return errorResponse("記事が見つかりません。", 404);
      }

      if (upstream.status === 400 || upstream.status === 409) {
        return errorResponse(
          "現在の記事の状態では、この操作を実行できません。",
          upstream.status,
        );
      }

      return errorResponse(
        "記事の状態を変更できませんでした。",
        upstream.status,
      );
    }

    // バックエンドの成功レスポンス本文に依存しない
    return new NextResponse(null, {
      status: 204,
      headers: {
        "Cache-Control": "no-store",
      },
    });
  } catch {
    return errorResponse(
      "記事APIに接続できませんでした。状態を確認してください。",
      502,
    );
  }
}
