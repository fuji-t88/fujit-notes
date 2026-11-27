
import { NextRequest, NextResponse } from "next/server";

export async function POST(request: NextRequest) {
  // ブラウザからの同一オリジン要求のみ許可
  const origin = request.headers.get("origin");

  if (!origin || origin !== request.nextUrl.origin) {
    return NextResponse.json(
      { message: "許可されていないリクエストです。" },
      { status: 403 },
    );
  }

  const baseUrl = process.env.FUJIT_NOTES_API_BASE_URL;

  if (!baseUrl) {
    return NextResponse.json(
      { message: "API接続先が設定されていません。" },
      { status: 503 },
    );
  }

  let credentials: unknown;

  try {
    credentials = await request.json();
  } catch {
    return NextResponse.json(
      { message: "リクエスト形式が不正です。" },
      { status: 400 },
    );
  }

  if (
    typeof credentials !== "object" ||
    credentials === null ||
    !("username" in credentials) ||
    !("password" in credentials) ||
    typeof credentials.username !== "string" ||
    typeof credentials.password !== "string" ||
    !credentials.username.trim() ||
    !credentials.password
  ) {
    return NextResponse.json(
      { message: "ユーザー名とパスワードを入力してください。" },
      { status: 400 },
    );
  }

  try {
    const upstream = await fetch(
      `${baseUrl.replace(/\/+$/, "")}/api/admin/auth/login`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          username: credentials.username,
          password: credentials.password,
        }),
        cache: "no-store",
      },
    );

    const response = NextResponse.json(
      {
        authenticated: upstream.ok,
        message: upstream.ok
          ? "ログインしました。"
          : "ログインできませんでした。",
      },
      { status: upstream.status },
    );

    const setCookie = upstream.headers.get("set-cookie");

    if (upstream.ok && setCookie) {
      response.headers.set("Set-Cookie", setCookie);
    }

    response.headers.set("Cache-Control", "no-store");

    return response;
  } catch {
    return NextResponse.json(
      { message: "APIサーバーに接続できません。" },
      { status: 502 },
    );
  }
}
