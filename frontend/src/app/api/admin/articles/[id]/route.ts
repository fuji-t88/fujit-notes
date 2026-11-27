
import {
  NextRequest,
  NextResponse,
} from "next/server";

const COOKIE_NAME = "FujitNotes.Admin";

type RouteContext = {
  params: Promise<{
    id: string;
  }>;
};

type UpdateArticleInput = {
  title: string;
  content: string;
  metadata: {
    category: string | null;
    tags: string[];
  };
};

function getApiBaseUrl(): string | null {
  const value = process.env.FUJIT_NOTES_API_BASE_URL;

  if (!value) {
    return null;
  }

  try {
    const url = new URL(value);

    if (!["http:", "https:"].includes(url.protocol)) {
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

function isValidId(id: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(
    id,
  );
}

function isUpdateInput(
  value: unknown,
): value is UpdateArticleInput {
  if (
    typeof value !== "object" ||
    value === null ||
    !("title" in value) ||
    !("content" in value) ||
    !("metadata" in value)
  ) {
    return false;
  }

  if (
    typeof value.title !== "string" ||
    !value.title.trim() ||
    value.title.length > 200 ||
    typeof value.content !== "string"
  ) {
    return false;
  }

  const metadata = value.metadata;

  if (
    typeof metadata !== "object" ||
    metadata === null ||
    !("category" in metadata) ||
    !("tags" in metadata)
  ) {
    return false;
  }

  return (
    (metadata.category === null ||
      typeof metadata.category === "string") &&
    Array.isArray(metadata.tags) &&
    metadata.tags.length <= 10 &&
    metadata.tags.every(
      (tag: unknown) =>
        typeof tag === "string" && tag.trim().length > 0,
    )
  );
}

function errorResponse(
  message: string,
  status: number,
): NextResponse {
  return NextResponse.json(
    { message },
    { status, headers: { "Cache-Control": "no-store" } },
  );
}

// GET /api/admin/articles/[id]
export async function GET(
  request: NextRequest,
  context: RouteContext,
) {
  const { id } = await context.params;

  if (!isValidId(id)) {
    return errorResponse("記事IDが不正です。", 400);
  }

  const cookie = getAdminCookie(request);

  if (!cookie) {
    return errorResponse("ログインが必要です。", 401);
  }

  const baseUrl = getApiBaseUrl();

  if (!baseUrl) {
    return errorResponse("APIの設定がありません。", 500);
  }

  try {
    const upstream = await fetch(
      `${baseUrl}/api/articles/${id}`,
      {
        method: "GET",
        headers: {
          Cookie: cookie,
        },
        cache: "no-store",
      },
    );

    if (!upstream.ok) {
      return errorResponse(
        "記事を取得できませんでした。",
        upstream.status,
      );
    }

    const article: unknown = await upstream.json();

    return NextResponse.json(article, {
      headers: { "Cache-Control": "no-store" },
    });
  } catch {
    return errorResponse(
      "記事APIに接続できませんでした。",
      502,
    );
  }
}

// PUT /api/admin/articles/[id]
export async function PUT(
  request: NextRequest,
  context: RouteContext,
) {
  const { id } = await context.params;

  if (!isValidId(id)) {
    return errorResponse("記事IDが不正です。", 400);
  }

  const origin = request.headers.get("origin");

  if (!origin || origin !== request.nextUrl.origin) {
    return errorResponse("不正なリクエストです。", 403);
  }

  const contentType =
    request.headers.get("content-type") ?? "";

  if (!contentType.toLowerCase().startsWith("application/json")) {
    return errorResponse("JSON形式で送信してください。", 415);
  }

  const cookie = getAdminCookie(request);

  if (!cookie) {
    return errorResponse("ログインが必要です。", 401);
  }

  const baseUrl = getApiBaseUrl();

  if (!baseUrl) {
    return errorResponse("APIの設定がありません。", 500);
  }

  let input: unknown;

  try {
    input = await request.json();
  } catch {
    return errorResponse("JSON形式が不正です。", 400);
  }

  if (!isUpdateInput(input)) {
    return errorResponse("記事の入力内容が不正です。", 400);
  }

  try {
    const upstream = await fetch(
      `${baseUrl}/api/articles/${id}`,
      {
        method: "PUT",
        headers: {
          Cookie: cookie,
          "Content-Type": "application/json",
        },
        body: JSON.stringify(input),
        cache: "no-store",
      },
    );

    if (!upstream.ok) {
      return errorResponse(
        "記事を更新できませんでした。",
        upstream.status,
      );
    }

    // バックエンドが204 No Contentでも動作するようにする。
    return new NextResponse(null, {
      status: 204,
      headers: {
        "Cache-Control": "no-store",
      },
    });
  } catch {
    return errorResponse(
      "記事APIに接続できませんでした。",
      502,
    );
  }
}
