
import { NextRequest, NextResponse } from "next/server";

export async function GET(request: NextRequest) {
  const baseUrl = process.env.FUJIT_NOTES_API_BASE_URL;

  if (!baseUrl) {
    return NextResponse.json(
      { message: "API接続先が設定されていません。" },
      { status: 503 },
    );
  }

  // ブラウザが送信したCookieをASP.NET Coreに転送
  const cookie = request.headers.get("cookie");

  try {
    const upstream = await fetch(
      `${baseUrl.replace(/\/+$/, "")}/api/admin/auth/me`,
      {
        method: "GET",
        headers: cookie ? { Cookie: cookie } : {},
        cache: "no-store",
      },
    );

    if (!upstream.ok) {
      return NextResponse.json(
        { authenticated: false },
        {
          status: upstream.status,
          headers: { "Cache-Control": "no-store" },
        },
      );
    }

    const data = await upstream.json();

    return NextResponse.json(data, {
      headers: { "Cache-Control": "no-store" },
    });
  } catch {
    return NextResponse.json(
      { message: "APIサーバーに接続できません。" },
      { status: 502 },
    );
  }
}
